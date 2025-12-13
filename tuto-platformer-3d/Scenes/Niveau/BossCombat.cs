using Godot;
using System;

public partial class BossCombat : Node3D
{
	// ============================================
	// VARIABLES EXPORTÉES (modifiables dans l'éditeur)
	// ============================================
	
	[Export] public int maxHealth = 16; // PV maximum du boss
	[Export] public AnimationPlayer animationPlayer; // Lecteur d'animations
	[Export] public Node3D weakPointVisual; // Cercle blanc + croix sur le dos
	[Export] public MeshInstance3D bossMesh; // Mesh principal du boss (pour le flash blanc)
	[Export] public AudioStreamPlayer3D impactSound; // Son d'impact quand touché
	[Export] public PackedScene stunStarScene; // Scène de l'étoile à instancier
	[Export] public Marker3D stunStarsSpawnPoint; // Point d'apparition des étoiles (au-dessus tête)
	[Export] public AnimationPlayer animBossCombat;
	[Export] public BossHealthBar bossHealthBar;
	[Export] public PackedScene player;
	[Export] public MarginContainer bossHPContainer;
	public GameState gameState;
	public PlayerBotCtrl playerScript;
	

	// Paramètre du Coup Special  "Bourrasque"
	[Export] float windForce = 15.0f; 
	
	// Effets visuels du mitraillage
	[Export] public GpuParticles3D propulseurGauche; // Particules du propulseur gauche
	[Export] public GpuParticles3D propulseurDroit; // Particules du propulseur droit
	
	
	// Paramètre de vol
	[Export] public float hauteurVolBoss = 5f; // Hauteur de vol au-dessus du sol
	

	// ============================================
	// VARIABLES INTERNES
	// ============================================
	
	private int currentHealth; // PV actuels
	private Vector3 initialPosition; // Position de départ (pour le retour après stun)
	private float initialRotationY; // Rotation Y de départ (pour la bourrasque)
	
	// État du boss (State Machine)
	private enum BossState
	{
		Idle,
		Attacking,
		Flying,
		Stunned,
		Dead
	}
	private BossState currentState = BossState.Idle;
	
	// Phase du combat (1 ou 2)
	private int currentPhase = 1;
	
	// Vulnérabilité au stun (true quand en l'air)
	public bool isVulnerableToStun = false; // Public pour BossWeakPoint
	
	// Position du joueur au début de la charge
	private Vector3 chargeTargetPosition;
	
	// Timer pour gérer les cycles d'attaques
	private float stateTimer = 0f;
	
	// Timer pour le mitraillage (dégâts continus)
	private float mitraillageTimer = 0f;
	private bool isMitraillaging = false;
	
	// Compteur d'étapes dans le cycle
	private int cycleStep = 0;
	
	// Étoiles de stun instanciées
	private Node3D[] stunStars = new Node3D[5];
	
	// État de vol du boss
	private bool isFlying = false; // Le boss est-il en train de voler ?
	private float groundY; // Position Y au sol (pour revenir après vol)
	
	// Matériau pour le flash blanc (override temporaire)
	private StandardMaterial3D flashMaterial;
	
	// Etat du personnage, s'il vient de se prendre un dommage -> playerIsHurt = true
	private bool _playerIsHurt = false;

	//// Rayons laser visuels actifs
	//private Godot.Collections.Array<Node3D> activeRays = new Godot.Collections.Array<Node3D>();
	
	// ============================================
	// INITIALISATION
	// ============================================
	
	public override void _Ready()
	{
		
		playerScript = GetNode<PlayerBotCtrl>("/root/World1/PlayerBotCtrl");
		
		gameState = GetNode<GameState>("/root/GameState");
		
		// on rend visible la barre de vie
		bossHPContainer.Visible = true;
		
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
		if (weakPointVisual != null)
			weakPointVisual.Visible = false;
		
	}
	
	// ============================================
	// BOUCLE PRINCIPALE
	// ============================================
	
