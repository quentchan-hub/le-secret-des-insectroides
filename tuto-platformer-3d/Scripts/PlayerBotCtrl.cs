using Godot;
using System;

public partial class PlayerBotCtrl : CharacterBody3D
{
	
	// < Paramètres Personnage >
	
	[Export] int life = 3; 						// points de vie du personnage
	[Export] float speed = 20f; 				// vitesse déplacement
	[Export] float acceleration = 15; 			// acceleration du personnage de 0 à speed
	[Export] float airAcceleration = 5f; 		// modification trajectoire saut
	[Export] float maxVelocity = 55f; 			// vitesse maximale atteignable
	[Export] float jumpForce = 15f; 			// impulsion saut (plafonnée par maxVelocity)
	[Export] AnimationPlayer animationPlayer;	// Animations du personnages
	
	private float extraSpeed = 1f;				// boost vitesse (voir -> input "run") 
	private Vector3 velocity; 					// velocity = vitesse(= distance) + direction
	private float yVelocity; 					// velocity vers le haut (axe Y)
	public bool bounce = false;					// Rebondir sur un ennemi
	
	// < Paramètres Environnement >
	
	[Export] float gravity = 1f; 				// gravité
	

	public Vector3 ExternalPushVelocity { get; set; } = Vector3.Zero;
	// force du vent du Boss (coup spécial)
	// { get; set; } est OBLIGATOIRE pour que le Tween puisse la modifier
	
	[Export] Area3D noyade;						// zone mort personnage dans l'eau
	[Export] Marker3D teleportMarkerSG1; 		// point de sortie tp SG1
	[Export] Marker3D teleportMarkerSG2; 		// point de sortie tp SG2
	
	
	// < Souris et Caméra >
	
	[Export(PropertyHint.Range, "0.1,1.0")] float mouseSensivity = 0.3f; 
	// Sensibilité mouvement souris "mouseSensivity" 
	// Associée à position angulaire du personnage ("RotationDegrees") 
	// => fait tourner la caméra ("cameraPivot.RotationDegrees = rotDeg;")
	
	[Export(PropertyHint.Range, "-90,0,1")]	float minPitch = 90f;
	// Rotation maximale caméra vers le bas
	
	[Export(PropertyHint.Range, "0,90,1")]	float maxPitch = 90f;
	// Rotation maximale caméra vers le haut
	
	[Export] Node3D cameraPivot;				// Support caméra
	[Export] Camera3D camera;					// Caméra
	
	
	// < Paramètres Feedback combat >
	
	[Export] MeshInstance3D playerMesh;			// modelisation graphique personnage
	public StandardMaterial3D hitFlashMat;    	// feedback visuel de dommage
		
	
	// < Paramètres UI >
	
	[Export] PackedScene lifeHeart; 			// gestion UI Heart couplé à vie perso
	[Export] HBoxContainer heartContainer;
	
