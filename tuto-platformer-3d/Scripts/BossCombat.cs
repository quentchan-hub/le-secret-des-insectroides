using Godot;
using System;

public partial class BossCombat : Node3D
{
	/*
		 NOTE DE CHOIX DE DESIGN — Mitraillage & Zone de feu

		 La Logique de Dégâts (application des dommages au joueur)
		 est gérée par le code.

		 La Partie Cosmétique (animation du boss, tirs, FX, activation
		 de la zone de feu) est pilotée par l'AnimationPlayer.

		 Le Monitoring de la FireZoneArea qui relie la Logique de Dégat à 
		 la Partie Cosmétique est animé directement dans l'AnimationPlayer
		 afin de garantir une synchronisation parfaite avec l'animation visuelle.
		 
		 Le code peut néanmoins forcer l'activation ou la désactivation
		 de la FireZoneArea si nécessaire (override possible).
	*/
	
	
	// ============================================
	// SIGNAUX
	// ============================================
	
	[Signal] public delegate void BossFightStartedEventHandler();
	[Signal] public delegate void CombatFiniEventHandler();
	
	// ============================================
	// VARIABLES EXPORTÉES (modifiables dans l'éditeur)
	// ============================================
	
	// < Paramètres Boss >
	[Export] public int maxHealth = 16; 				// PV maximum du boss
	[Export] public float hauteurVolBoss = 5f;			// Hauteur de vol au-dessus du sol
	
	// < Charge >
	[Export] private float chargeSpeed = 5f;			// Vitesse de Charge VERS Joueur
	[Export] private float retreatSpeed = 4f;			// Vitesse de Charge au RECUL
	
	// < Coup Special Bourrasque >
	[Export] float windForce = 15.0f; 					// Force Bourrasque
	[Export] private Timer timerStartBourrasque; 		// Temps préparation Bourrasque
	[Export] private Timer timerEndBourrasque;			// Fin de Bourrasque
	
	// < Barrel Roll anti-campement >
	[Export] Node3D bossCoxane; 							// Centre de gravité du tonneau 
	
	// < Feedback Dommage sur Boss >
	[Export] public Node3D weakPointVisual;				// Cercle blanc + croix sur le dos
	[Export] public MeshInstance3D bossMesh; 			// Mesh principal du boss (pour le flash blanc)
	
	// < FeedBack Stun -> Etoiles >
	[Export] public PackedScene stunStarScene; 			// Scène de l'étoile à instancier
	[Export] public Marker3D stunStarsSpawnPoint; 		// Point d'apparition des étoiles (au-dessus tête)
	
	// < Attaque Mitraillage >
	[Export] public GpuParticles3D propulseurGauche; 	// Particules du propulseur gauche
	[Export] public GpuParticles3D propulseurDroit; 	// Particules du propulseur droit
	
	[Export] public Area3D fireZoneArea;				// AOE Feu : Monitoring géré via AnimationPlayer
														// pour synchro avec l'animation et les FX.
	[Export] private Timer fireDotTimer;				// DOT infligé sur l'AOE feu
		
	// < Animations >
	[Export] public AnimationPlayer bossAnimPlayer; 	// Animations BOSS + Mitraillage
	[Export] public AnimationPlayer CombatAnimPlayer; 	// Animation Explosion bombe
	[Export] public AnimationPlayer weakPointPlayer; 	// Affichage et animation point faible
	[Export] public AnimationPlayer uIAnimPlayer; 		// Animations Touches claviers
	
	// < UI >
	[Export] public BossHealthBar bossHealthBar;
	[Export] public Control uICombat;
	[Export] public Control controlInventaire;
	[Export] public Label warningUnderFire;				// Feedback impact FireZone sur player
	[Export] public TextureRect popUpBossWeak;
	
	// < Références au joueur >
	[Export] public PackedScene player;
	public PlayerBotCtrl playerScript;
	
	// < Autres >
	public GameState gameState;
	public SoundManager soundManager;
	
	

