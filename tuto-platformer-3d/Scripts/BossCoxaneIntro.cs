using Godot;
using System;

public partial class BossCoxaneIntro : Node3D
{
	[Signal] public delegate void IntroFinieEventHandler(); 
	[Signal] public delegate void IntroBossOnEventHandler(bool _on);
	
	[Export] AnimationPlayer AnimPlayerIntroCombat;
	[Export] AnimationPlayer animBoss;
	[Export] Control uIIntroCombat;
	[Export] Camera3D cameraIntroCombat;
	
	public override void _Ready()
	{
		
	}
	
	public override void _Process(double delta)
	{

	}
	
	public void Activate()
	{
		Visible = true;
		ProcessMode = ProcessModeEnum.Inherit;
		EmitSignal(SignalName.IntroBossOn, true);
		LancerIntro();
	}
	
	public void Deactivate()
	{
		ProcessMode = ProcessModeEnum.Disabled;
		Visible = false;
		EmitSignal(SignalName.IntroBossOn, false);
	}
	
	public void LancerIntro()
	{
		// Apparition Ui cinématique (dialogue, fade out)
		uIIntroCombat.Visible = true;
		
		animBoss.Play("Idle");
		AnimPlayerIntroCombat.Play("DescenteBoss");
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	
	// Permet à AnimPlayerIntroCombat de jouer les animation du Boss 
	public void DecolVol() => animBoss.Play("Décollage&Vol");
	public void Vol() => animBoss.Play("Vol");
	public void StopVol() => animBoss.Stop();
	public void VolAtter() => animBoss.Play("Vol&Atterrissage");
	public void Atter() => animBoss.Play("Atterrissage");
	
	// Signal envoyé à SceneBossLvl1 => Animation "DescenteBoss" terminé
	public void _on_anim_player_intro_combat_animation_finished(StringName AnimName)
	{
		if (AnimName == "DescenteBoss")
		{
			EmitSignal(SignalName.IntroFinie);
		}
	}
	
	//================ SKIP INTRO ==================//
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			animBoss.Stop();
			AnimPlayerIntroCombat.Stop();
			cameraIntroCombat.Current = false;
			cameraIntroCombat.Visible = false;
			uIIntroCombat.Visible = false;
			GetTree().CreateTimer(0.3f).Timeout += () => EmitSignal(SignalName.IntroFinie);
			GD.Print("Intro zappée");
		}
	}
	
}
