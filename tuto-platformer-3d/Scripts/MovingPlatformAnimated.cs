using Godot;
using System;

public partial class MovingPlatformAnimated : Node3D
{
	//private AnimationPlayer animationPlayer;
	[Export] AnimationPlayer animationPlayer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//animationPlayer = a<AnimationPlayer>("AnimationPlayer");
		//animationPlayer.Stop();
	}
	
	public void PlayAnim()
	{
		GD.Print("bouton déclenche platform");
		animationPlayer.Play("block_ping_pong");
	}
	


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