	// ============================================
	// VARIABLES INTERNES
	// ============================================
	
	private int currentHealth; // PV actuels
	private Vector3 initialPosition; // Position de départ (pour le retour après stun)
	private float initialRotationY; // Rotation Y de départ (pour la bourrasque)
	
	// < État du boss (State Machine) >
	private enum BossState
	{
		Idle,
		Attacking,
		TakingOff,
		Mitraillaging,
		Bourrasquing,
		Landing,
		Stunned,
		Dead
	}
	
	private BossState currentState = BossState.Idle;
	
	// < Phase du combat (1 ou 2) >
	private int currentPhase = 1;
	
	// Possibilité de dommage sur Boss
	public bool CanTakeDamage { get; private set; }
	
	// Vulnérabilité au stun (true quand en l'air)
	public bool isVulnerableToStun = false; 		// Public pour BossWeakPoint
	
	// Position joueur au début de la charge
	private Vector3 chargeTargetPosition;
	
	// Timer pour gérer les cycles d'attaques
	private float stateTimer = 0f;
	
	// Compteur d'étapes dans le cycle
	private int cycleStep = 0;
	
	// Étoiles de stun instanciées
	private Node3D[] stunStars = new Node3D[5];
	
	// État de vol du boss
	private bool isFlying = false; 					// Boss en vol ?
	private float groundY; 							// Position Y au sol (point retour après vol)
	
	// Matériau pour le flash blanc (override temporaire)
	private StandardMaterial3D flashMaterial;
	
	// Etat du personnage, s'il vient de se prendre un dommage -> playerIsHurt = true
	private bool _playerIsHurt = false;
	
	private bool bombUsed = false;
	
	// Sécurisation anti-boucle point faible
	private bool weakPointGlowing;
	
	// Utilisation ciblé du fouet
	private bool fouetClignote = false;

	// ============================================
	// INITIALISATION
	// ============================================
	
	public override void _Ready()
	{
		playerScript = GetNode<PlayerBotCtrl>("/root/World1/PlayerBotCtrl");
		gameState = GetNode<GameState>("/root/GameState");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
		//on check si player vient de se faire taper dessus ? Oui = true
		_playerIsHurt = false;
		
		// Stocker la position et rotation initiales
		initialPosition = GlobalPosition;
		initialRotationY = RotationDegrees.Y;
		groundY = GlobalPosition.Y; // Sauvegarder la hauteur au sol
		
		// Initialiser les PV
		currentHealth = maxHealth;
		
		// Créer le matériau de flash blanc (override temporaire)
		flashMaterial = new StandardMaterial3D();
		flashMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		flashMaterial.AlbedoColor = new Color(10f, 10f, 10f, 1f); // Blanc super brillant
		
		// Cacher le point faible au début
		//if (weakPointVisual != null)
			//weakPointVisual.Visible = false;
		weakPointGlowing = false;
		
		//Par sécurité on va couper les canons au démarrage
		propulseurGauche.Emitting = false;
		propulseurDroit.Emitting = false;
		
		warningUnderFire.Visible = false;
	}
	
	public void Activate()
	{
		Visible = true;
		controlInventaire.Visible = true;
		ProcessMode = ProcessModeEnum.Inherit;
		uICombat.Visible = true;
		EmitSignal(SignalName.BossFightStarted);
		GD.Print("UI activée");
	}
	
	public void Deactivate()
	{
		uICombat.Visible = false;
		controlInventaire.Visible = false;
		ProcessMode = ProcessModeEnum.Disabled;
		Visible = false;
	}
	
	// ============================================
	// BOUCLE PRINCIPALE
	// ============================================
	