	public override void _Process(double delta)
	{
		// Si le boss est mort, ne rien faire
		if (currentState == BossState.Dead)
			return;
		
		// Gérer la visibilité du point faible (visible seulement quand on peut faire des dégâts)
		if (weakPointVisual != null)
		{
			// Visible si :
			// - Boss vulnérable au stun (en vol)
			// - OU pendant la première moitié de la charge (2 premières secondes sur 4s)
			bool visibleDuringCharge = (currentState == BossState.Attacking && stateTimer <= 2f);
			weakPointVisual.Visible = isVulnerableToStun || visibleDuringCharge;
		}
		
		// Gérer le mitraillage (raycasts continus)
		if (isMitraillaging)
		{
			//animBossCombat.Play("ZoneMitraillee");
			gameState.zoneFeu = true;
		}
		
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
				pos.Y -= 3f * (float)delta;
				
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
			if (stateTimer > 2f)
			{
				UpdateChargeMovement(delta);
			}
			// Si on est dans la seconde moitié (stateTimer <= 2s), le boss recule
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
				float retreatSpeed = 5f;
				GlobalPosition += directionHome * retreatSpeed * (float)delta;
			}
		}
	}
	// ============================================
	// GESTION DES DÉGÂTS SUR LE JOUEUR
	// ============================================
	
	public void _on_area_dmg_mandib_r_body_entered(Node3D body)
	{
		if (body is PlayerBotCtrl)
		{
			playerScript.TakeDamages();
		}
	}
	
	
	


	// ============================================
	// GESTION DES DÉGÂTS SUR LE BOSS
	// ============================================
	
	// Appelé par BossWeakPoint quand le joueur touche le point faible
	public void BossTakeDamage(int damage, bool shouldStun)
	{
		// Réduire les PV
		currentHealth -= damage;
		GD.Print($"Boss a pris {damage} dégâts ! PV restants : {currentHealth}");
		
		// Mettre à jour la barre de vie
		if (bossHealthBar != null)
			bossHealthBar.UpdateBossLife(currentHealth);
		
		// Feedback visuel : flash blanc
		FlashWhite();
		
		// Jouer le son d'impact
		if (impactSound != null)
			impactSound.Play();
		
		// Si le boss doit être stun (touché en l'air)
		if (shouldStun && isVulnerableToStun)
		{
			ApplyStun();
		}
		
		// Vérifier si le boss doit mourir
		if (currentHealth <= 0)
		{
			Die();
			return;
		}
		
		// Vérifier si on passe en Phase 2 (25% PV = 4 PV)
		if (currentHealth <= 4 && currentPhase == 1)
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
		
		GD.Print("Flash blanc terminé !");
	}
	
	// ============================================
	// SYSTÈME DE STUN
	// ============================================
	
	private async void ApplyStun()
	{
		GD.Print("Boss stunned!");
		
		// Passer en état Stunned
		currentState = BossState.Stunned;
		isVulnerableToStun = false;
		
		// Arrêter l'animation en cours
		animationPlayer.Stop();
		animBossCombat.Stop();
		
		// Arrêter le mitraillage si actif
		isMitraillaging = false;
		
		// Désactiver les effets visuels
		if (propulseurGauche != null) propulseurGauche.Emitting = false;
		if (propulseurDroit != null) propulseurDroit.Emitting = false;
		
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
		animationPlayer.Play("Stun");
		
		// Faire apparaître les 5 étoiles
		SpawnStunStars();
		
		// Attendre 3 secondes (durée du stun)
		await ToSignal(GetTree().CreateTimer(3f), "timeout");
		
		// Faire disparaître les étoiles
		DespawnStunStars();
		
		// IMPORTANT : Forcer le retour complet à la position initiale au sol
		isFlying = false;
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
		float radius = 3.0f;
		
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
		// Vitesse de déplacement (encore plus rapide : 5 m/s)
		float chargeSpeed = 5f;
		
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
	// PHASE 1 : CYCLE D'ATTAQUES (16 PV → 4 PV)
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
			case 0: // Idle (2s)
				animationPlayer.Play("Idle");
				stateTimer = 2f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 1: // AvanceCroque&Recule (4s)  (2 fois plus vite que l'animation originale)
				currentState = BossState.Attacking;
				
				// Préparer la charge (orienter vers joueur)
				PrepareCharge();
				
				// Lancer l'animation
				animationPlayer.Play("AvanceCroque&Recule");
				stateTimer = 4f; 
				
				// Boss NE PEUT PAS ETRE STUN pendant la charge (AvanceCroque&Recule)
				isVulnerableToStun = false;
				
				// Activer le déplacement vers J-ACR (géré dans _PhysicsProcess)
				cycleStep++;
				break;
			
			case 2: // Idle (1s)
				currentState = BossState.Idle;
				animationPlayer.Play("Idle");
				stateTimer = 1f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 3: // Décollage&Vol (3s)
				currentState = BossState.Flying;
				
				// IMPORTANT : Forcer l'orientation initiale avant le vol
				// pour éviter que le boss décolle dans une mauvaise direction
				Vector3 resetRotation = RotationDegrees;
				resetRotation.Y = initialRotationY;
				RotationDegrees = resetRotation;
				
				animationPlayer.Play("Décollage&Vol");
				stateTimer = 3f;
				isVulnerableToStun = true; // VULNERABLE!
				
				// Activer le vol (le boss va monter)
				isFlying = true;
				
				cycleStep++;
				break;
			
			case 4: // Vol&Mitraillage (7s)
				animationPlayer.Play("Vol&Mitraillage");
				stateTimer = 7f;
				isVulnerableToStun = true; // VULNERABLE!
				
				// Le boss reste en vol
				isFlying = true;
				
				// Activer les effets visuels des propulseurs
				if (propulseurGauche != null) propulseurGauche.Emitting = true;
				if (propulseurDroit != null) propulseurDroit.Emitting = true;
				
				// Activer le système de mitraillage (raycasts)
				isMitraillaging = true;
				mitraillageTimer = 0f; // Dégât immédiat
				
				cycleStep++;
				break;
			
			case 5: // Vol&Atterrissage (3s)
				animationPlayer.Play("Vol&Atterrissage");
				stateTimer = 3f;
				isVulnerableToStun = false; // Plus de stun en descente!
				
				// Désactiver le vol (le boss va redescendre)
				isFlying = false;
				
				// Désactiver le mitraillage et les effets visuels
				isMitraillaging = false;
				if (propulseurGauche != null) propulseurGauche.Emitting = false;
				if (propulseurDroit != null) propulseurDroit.Emitting = false;
				////ClearAllRays();
				
				cycleStep++;
				break;
			
			default: // Recommencer le cycle
				cycleStep = 0;
				ExecutePhase1Step();
				break;
		}
	}
	
	// ============================================
	// PHASE 2 : CYCLE D'ATTAQUES (4 PV → 0 PV)
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
			case 0: // Idle (1s)
				animationPlayer.Play("Idle");
				stateTimer = 1f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 1: // AvanceCroque&ReculeSD (4s) - Encore plus rapide!
				currentState = BossState.Attacking;
				
				// Préparer la charge SD (orienter vers joueur)
				PrepareCharge();
				
				// Lancer l'animation
				animationPlayer.Play("AvanceCroque&ReculeSD");
				stateTimer = 4f; // Encore plus rapide
				
				// Boss N'EST PAS vulnérable pendant la charge
				isVulnerableToStun = false;
				
				cycleStep++;
				break;
			
			case 2: // Idle (0.5s)
				currentState = BossState.Idle;
				animationPlayer.Play("Idle");
				stateTimer = 0.5f;
				isVulnerableToStun = false;
				cycleStep++;
				break;
			
			case 3: // Décollage&Vol (3s)
				currentState = BossState.Flying;
				
				// IMPORTANT : Forcer l'orientation initiale avant le vol
				Vector3 resetRotation2 = RotationDegrees;
				resetRotation2.Y = initialRotationY;
				RotationDegrees = resetRotation2;
				
				animationPlayer.Play("Décollage&Vol");
				stateTimer = 3f;
				isVulnerableToStun = true; // VULNERABLE!
				
				// Activer le vol
				isFlying = true;
				
				cycleStep++;
				break;
			
			case 4: // Vol&Mitraillage (7s)
				animationPlayer.Play("Vol&Mitraillage");
				stateTimer = 7f;
				isVulnerableToStun = true; // VULNERABLE!
				
				// Le boss reste en vol
				isFlying = true;
				
				// Activer les effets visuels
				if (propulseurGauche != null) propulseurGauche.Emitting = true;
				if (propulseurDroit != null) propulseurDroit.Emitting = true;
				
				// Activer le système de mitraillage
				isMitraillaging = true;
				mitraillageTimer = 0f;
				
				cycleStep++;
				break;
			
			case 5: // Coup Special (7s) - BOURRASQUE!
				animationPlayer.Play("Coup Special");
				stateTimer = 7f;
				isVulnerableToStun = true; // VULNERABLE!
				
				// Le boss reste en vol pendant la bourrasque
				isFlying = true;
				
				// Lancer la bourrasque
				ApplyBourrasque();
				cycleStep++;
				break;
			
			case 6: // Vol&Atterrissage (3s)
				animationPlayer.Play("Vol&Atterrissage");
				stateTimer = 3f;
				isVulnerableToStun = false; // Plus de stun!
				
				// Désactiver le vol (descente)
				isFlying = false;
				
				// Désactiver le mitraillage et effets visuels
				isMitraillaging = false;
				if (propulseurGauche != null) propulseurGauche.Emitting = false;
				if (propulseurDroit != null) propulseurDroit.Emitting = false;
				////ClearAllRays();
				
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
		if (player == null)
		{
			GD.PrintErr("ERREUR : As-tu penser à assigner le PlayerBotCtrl dans l'inspecteur du Boss ?");
			return;
		}
		
		// --- CALCUL DE LA DIRECTION ---
		// Méthode Pro : Utiliser le Basis du Boss.
		// Dans Godot, Basis.Z est le vecteur "arrière". -Basis.Z est "devant".
		// Si le vent sort du ventre du boss, c'est souvent -GlobalTransform.Basis.Z
		Vector3 windDirection = -this.GlobalTransform.Basis.Z;
		
		// On aplatit Y à 0 pour ne pas enfoncer le joueur dans le sol ou le faire voler
		windDirection.Y = 0; 
		windDirection = windDirection.Normalized();
		
		Vector3 finalWindVector = windDirection * windForce;
		
		// --- TWEEN (Animation des valeurs) ---
		Tween tween = CreateTween();
		
		// 1. Monter la puissance du vent (0 -> Max) en 0.5 sec
		// Note la syntaxe correcte : Tween.EaseType.Out
		
		tween.TweenProperty(playerScript, nameof(PlayerBotCtrl.ExternalPushVelocity), finalWindVector, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
				
		// 2. Maintenir le vent pendant 3 secondes
		tween.TweenInterval(3.0f);
		
		// 3. Arrêter le vent (Max -> 0) en 1.0 sec
		tween.TweenProperty(playerScript, nameof(PlayerBotCtrl.ExternalPushVelocity), Vector3.Zero, 1.0f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In); // "In" est souvent mieux pour la fin (ralentissement progressif)
			 
		GD.Print("Bourrasque lancée !");
	}
		
		
	// ============================================
	// MORT DU BOSS
	// ============================================
	
	private void Die()
	{
		GD.Print("=== BOSS VAINCU ===");
		currentState = BossState.Dead;
		
		// Arrêter toutes les animations
		animationPlayer.Stop();
		
		// Désactiver le mitraillage
		isMitraillaging = false;
		
		bossHPContainer.Visible = false;
		
		
		// TODO: Animation de mort, loot, débloquer la zone de fin, etc.
		// Pour l'instant, on supprime juste le boss
		QueueFree();
	}
}
