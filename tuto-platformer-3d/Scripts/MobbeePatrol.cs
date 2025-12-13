using Godot;
using System;

public partial class MobbeePatrol : Node3D
{
	[Export] MeshInstance3D bee_Bot2;
	public StandardMaterial3D flashHitMat;
	[Export] AnimationPlayer aniPlayer;
	[Export] AnimationPlayer aniPlayerBee;
	[Export] public int life = 3;
	[Export] public float speed = 2.0f;
	[Export] public PlayerBotCtrl noeudPersonnage;
	private Vector3 startingPoint;
	public bool isCharInZone = false;
	public SoundManager soundManager;

	public override void _Ready()
	{
		aniPlayerBee = GetNode<AnimationPlayer>("Mobbee/BeebotSkin/bee_bot/AnimationPlayer");
		aniPlayer.Play("Patrouille");
		aniPlayerBee.Play("Idle");
		startingPoint = GlobalPosition;
		
		flashHitMat = new StandardMaterial3D();
		flashHitMat.AlbedoColor = new Color(1, 0, 0, 1);
		
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
	}

	private void _on_area_3d_top_area_entered(Area3D area)
	{
		if (noeudPersonnage.Velocity.Y >= 0)
			return;
		
		// Récupérer le parent qui est un Body ou CharacterBody3D
		Node parentNode = GetParent().GetParent();

		if (parentNode != null)
			GD.Print("Area parent: " + parentNode.Name);
		else
			GD.Print("Aucun parent trouvé pour l'Area détectrice.");
		
		if (area.Name == "Area3DPlayer")
	{
				life--;
		HitFlash();
		GD.Print("PV restants : " + life);
		if (life <= 0)
		{
			soundManager.PlayDeathMob();
			Die();
			var worldScript1 = GetNode<GameState>("/root/GameState");
			worldScript1.destroyedMobs += 2;
		}
		else
		{
			soundManager.PlayMobDamaged2();
		}
	}

	}	
	public async void HitFlash()
	{
		bee_Bot2.MaterialOverride = flashHitMat;
		await ToSignal(GetTree().CreateTimer(0.3), "timeout");
		bee_Bot2.MaterialOverride = null;
	}

	
	public async void Die()
	{
		GD.Print("Mobbee est mort");
		aniPlayer.Pause();
		aniPlayerBee.Play("power_off");
		await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
		QueueFree();
	}

	public void resetPosition()
	{
		GlobalPosition = startingPoint;
	}

	public override void _Process(double delta)
	{
		// Ici tu peux ajouter la logique de déplacement si besoin
	}


	private void _on_area_3d_bott_area_entered(Area3D area)
	{
		if (area.Name == "Area3DDommage")
		{
			if (noeudPersonnage != null)
			{
				noeudPersonnage.TakeDamages();
				GD.Print("Joueur touché par Mobbee");
				GD.Print($"Personnage a encore {noeudPersonnage.life} vies restantes");
			}
		}
	}
}