	public override void _Process(double delta)
	{
		if (currentState == BossState.Dead || currentState == BossState.Stunned)
		{
			if (weakPointVisual != null)
				weakPointVisual.Visible = false;
			return;
		}
		
		  /////////////////////////////////////////////////
		// GESTION DE LA FENETRE DE VULNERABILITE DU BOSS //
		//////////////////////////////////////////////////
		
				// > Dommages via WeakPoint
		
		if (currentState == BossState.Attacking)
		{
			CanTakeDamage = stateTimer <= 2f;
		}
		else if (currentState == BossState.Mitraillaging || currentState == BossState.Bourrasquing)
		{
			CanTakeDamage = true;
		}
		else
		{
			CanTakeDamage = false;
		}
		
		if (CanTakeDamage && !weakPointGlowing)
		{
			weakPointGlowing = true;
			weakPointPlayer.Play("WeakPointGlowing");
		}
		else if (!CanTakeDamage && weakPointGlowing)
		{
			weakPointPlayer.Stop();
			weakPointGlowing = false;
		}
		
		if (gameState.aLaBomba && Input.IsActionJustPressed("use_special_item"))
		{
			if (bombUsed)
				return;
			else
			{
				bombUsed = true;
				currentHealth -= 6;
				bossHealthBar.UpdateBossLife(currentHealth);
				gameState.aLaBomba = false;
				CombatAnimPlayer.Play("Explosion");
				GD.Print("Bomba Explosa !");
			}

		}

		 ///////////////////////////////////////////////////////////
		///////////////////////////////////////////////////////////
		
		// Décompter le timer d'état
		stateTimer -= (float)delta;
		
		// Si le timer est écoulé, passer à l'étape suivante du cycle
		if (stateTimer <= 0)
		{
			if (currentPhase == 1)
				ExecutePhase1Step();
			else
				ExecutePhase2Step();
		}
		
	}
	

	
	public override void _PhysicsProcess(double delta)
	{
		// Gérer le vol (montée et descente)
		if (isFlying)
		{
			// Monter progressivement vers la hauteur de vol
			float targetY = groundY + hauteurVolBoss;
			float currentY = GlobalPosition.Y;
			
			if (currentY < targetY - 0.1f)
			{
				// Monter à 3 m/s
				Vector3 pos = GlobalPosition;
				pos.Y += 3f * (float)delta;
				GlobalPosition = pos;
			}
		}
		else
		{
			// Redescendre progressivement vers le sol
			float currentY = GlobalPosition.Y;
			
			if (currentY > groundY + 0.1f)
			{
				// Descendre à 3 m/s
				Vector3 pos = GlobalPosition;
				pos.Y -= 5f * (float)delta;
				
				// Ne pas descendre en dessous du sol
				if (pos.Y < groundY)
					pos.Y = groundY;
					
				GlobalPosition = pos;
			}
		}
		
		// Gérer le mouvement de charge (pendant les 2 premières secondes au lieu de 3)
		if (currentState == BossState.Attacking)
		{
			// Vérifier qu'on est dans la première moitié de l'animation (2s sur 4s)
			if (stateTimer > 2.5f)
			{
				UpdateChargeMovement(delta);
			}
			// Si on est dans la seconde moitié (stateTimer <= 2.5s), le boss recule
			// vers sa position initiale
			else
			{
				// Calculer la direction vers la position initiale
				Vector3 directionHome = (initialPosition - GlobalPosition);
				directionHome.Y = 0;
				
				// Si on est proche de la position initiale, arrêter le mouvement
				if (directionHome.Length() < 0.5f)
					return;
				
				directionHome = directionHome.Normalized();
				
				// Reculer vers la position initiale (plus rapide : 5 m/s)
				//float retreatSpeed = 4f; // A l'export
				GlobalPosition += directionHome * retreatSpeed * (float)delta;
			}
			
			if (fouetClignote && Input.IsActionJustPressed("use_special_item"))
			{
				uIAnimPlayer.Stop();
			}
		}
	}
	// ============================================
	// GESTION DES DÉGÂTS SUR LE JOUEUR
	// ============================================
	
