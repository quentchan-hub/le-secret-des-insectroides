using Godot;
using System;

public partial class PlayerBotCtrl : CharacterBody3D
{
	GameState gameState;
	[Export] public int life = 3; //vie du personnage
	[Export] float speed = 20f; //vitesse de marche
	float extraSpeed = 1f;
	[Export] float acceleration = 15; //acceleration du personnage de 0 à speed
	[Export] float airAcceleration = 5f; // modification légère d'une 
										 // trajectoire aerienne
	[Export] float gravity = 1f; //gravité
	[Export] float maxTerminalVelocity = 55f; // vitesse maximale 
											  //atteignable en général
	[Export] float jumpForce = 15f; // hauteur de saut
	[Export] PackedScene lifeHeart; 
	[Export] HBoxContainer heartContainer;
	[Export] MeshInstance3D playerMesh;
	public StandardMaterial3D hitFlashMat;
	public SoundManager soundManager;
	

	
	
	//
	// les variables ci-dessous determinent la sensibilité de la souris
	// = petit ou grand mouvement pour faire bouger la souris
	// et enregistre cette sensibilté dans une var. "mouseSensivity" 
	// on utilisera cette avr plus tard associée à la position angulaire du 
	// personnage ("RotationDegrees") pour faire bouger la caméra 
	// ("cameraPivot.RotationDegrees = rotDeg;")
	[Export(PropertyHint.Range, "0.1,1.0")] // export de la var mouseSensivity
	float mouseSensivity = 0.3f; // qui est la ligne suivante grâce au ; final
	[Export(PropertyHint.Range, "-90,0,1")]
	float minPitch = 90f;
	[Export(PropertyHint.Range, "0,90,1")]
	float maxPitch = 90f;
	public bool bounce = false; // Rebondir sur un ennemi
	
	Vector3 velocity; // velocity = vitesse + direction
	float yVelocity; 
	
	//  AJOUT GEMINI : La variable qui recevra la force du vent du Boss
	//  { get; set; } est OBLIGATOIRE pour que le Tween puisse la modifier.
	public Vector3 ExternalPushVelocity { get; set; } = Vector3.Zero;
	
	[Export] Node3D cameraPivot;
	[Export] Camera3D camera;
	[Export] AnimationPlayer animationPlayer;
	[Export] public QuitMenu quitMenu;
	[Export] public GameOverScene sceneGameOver;
	[Export] Area3D noyade;
	[Export] Marker3D teleportMarkerSG1; // point de sortie tp SG1
	[Export] Marker3D teleportMarkerSG2; // point de sortie tp SG2
	
	public override void _Ready()
	{


		//Input.MouseMode = Input.MouseModeEnum.Captured;
		animationPlayer.Play("Idle");
		gameState = GetNode<GameState>("/root/GameState");
		//gameState.playerStartPosition = GlobalPosition;
		sceneGameOver.Visible = false;
		managingLifeHeart();
		hitFlashMat = new StandardMaterial3D();
		hitFlashMat.AlbedoColor = new Color(0, 0, 1, (float)0.6f);
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
		if (RespawnManager.LastRespawnPoint != Vector3.Zero)
			GlobalPosition = RespawnManager.LastRespawnPoint;
		else
			RespawnManager.LastRespawnPoint = GlobalPosition; 
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (quitMenu.Visible == false)
			{
				quitMenu.MontrePopup();
			}
			
			//Input.MouseMode = Input.MouseModeEnum.Visible; 
			//// Définir le mode de la souris sur Visible
		}
	
		if (Input.IsActionJustPressed("touche_check"))  //touche check => x
		{
			GD.Print(gameState.aLaCle);
		}
	}
	public override void _Input(InputEvent @event)
	//
	// * _Input => Méthode virtuelle de Godot : Elle est appelée automatiquement 
	//             à chaque événement d'entrée (clavier, souris, manette, etc.).
	// * InputEvent => Type de base pour tous les événements d'entrée
	//                 (touches, clics, mouvements de souris, etc.).
	// * @event => Paramètre qui représente l'événement actuel (clic...)
	//             Le @ devant event est utilisé car "event" est un mot-clé 
	//             réservé* en C#. Le @ permet de l'utiliser comme nom de var.
	// * mot-clé réservé => ne pas utiliser comme nom de variable standard
	//                      ou mettre un @ devant donc "event" interdit 
	//                      "@event" autorisé
	//
	// 	Les grands types d'évènement classiques @event :
	// 			InputEventMouseMotion : Mouvement de la souris.
	// 			InputEventKey : Appui sur une touche du clavier.
	// 			InputEventMouseButton : Clic de souris.
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			Vector3 rotDeg = RotationDegrees;
			rotDeg.Y -= motionEvent.Relative.X * mouseSensivity;
			RotationDegrees = rotDeg; 
		// RotationDegrees est la position angulaire du personnage
		// par rapport au "nord" ici l'axe Z car le personnage est face à 
		// l'axe Z en position initiale dans sa scène
			
			rotDeg = cameraPivot.RotationDegrees;
		// attention "cameraPivot.RotationDegrees" c'est la position angulaire 
		// de la caméra !
			rotDeg.X -= motionEvent.Relative.Y * -mouseSensivity;
			rotDeg.X = Mathf.Clamp(rotDeg.X, minPitch, maxPitch);
			cameraPivot.RotationDegrees = rotDeg;
		//
		// avec rotDeg qui enregistre la position angulaire initiale 
		// et finale du personnage. La boucle est bouclée.
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
	}
	
	private void HandleMovement(double delta)
	{
		Vector3 direction = new Vector3(0,0,0);
		
		if (Input.IsActionPressed("move_up"))
			direction += Transform.Basis.Z;
		if (Input.IsActionPressed("move_down"))
			direction -= Transform.Basis.Z;
		if (Input.IsActionPressed("move_left"))
			direction += Transform.Basis.X;
		if (Input.IsActionPressed("move_right"))
			direction -= Transform.Basis.X;
			
		if (Input.IsActionPressed("run"))
		{
			extraSpeed = 2f;
		} else {
			extraSpeed = 1f;
		}
		
		direction = direction.Normalized();

		float accel = IsOnFloor() ? acceleration : airAcceleration;
		//velocity = velocity.lerp(direction * speed, accel * delta);
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
				yVelocity = Mathf.Clamp(yVelocity-gravity, -maxTerminalVelocity, maxTerminalVelocity);
				if (animationPlayer.CurrentAnimation != "fall")
					animationPlayer.Play("fall");
			}
		}
		
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			yVelocity = jumpForce;
			if (gameState.aLeRessort)
			{
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
		
		if (gameState.aLeRessort)
		{
			jumpForce = 30f;
		}
		
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
		
		// Si c'est le dessus d'un ennemi
		if (area.Name == "Area3DTop")
		{
			GD.Print("Rebond sur ennemi !");
			bounce = true;
		}
	}
	
	private void _on_area_3d_noyade_body_entered(Node3D body)
	{
		life = 0;
		GD.Print("noyade");
		GD.Print("ENTER WATER AT ", GlobalPosition);
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
