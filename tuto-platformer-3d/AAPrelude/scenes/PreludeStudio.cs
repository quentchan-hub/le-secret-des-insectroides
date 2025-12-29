using Godot;
using System;

public partial class PreludeStudio : Control
{
	[Signal] public delegate void GameStartedEventHandler();
	
	[Export] public AnimationPlayer animationPlayer;
	[Export] public Node3D introJeu;
	[Export] public PackedScene IntroJeuBoss; 			// Scène exportée
	
	public SoundManager soundManager;

	
	public override void _Ready()
	{
		Visible = true;
				
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		soundManager.PlayMenuTheme();
		animationPlayer.Play("fondu");
		
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _Process(double delta)
	{
	}

	public void _on_button_demarrer_button_down()
	{
		animationPlayer.Play("LancementJeu");
		
		Visible = false;
		EmitSignal(SignalName.GameStarted);
		
		ProcessMode = Node.ProcessModeEnum.Disabled;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		Node3D introJeuBossInstance = (Node3D)IntroJeuBoss.Instantiate();
		introJeu.AddChild(introJeuBossInstance);
	}
}