	public void _on_area_dmg_mandib_r_body_entered(Node3D body)
	{
		if (currentState == BossState.Attacking)
		{
			if (body is PlayerBotCtrl && !_playerIsHurt)
			{
				playerScript.TakeDamages();
				_playerIsHurt = true;
			
				// pour éviter de faire double impact avec les 2 mandibules
				GetTree().CreateTimer(1.0f).Timeout += () => _playerIsHurt = false;
			}
		}
		
	}
	
	public void _on_area_dmg_mandib_l_body_entered(Node3D body)
	{
		if (currentState == BossState.Attacking)
		{
			if (body is PlayerBotCtrl && !_playerIsHurt)
			{
				playerScript.TakeDamages();
				_playerIsHurt = true;
				GetTree().CreateTimer(1.0f).Timeout += () => _playerIsHurt = false;
			}
		}
	}
	
	// Applique les dégâts au joueur lorsqu'il entre dans la zone de feu.
	// La validité de la zone est contrôlée par l'AnimationPlayer (Monitoring).
	public void _on_fire_zone_area_body_entered(Node3D body)
	{
		if (currentState == BossState.Mitraillaging)
		{
			if (body is PlayerBotCtrl)
			{
				////A décommenter pour infliger 1 dommage puis DOT 1s après, 
				//// sinon c'est DOT au bout d'1s (grace period)
				//_on_fire_dot_timer_timeout()					 
				fireDotTimer.Start();
			}
		}
	}
	
	public void _on_fire_zone_area_body_exited(Node3D body)
	{
		if (body is PlayerBotCtrl)
		{
			fireDotTimer.Stop();
		}
	}
	
	private void _on_fire_dot_timer_timeout()
	{
		if (currentState != BossState.Mitraillaging)
		{
			fireDotTimer.Stop();
			return;
		}

		playerScript.TakeDamages();
		warningUnderFire.Visible = true;
		GetTree().CreateTimer(1.0f).Timeout += () => warningUnderFire.Visible = false;
	}
	


	// ============================================
	// GESTION DES DÉGÂTS SUR LE BOSS
	// ============================================
	
	// Appelé par <BossWeakPoint> quand joueur touche le point faible
	public void BossTakeDamage(int damage, bool shouldStun)
	{
		currentHealth -= damage;						// Réduire les PV
		GD.Print($"{damage} dégâts sur Boss !");
		GD.Print($"PV Boss : {currentHealth}");
		
		if (bossHealthBar != null)						// Mettre à jour la barre de vie
			bossHealthBar.UpdateBossLife(currentHealth);
		
		FlashWhite();									// Feedback visuel : flash blanc
		soundManager.PlayBossDamaged();					// Jouer le son d'impact
		
		if (shouldStun && isVulnerableToStun)			// Si le boss doit être stun (touché en l'air)
		{
			ApplyStun();
		}
		
		GD.Print($"HP = {currentHealth} | aLeFouet = {gameState.aLeFouet}");
		
		if (currentHealth <= 3 && gameState.aLeFouet && !fouetClignote)	// Momento Fouet
		{
			fouetClignote = true;
			uIAnimPlayer.Play("ToucheFouetClignote");
			GD.Print("ToucheFouetClignote");
		}
		
		if (currentHealth <= 0)							// Décès du Boss
		{
			Die();
			return;
		}
		
		if (currentHealth <= 8 && currentPhase == 1)	// Passage en Phase 2
		{
			TransitionToPhase2();
		}
	}
	
	// Flash blanc sur le boss (Surface Material Override)
	private async void FlashWhite()
	{
		if (bossMesh == null)
		{
			GD.PrintErr("Boss Mesh non assigné !");
			return;
		}
		
		GD.Print("Flash blanc déclenché !");
		
		// Appliquer le matériau blanc sur TOUTES les surfaces du mesh
		int surfaceCount = bossMesh.GetSurfaceOverrideMaterialCount();
		
		for (int i = 0; i < surfaceCount; i++)
		{
			bossMesh.SetSurfaceOverrideMaterial(i, flashMaterial);
		}
		
		// Attendre 0.15 seconde
		await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
		
		// Retirer tous les overrides (revenir aux matériaux normaux)
		if (IsInstanceValid(this) && IsInstanceValid(bossMesh))
		{
			for (int i = 0; i < surfaceCount; i++)
			{
				bossMesh.SetSurfaceOverrideMaterial(i, null);
			}
		}
	}
	
	
	// ============================================
	// SYSTÈME DE STUN
	// ============================================
	