	[Export] public QuitMenu quitMenu;
	[Export] public GameOverScene sceneGameOver;
	
	
	//  < Sons et Musiques >
	public SoundManager soundManager;			// gestion musiques/sons
	
	
	//  < Game Manager >
	public GameState gameState;					// autoload game manager

	
	public override void _Ready()
	{
		//Input.MouseMode = Input.MouseModeEnum.Captured;
		
		animationPlayer.Play("Idle");
		
		managingLifeHeart();
		
		hitFlashMat = new StandardMaterial3D();
		hitFlashMat.AlbedoColor = new Color(0, 0, 1, (float)0.6f);
		
		if (RespawnManager.LastRespawnPoint != Vector3.Zero)
			GlobalPosition = RespawnManager.LastRespawnPoint;
		else
			RespawnManager.LastRespawnPoint = GlobalPosition;
		
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
		gameState = GetNode<GameState>("/root/GameState");
		//gameState.playerStartPosition = GlobalPosition;
			
		sceneGameOver.Visible = false;
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))	// ui_cancel = touche Esc.
		{
			if (quitMenu.Visible == false)
			{
				quitMenu.MontrePopup();				// Affiche QuitMenu
			}
		}
	
		if (Input.IsActionJustPressed("touche_check"))  //touche_check = touche X
		{
			GD.Print("test = aucun test en cours");
		}
	}
	
	public override void _Input(InputEvent @event)

	// * _Input => Méthode intégrée de Godot : appelée automatiquement 
	//             à chaque événement d'entrée (clavier, souris, manette, etc.).
	
	// * InputEvent => trad. litt. événement d'entrée
	//                 (touches, clics, mouvements de souris, etc.).
	
	// * @event => Paramètre qui représente l'événement actuel (clic...)
	//             Le @ devant event est utilisé car "event" est un mot-clé 
	//             réservé* en C#. Le @ permet de l'utiliser comme nom de var.
	
	// * mot-clé réservé => nom de variable pré-intégrée à un langage, 
	//                      ne pas utiliser ou mettre un @ devant  
	//                      donc : "event" interdit / "@event" autorisé

	// 	Les grands types d'évènement classiques @event :
	// 			InputEventMouseMotion : Mouvement de la souris.
	// 			InputEventKey : Appui sur une touche du clavier.
	// 			InputEventMouseButton : Clic de souris.
	
	{
		if (@event is InputEventMouseMotion motionEvent)
		// input event observé = mouvement souris => motionEvent
		
		{
			Vector3 rotDeg = RotationDegrees;
			// RotationDegrees = position angulaire du personnage
			
			rotDeg.Y -= motionEvent.Relative.X * mouseSensivity; 
			// rotDeg.Y = rotation autour de l'axe Y
			// Application du mouvement souris sur l'axe X (gauche/droite)
			// et mouvement proportionnalisé à la sensibilité souris
			
			RotationDegrees = rotDeg; 
			// Application de la nouvelle position à rotDeg
			
			rotDeg = cameraPivot.RotationDegrees;
			// "cameraPivot.RotationDegrees" = position angulaire caméra
			
			rotDeg.X -= motionEvent.Relative.Y * -mouseSensivity;
			rotDeg.X = Mathf.Clamp(rotDeg.X, minPitch, maxPitch);
			cameraPivot.RotationDegrees = rotDeg;
			
			// pourquoi même var rotDeg pour les 2 variables?? 
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);					// aucune idée de ce que c'est
	}
	
	private void HandleMovement(double delta)
	{
		Vector3 direction = new Vector3(0,0,0);	// direction personnage
		
		if (Input.IsActionPressed("move_up"))
			direction += Transform.Basis.Z;
		if (Input.IsActionPressed("move_down"))
			direction -= Transform.Basis.Z;
		if (Input.IsActionPressed("move_left"))
			direction += Transform.Basis.X;
		if (Input.IsActionPressed("move_right"))
			direction -= Transform.Basis.X;
			
		if (Input.IsActionPressed("run"))		// pour test jeu
		{
			extraSpeed = 2f;					// ici pour modifier boost
		} else {
			extraSpeed = 1f;
		}
		
		direction = direction.Normalized();		// même vitesse qq soit la direction	

		float accel = IsOnFloor() ? acceleration : airAcceleration;

		velocity = direction * speed * accel * extraSpeed; 
		
		if (bounce)
		{
			yVelocity = jumpForce;
			bounce = false;
		}
		else
		{
			if (IsOnFloor())
			{
				yVelocity = -0.01f; 
				if (direction != Vector3.Zero)
				{
					if (animationPlayer.CurrentAnimation != "walk")
						animationPlayer.Play("walk");
				}
				else
				{
					if (animationPlayer.CurrentAnimation != "Idle")
						animationPlayer.Play("Idle");
				}
			}
			else
			{
				yVelocity = Mathf.Clamp(yVelocity-gravity, -maxVelocity, maxVelocity);
				if (animationPlayer.CurrentAnimation != "fall")
					animationPlayer.Play("fall");
			}
		}
		
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			yVelocity = jumpForce;
			
			if (gameState.aLeRessort)
			{
				jumpForce = 30f;
				soundManager.PlayHighJump();
			}
		}		
		
		velocity.Y = yVelocity;
		
		//  ---------------------------------------------------------
		//  AJOUT GEMINI : C'est ici qu'on ajoute le vent !
		//  ---------------------------------------------------------
		//  Pourquoi ici ? 
		//  1. On a déjà calculé ton mouvement volontaire (velocity.X et Z).
		//  2. On a déjà calculé la gravité/saut (velocity.Y).
		//  3. Maintenant, on ajoute la force externe par dessus le tout.
		
		velocity += ExternalPushVelocity;

		//  Si le vent est fort (ex: 5m/s) et que tu avances (5m/s) dans le sens opposé,
		//  5 + (-5) = 0, tu feras du surplace. C'est automatique !
		//  ---------------------------------------------------------

		// Envoi final au CharacterBody3D
		Velocity = velocity;
		
		MoveAndSlide();
		
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
	// GD.Print("I collided with ", ((Node)collision.GetCollider()).Name);
		}
	
			//if (direction != new Vector3(0,0,0) && IsOnFloor())
		//{
			//animationPlayer.Play("walk");
		//} // Idle, fall, walk, jump, run, ground_impact, simple_punch
		//else if (direction == new Vector3(0,0,0) && IsOnFloor())
		//{
			//animationPlayer.Play("Idle");
		//}
	}
	

	private void _on_area_3d_area_entered(Area3D area)
	{
		//GD.Print("Area détectée : ", area.Name);
		
		if (area.Name == "Area3DTop")				// Si c'est le dessus d'un ennemi
		{
			bounce = true;
		}
		
	}
	
	private void _on_area_3d_noyade_body_entered(Node3D body)
	{
		life = 0;
		die();
	}
	
	
	public void TakeDamages()
	{
		GD.Print("TakeDamages!");
		soundManager.playerDamagedAudio.Play();
		HitFlash();
		RemoveLife();
	}
	
	public async void HitFlash()
	{
		playerMesh.MaterialOverride = hitFlashMat;
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		playerMesh.MaterialOverride = null;
		
	}
	
	public void RemoveLife(int amount = 1)
	{
		life --;
		
		managingLifeHeart();
		
		if (life <= 0)
		{
			life = 0;
			die();
		}
		
	}
	
	public void AddLife(int amount)
	{
		life += amount;
		managingLifeHeart();
	}
	
	public void managingLifeHeart()
	{    
		foreach (Node child in heartContainer.GetChildren())
		{
			child.QueueFree();
		}
		for (int i = 0; i < life; i++)
		{
			TextureRect heartInstance = (TextureRect)lifeHeart.Instantiate();
			heartContainer.AddChild(heartInstance);
		}
	}
	
	private async void die()
	{
		GD.Print("Player Dead!");
		//
		//GetTree().ReloadCurrentScene();  <- annulé car provoquait une erreur : 
		//Cette erreur se produit lorsque tu supprimes ou recharges une scène 
		//(ou un nœud de type CollisionObject3D) pendant une callback de physique 
		//(comme body_entered). Godot ne permet pas de modifier la hiérarchie des 
		//nœuds de physique pendant qu'une simulation physique est en cours, 
		//car cela peut causer des comportements imprévisibles.
		//au final on va préférer faire un appel différé de la fonction 
		//en s'assurant d'être à la fin de la frame -> CallDeferred("nom de la fonction")
		//en string donc "nom" et pas de () à la fin
		
		soundManager.gameOverAudio.Play();
		soundManager.StopMainTheme();
		sceneGameOver.Visible = true;
		
		
		await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
		soundManager.EcouterExclusivement(soundManager.gameOverThemeMusic);
		GetTree().Paused = true;
		
		Input.MouseMode = Input.MouseModeEnum.Visible;
		
		//CallDeferred("ReloadScene"); 
	}
	

	
	public void _on_area_sg_1_area_entered(Area3D area)
	{
		if (area.Name == ("Area3DPlayer"))
		{
			soundManager.PlayTeleport();
			Vector3 destination = teleportMarkerSG1.GlobalPosition;
			Teleportation(destination);
			soundManager.EcouterExclusivement(soundManager.newIslandThemeMusic);
		}
	}
	
	public void _on_area_sg_2_area_entered(Area3D area)
	{
		if (area.Name == ("Area3DPlayer"))
		{
			soundManager.PlayTeleport();
			Vector3 destination = teleportMarkerSG2.GlobalPosition;
			Teleportation(destination);
		}
	}
	
	public async void Teleportation(Vector3 destination)
	{
		Tween tween = GetTree().CreateTween(); //création animation intermédiaire
		// Ensuite on détermine ce qu'on va modifier pour faire l'animation in between
		// TweenProperty(variable noeud, "propriété", valeur propriété, durée: float)
		tween.TweenProperty(this,"scale", new Vector3(0.1f, 0.1f, 0.1f),1f)
			.SetTrans(Tween.TransitionType.Bounce)
			.SetEase(Tween.EaseType.InOut);
		
		
		await ToSignal(tween, "finished");
		
		this.GlobalPosition = destination;
		
		
		Scale = new Vector3(0.1f, 0.1f, 0.1f);
		
		Tween tween2 = GetTree().CreateTween();
		tween2.TweenProperty(this, "scale", new Vector3(1.5f, 1.5f, 1.5f), 1f)
			.SetTrans(Tween.TransitionType.Elastic)
			.SetEase(Tween.EaseType.Out);
		
	}
	

}
