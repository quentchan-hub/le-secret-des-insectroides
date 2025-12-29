using Godot;
using System;

public partial class Coffre : Node3D
{
	[Signal] public delegate void CoffreOuvertEventHandler(Coffre coffre);
	
	// ------- apparition et animation du contenu coffre (loot) à l'écran ------
	[Export] public PackedScene sceneLoot;
	[Export] public float animationDuration = 1.0f; 		// Durée de l'animation
	[Export] public float waitDuration = 2.0f; 				// Durée d'attente avant la disparition en secondes
	[Export] private Timer _lootTimer; 		
	
	[Export] Label avertissementCoffre;						// indique comment ouvrir coffre
	
	private AnimationPlayer animationPlayer;				// gère ouverture/fermeture coffre	
	private SoundManager soundManager;						// son ouverture coffre
	
	
	//------- Variables internes ----------

	private bool playerNearChest = false;
	private bool estOuvert = false;					

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
		animationPlayer.Play("close");
		avertissementCoffre.Visible = false;
	}
		
	private void _on_area_3d_area_entered(Area3D area)
	{
		GD.Print($"Devant le coffre {Name}");
		playerNearChest = true;
		
		if (!estOuvert)
			avertissementCoffre.Visible = true;

			GetTree().CreateTimer(1.5f).Timeout += () =>
			{
				avertissementCoffre.Visible = false;
			};
	}
	
	public override void _Process(double delta)
	{
		if (estOuvert)
			return;

		if (playerNearChest && Input.IsActionJustPressed("action"))
		{
			OuvrirCoffre();
		}
	}	
	
	private void OuvrirCoffre()
	{
		estOuvert = true;

		animationPlayer.Play("open");
		soundManager.PlayChestOpen();

		Node3D loot = (Node3D)sceneLoot.Instantiate();
		GetParent().AddChild(loot);
		loot.GlobalPosition = GlobalPosition + Vector3.Up;	//Vector3.Up = new Vector3(0, 1, 0)

		EmitSignal(SignalName.CoffreOuvert, this);

		// Démarrer l'animation pour le loot
		StartLootAnimation(loot);
	}

	// ----- Animation de montée et rotation du loot -----
	private async void StartLootAnimation(Node3D loot)
	{
		float elapsed = 0f;
		Vector3 initialPosition = loot.GlobalPosition;
		Vector3 targetPosition = initialPosition + new Vector3(0, 2, 0); // Monter de 2 unités

		while (elapsed < animationDuration)
		{
			float t = elapsed / animationDuration;
			loot.GlobalPosition = initialPosition.Lerp(targetPosition, t);
			loot.RotateY(4 * (float)GetProcessDeltaTime());
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			elapsed += (float)GetProcessDeltaTime();
		}

		// Configurer et démarrer le timer pour la disparition
		_lootTimer.Timeout += () => loot.QueueFree();			// on prévoit d'abord la finalité du Timer
		_lootTimer.Start(waitDuration);							// puis on le démarre (avec un délai avant activation)
		
	}
}