	private async void ApplyStun()
	{
		GD.Print("Boss stunned!");
		
		// pour éviter doublon de stun
		if (currentState == BossState.Stunned)
			return;
		
		// Reset le timer du cycle (+ blocage au process = cycle repart de 0 APRES Stun)
		stateTimer = 3f;
		cycleStep = 0;
		
		// Passer en état Stunned
		isVulnerableToStun = false;
		
		// Arrêter l'animation en cours
		bossAnimPlayer.Stop();
		
		// Désactiver les effets visuels
		if (propulseurGauche != null) propulseurGauche.Emitting = false;
		if (propulseurDroit != null) propulseurDroit.Emitting = false;
		
		//Désactiver la Firezone
		fireZoneArea.Visible = false; 
		fireZoneArea.Monitoring = false;
		
		// Désactiver le vol
		isFlying = false;
		
		// Faire tomber le boss vers sa position initiale en 0.5s
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "global_position", initialPosition, 0.5f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		
		// Attendre la fin de la chute
		await ToSignal(tween, "finished");
		
		// Le boss touche le sol : lancer l'animation Stun
		bossAnimPlayer.Play("Stun");
		
		// Faire apparaître les 5 étoiles
		DespawnStunStars();								// pour protéger en cas de persistance
		SpawnStunStars();
		
		// Attendre 3 secondes (durée du stun)
		await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
		
		// Faire disparaître les étoiles
		DespawnStunStars();
		
		// IMPORTANT : Forcer le retour complet à la position initiale au sol
				   
		Vector3 resetPos = initialPosition;
		resetPos.Y = groundY; // Force la hauteur au sol
		GlobalPosition = resetPos;
		
		// Forcer la rotation initiale pour éviter désorientation
		Vector3 resetRot = RotationDegrees;
		resetRot.Y = initialRotationY;
		RotationDegrees = resetRot;
		
		GD.Print("Stun terminé, boss revient à position initiale au sol");

		// Reprendre le cycle normal depuis le début
		if (currentPhase == 1)
			StartPhase1Cycle();
		else
			StartPhase2Cycle();
	}
	
	// Instancier les 5 étoiles en cercle au-dessus de la tête
	private void SpawnStunStars()
	{
		if (stunStarScene == null || stunStarsSpawnPoint == null)
			return;
		
		// Rayon du cercle d'étoiles
		float radius = 5.0f;
		
		// Instancier 5 étoiles espacées de 72° (360° / 5)
		for (int i = 0; i < 5; i++)
		{
			// Calculer l'angle de cette étoile
			float angle = i * (360f / 5f);
			float angleRad = Mathf.DegToRad(angle);
			
			// Calculer la position en cercle
			Vector3 offset = new Vector3(
				Mathf.Cos(angleRad) * radius,
				0,
				Mathf.Sin(angleRad) * radius
			);
			
			// Instancier l'étoile
			Node3D star = stunStarScene.Instantiate<Node3D>();
			stunStarsSpawnPoint.AddChild(star);
			star.Position = offset;
			
			// Stocker la référence
			stunStars[i] = star;
		}
	}
	
	// Supprimer les étoiles
	private void DespawnStunStars()
	{
		for (int i = 0; i < 5; i++)
		{
			if (stunStars[i] != null && IsInstanceValid(stunStars[i]))
			{
				stunStars[i].QueueFree();
				stunStars[i] = null;
			}
		}
	}
	
	// ============================================
	// SYSTÈME DE CHARGE (AVANCÉE VERS JOUEUR)
	// ============================================
	
