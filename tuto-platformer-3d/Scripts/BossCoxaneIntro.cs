using Godot;
using System;

public partial class BossCoxaneIntro : Node3D
{
	[Export] AnimationPlayer AnimPlayerIntroCombat;
	[Export] AnimationPlayer animBoss;
	[Export] public PackedScene bossCombat;

	
	
	public override void _Ready()
	{
		var player = GetNode<CharacterBody3D>("/root/World1/PlayerBotCtrl");
		player.Visible = false;
		var inventaire = GetNode<Control>("/root/World1/ControlInventaire");
		inventaire.Visible = false;
		var piecesEtVie = GetNode<Control>("/root/World1/UIControlHeartCoin");
		piecesEtVie.Visible = false;
		animBoss.Play("Idle");
		AnimPlayerIntroCombat.Play("DescenteBoss");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("skip_intro"))
		{
			LibererScene();
		}
	}
	
	public void DecolVol()
	{
		animBoss.Play("Décollage&Vol");
	}
	
	public void Vol()
	{
		animBoss.Play("Vol");
	}
	
		public void StopVol()
	{
		animBoss.Stop();
	}
	
	public void VolAtter()
	{
		animBoss.Play("Vol&Atterrissage");
	}
	
	public void Atter()
	{
		animBoss.Play("Atterrissage");
	}
	
	public void _on_anim_player_intro_combat_animation_finished(StringName AnimName)
	{
		if (AnimName == "DescenteBoss")
		{
			GD.Print("animation intro combat final finie");
			
			CallDeferred("LibererScene");
			
		}
	}
	public async void LibererScene()
	{
			await ToSignal(GetTree().CreateTimer(3f),"timeout");
			Node3D instanceCombat = (Node3D)bossCombat.Instantiate();
			var combatFinal = GetNode<Node>("../../CombatFinal");
			combatFinal.AddChild(instanceCombat);
			
			var player = GetNode<CharacterBody3D>("/root/World1/PlayerBotCtrl");
			player.Visible = true;
			var inventaire = GetNode<Control>("/root/World1/ControlInventaire");
			inventaire.Visible = true;
			var piecesEtVie = GetNode<Control>("/root/World1/UIControlHeartCoin");
			piecesEtVie.Visible = true;
			this.QueueFree();
	}
}
