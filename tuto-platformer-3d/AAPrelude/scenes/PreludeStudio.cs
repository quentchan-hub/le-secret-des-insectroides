using Godot;
using System;

public partial class PreludeStudio : Control
{
	[Export] public AnimationPlayer animationPlayer;
	[Export] public Node3D introJeu;
	[Export] public PackedScene IntroJeuBoss; // Scène exportée
	public SoundManager soundManager;

	public override void _Ready()
	{
		Visible = true;
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		soundManager.PlayMenuTheme();
		animationPlayer.Play("fondu");
		GD.Print("animation fondu");
		Input.MouseMode = Input.MouseModeEnum.Visible;
		
	}

	public override void _Process(double delta)
	{
	}

	public void _on_button_demarrer_button_down()
	{
		animationPlayer.Play("LancementJeu");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		Node3D introJeuBossInstance = (Node3D)IntroJeuBoss.Instantiate();
		introJeu.AddChild(introJeuBossInstance);
		GD.Print("IntroJeuBoss instanciée comme enfant de IntroJeu");
		
		//// Methode 1 propre efficace. Eteint et désactive la scène.
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.Disabled;
		
		//// Methode 2 bourrin ! en cas de conflit entre UIs
		//this.QueueFree();  
		//GD.Print("PreludeStudio supprimé");
	}

	//public async void _on_animation_player_animation_finished (StringName animName)
	//{
		//if (animName == "LancementJeu")
		//{
			//await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
			//Node3D introJeuBossInstance = (Node3D)IntroJeuBoss.Instantiate();
			//introJeu.AddChild(introJeuBossInstance);
			//GD.Print("IntroJeuBoss instanciée comme enfant de IntroJeu");
		//
			//this.QueueFree();
			//GD.Print("PreludeStudio supprimé");
		//}
	//}

}