	// Appelé au début de l'animation AvanceCroque&Recule
	// Enregistre la position du joueur et oriente le boss vers lui
	private void PrepareCharge()
	{
		// Chercher le joueur
		var player = GetNode<CharacterBody3D>("/root/World1/PlayerBotCtrl");
		if (player == null)
		{
			GD.PrintErr("Joueur introuvable pour la charge!");
			return;
		}
		
		// Enregistrer la position du joueur (Point J-ACR)
		chargeTargetPosition = player.GlobalPosition;
		
		// Calculer la direction vers le joueur (seulement en X et Z, pas Y)
		Vector3 directionToPlayer = (chargeTargetPosition - GlobalPosition);
		directionToPlayer.Y = 0; // Ignorer la hauteur
		directionToPlayer = directionToPlayer.Normalized();
		
		// Calculer l'angle de rotation nécessaire
		float targetAngle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
		
		// Orienter le boss vers le joueur instantanément
		Vector3 rotation = RotationDegrees;
		rotation.Y = Mathf.RadToDeg(targetAngle);
		RotationDegrees = rotation;
		
		//GD.Print($"Boss prépare sa charge vers {chargeTargetPosition}");
	}
	
	// Déplace le boss vers le point J-ACR pendant l'animation
	// À appeler dans _PhysicsProcess pendant les 2 premières secondes
	private void UpdateChargeMovement(double delta)
	{
		// A l'export
		//// Vitesse de déplacement (encore plus rapide : 5 m/s)
		//chargeSpeed = 6f;  
		
		// Direction vers la cible
		Vector3 direction = (chargeTargetPosition - GlobalPosition);
		direction.Y = 0;
		
		// Si on est arrivé (distance < 0.5m), ne plus bouger
		if (direction.Length() < 0.5f)
			return;
		
		direction = direction.Normalized();
		
		// Déplacer le boss
		GlobalPosition += direction * chargeSpeed * (float)delta;
	}
	
	
	// ============================================
	// PHASE 1 : CYCLE D'ATTAQUES (16 PV → 8 PV)
	// ============================================
	
	private void StartPhase1Cycle()
	{
		currentState = BossState.Idle;
		cycleStep = 0;
		ExecutePhase1Step();
	}
	
	private void ExecutePhase1Step()
	{
		switch (cycleStep)
		{
			case 0: // Idle (1s)
				bossAnimPlayer.Play("Idle");
				stateTimer = 1f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 1: // Charge (4s)  
				currentState = BossState.Attacking;
				
				// Préparer la charge (orienter vers joueur)
				PrepareCharge();
				
				// Lancer l'animation
				bossAnimPlayer.Play("AvanceCroque&Recule");
				stateTimer = 4f; 

				// Boss NE PEUT PAS ETRE STUN pendant la charge (AvanceCroque&Recule)
				isVulnerableToStun = false;
				
				// Activer le déplacement vers J-ACR (géré dans _PhysicsProcess)
				cycleStep++;
				break;
			
			case 2: // Idle (1s)
				currentState = BossState.Idle;
				bossAnimPlayer.Play("Idle");
				stateTimer = 0.5f;
						  
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 3: // Décollage&Vol (2s)
				currentState = BossState.TakingOff;
				
				// IMPORTANT : Forcer l'orientation initiale avant le vol
				// pour éviter que le boss décolle dans une mauvaise direction
				Vector3 resetRotation = RotationDegrees;
				resetRotation.Y = initialRotationY;
				RotationDegrees = resetRotation;
				
				GetTree().CreateTimer(0.3f).Timeout += () =>
				{
					Tween tweenRot = GetTree().CreateTween();
					Vector3 targetRot = bossCoxane.RotationDegrees;
					targetRot.Z += 360f;
					tweenRot.TweenProperty(bossCoxane, "rotation_degrees", targetRot, 0.8f)
						.SetTrans(Tween.TransitionType.Cubic)
						.SetEase(Tween.EaseType.InOut);
				};

				
				bossAnimPlayer.Play("Décollage&Vol");
				stateTimer = 2f;
						  
				isVulnerableToStun = false;
				
				// Activer le vol (le boss va monter)
				isFlying = true;
				
				cycleStep++;
				break;
			
			case 4: // Vol&Mitraillage (7s)
				currentState = BossState.Mitraillaging;
				
				bossAnimPlayer.Play("Vol&Mitraillage");
				stateTimer = 7f;
						
				isVulnerableToStun = true; 						
				isFlying = true;							// Boss en vol
				
				cycleStep++;
				break;
			
			case 5: // Vol&Atterrissage (2s)
				currentState = BossState.Landing;
				bossAnimPlayer.Play("Vol&Atterrissage");
				stateTimer = 1.5f;
						  
				isVulnerableToStun = false; // Plus de stun en descente!
				
				// Désactiver le vol (le boss va redescendre)
				isFlying = false;
				
				cycleStep++;
				break;
			
			default: // Recommencer le cycle
				cycleStep = 0;
				ExecutePhase1Step();
				break;
		}
	}
	
