using Godot;
using System;

public partial class Coffre : Node3D
{
	private AnimationPlayer animationPlayer;
	private bool playerNearChest = false;
	public string etatDuCoffre = "fermey";
	[Export] Label avertissementCoffre;
	public SoundManager soundManager;
	
	//apparition et animation du contenu coffre (loot) à l'écran
	[Export] public PackedScene sceneLoot;
	[Export] public float animationDuration = 1.0f; // Durée de l'animation
	[Export] public float waitDuration = 2.0f; // Durée d'attente avant la disparition en secondes
	[Export] private Timer _lootTimer; // Associe ce champ au nœud LootTimer dans l'inspecteur
	
	
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("close");
		avertissementCoffre.Visible = false;
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}
		
	private async void _on_area_3d_area_entered(Area3D area)
	{
		GD.Print($"Devant le coffre {Name}");
		playerNearChest = true;
		
		avertissementCoffre.Visible = true;
		await ToSignal(GetTree().CreateTimer(1.5f),"timeout");
		avertissementCoffre.Visible = false;
	}
	
	public override void _Process(double delta)
	{
		if (etatDuCoffre == "fermey" && playerNearChest && Input.IsActionJustPressed("action"))
		{
			//animation d'ouverture du coffre
			GD.Print("ouvre le coffre");
			animationPlayer.Play("open");
			
			//son d'ouverture
			soundManager.PlayChestOpen();
			
			//Instanciation du loot au dessus du coffre
			Node3D loot = (Node3D)sceneLoot.Instantiate();
			GetParent().AddChild(loot);
			loot.GlobalPosition = GlobalPosition + new Vector3(0, 1, 0);
			
			//maj état coffre pour empêcher la répétition de l'action
			etatDuCoffre = "ouvert";
			
			
			// Démarrer l'animation pour le loot
			StartLootAnimation(loot);
		}
	}

	private async void StartLootAnimation(Node3D loot)
	{
		float elapsed = 0f;
		Vector3 initialPosition = loot.GlobalPosition;
		Vector3 targetPosition = initialPosition + new Vector3(0, 2, 0); // Monter de 2 unités

		// Animation de montée et rotation
		while (elapsed < animationDuration)
		{
			float t = elapsed / animationDuration;
			loot.GlobalPosition = initialPosition.Lerp(targetPosition, t);
			loot.RotateY(4 * (float)GetProcessDeltaTime());
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			elapsed += (float)GetProcessDeltaTime();
		}

		// Configurer et démarrer le timer pour la disparition
		_lootTimer.Timeout += () => loot.QueueFree();
		_lootTimer.Start(waitDuration);
		
	}

	////Animation de fermeture du coffre quand on s'éloigne
	////idée finalement non retenue
	//private void _on_area_3d_area_exited(Area3D area)
	//{
		//GD.Print("Pas devant le coffre");
		//gameState.facingChest = false;
		//if (etatDuCoffre == "ouvert")
		//{
			//animationPlayer.Play("close");
			//etatDuCoffre = "fermey";
		//}
	//}
	
}
