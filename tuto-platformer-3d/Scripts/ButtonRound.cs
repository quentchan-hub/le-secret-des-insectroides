using Godot;
using System;

public partial class ButtonRound : Node3D
{
	[Export] public MovingPlatformAnimated platform;
	[Export] SoundManager soundManager;
	
	private AnimationPlayer animationPlayer;
	
	public bool toggled = false;
	
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		// Force le bouton en position haute au démarrage
		animationPlayer.Play("toggle-on");
		animationPlayer.Seek(0.0, true); 		// Va au début de l'animation
		animationPlayer.Stop(); 				// Arrête l'animation
	}
		
	private void _on_area_3d_area_entered(Area3D area)
	{
		if (toggled == false)
		{
			soundManager.PlayButtonRound();
			animationPlayer.Play("toggle-on");
			platform.PlayAnim();
			toggled = true;
		}
		
	}
}