	// ============================================
	// PHASE 2 : CYCLE D'ATTAQUES (8 PV → 0 PV)
	// ============================================
	
	private void TransitionToPhase2()
	{
		GD.Print("=== PASSAGE EN PHASE 2 ===");
		currentPhase = 2;
		// On laisse finir l'animation en cours, la Phase 2 démarrera au prochain cycle
	}
	
	private void StartPhase2Cycle()
	{
		currentState = BossState.Idle;
		cycleStep = 0;
		ExecutePhase2Step();
	}
	
	private void ExecutePhase2Step()
	{
		switch (cycleStep)
		{
			case 0: // Idle
				bossAnimPlayer.Play("Idle");
				stateTimer = 0.5f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 1: // AvanceCroque&ReculeSD (4s) - Encore plus rapide!
				currentState = BossState.Attacking;
				
				// Préparer la charge SD (orienter vers joueur)
				PrepareCharge();
				
				// Lancer l'animation
				bossAnimPlayer.Play("AvanceCroque&ReculeSD");
				stateTimer = 4f; // Encore plus rapide
				
				// Boss N'EST PAS vulnérable pendant la charge
				isVulnerableToStun = false;
				
				cycleStep++;
				break;
			
			case 2: // Idle (0.5s)
				currentState = BossState.Idle;
				bossAnimPlayer.Play("Idle");
				stateTimer = 0.5f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 3: // Décollage&Vol (2s)
				currentState = BossState.TakingOff;
				
				// IMPORTANT : Forcer l'orientation initiale avant le vol
				Vector3 resetRotation2 = RotationDegrees;
				resetRotation2.Y = initialRotationY;
				RotationDegrees = resetRotation2;
				
				GetTree().CreateTimer(0.3f).Timeout += () =>
				{
					Tween tweenRot = GetTree().CreateTween();
					Vector3 targetRot = bossCoxane.RotationDegrees;
					targetRot.Z += 360f;
					tweenRot.TweenProperty(bossCoxane, "rotation_degrees", targetRot, 0.8f)
						.SetTrans(Tween.TransitionType.Cubic)
						.SetEase(Tween.EaseType.InOut);
				};
				
				bossAnimPlayer.Play("Décollage&Vol");
				stateTimer = 2f;
				isVulnerableToStun = false;
				
				// Activer le vol
				isFlying = true;
				
				cycleStep++;
				break;
			
			case 4: // Coup Special (7s) - BOURRASQUE!
				currentState = BossState.Bourrasquing;
				bossAnimPlayer.Play("Coup Special");
				stateTimer = 7f;
				isVulnerableToStun = true;				// FENETRE DE TIR POUR PLAYER
				
				// Le boss reste en vol pendant la bourrasque
				isFlying = true;
				
				// Lancer la bourrasque
				ApplyBourrasque();
				cycleStep++;
				break;
			
			
			case 5: // Vol&Mitraillage (7s)
				currentState = BossState.Mitraillaging;
				bossAnimPlayer.Play("Vol&Mitraillage");
				stateTimer = 7f;
				isVulnerableToStun = false; 			// INVULNERABLE EN PHASE 2
				
				// Le boss reste en vol
				isFlying = true;
				
				cycleStep++;
				break;
			

			case 6: // Vol&Atterrissage (2s)
				bossAnimPlayer.Play("Vol&Atterrissage");
				stateTimer = 2f;
				isVulnerableToStun = false; // Plus de stun!
				
				// Désactiver le vol (descente)
				isFlying = false;
				
				
				cycleStep++;
				break;
			
			default: // Recommencer le cycle
				cycleStep = 0;
				ExecutePhase2Step();
				break;
		}
	}
	
	// ============================================
	// BOURRASQUE (COUP SPECIAL)
	// ============================================
	
	private void ApplyBourrasque()
	{
		timerStartBourrasque.Start();
	}
	
	public void _on_timer_start_bourrasque_timeout()
	{
		Vector3 windDirection = this.GlobalTransform.Basis.Z;	// dans le sens du Boss
		windDirection.Y = 0; 									// vent reste horizontal
		windDirection = windDirection.Normalized();				
		
		Vector3 finalWindVector = windDirection * windForce;	// vent = direction + force
		
		playerScript.ExternalPushVelocity = finalWindVector;	// application au joueur
		
		GD.Print("Bourrasque lancée !");
		
		timerEndBourrasque.Start();
	}
	
	public void _on_timer_end_bourrasque_timeout()
	{
		playerScript.ExternalPushVelocity = Vector3.Zero;
	}
	
	//======================================================
	// MORT DU JOUEUR --> <Continue> --> FULL RESET COMBAT
	//======================================================
	
public void _on_game_over_scene_back_from_death()
{
	GD.Print("Reset du boss après Game Over...");

	// ==========================
	// < Reset PV et UI >
	// ==========================
	currentHealth = maxHealth;
	if (bossHealthBar != null)
		bossHealthBar.UpdateBossLife(currentHealth);
	if (fouetClignote)
		fouetClignote = false;
	
	// =============================
	// < Reset position et rotation >
	// =============================
	GlobalPosition = initialPosition;
	RotationDegrees = new Vector3(0f, initialRotationY, 0f);
	isFlying = false;

	// ==========================
	// < Reset état et cycle >
	// ==========================
	currentState = BossState.Idle;
	currentPhase = 1;
	cycleStep = 0;
	stateTimer = 0f;
	CanTakeDamage = false;
	isVulnerableToStun = false;
	bombUsed = false;

	// ==========================
	// Stop animations
	// ==========================
	bossAnimPlayer?.Stop();
	CombatAnimPlayer?.Stop();
	weakPointPlayer?.Stop();

	// ==========================
	// Reset effets visuels
	// ==========================
	if (propulseurGauche != null) propulseurGauche.Emitting = false;
	if (propulseurDroit != null) propulseurDroit.Emitting = false;

	if (fireZoneArea != null)
	{
		fireZoneArea.Visible = false;
		fireZoneArea.Monitoring = false;
	}

	DespawnStunStars();
	weakPointGlowing = false;
	warningUnderFire.Visible = false;


	// ==========================
	// Relancer cycle Phase 1
	// ==========================
	CallDeferred(nameof(StartPhase1Cycle));

	GD.Print("Boss et joueur réinitialisés, prêt pour Phase 1 !");
}


	
	// ============================================
	// MORT DU BOSS
	// ============================================
	
	private void Die()
	{
		GD.Print("=== BOSS VAINCU ===");
		currentState = BossState.Dead;
		
		// Arrêter toutes les animations
		bossAnimPlayer.Stop();
		
		
		EmitSignal(SignalName.CombatFini);
	}
}
