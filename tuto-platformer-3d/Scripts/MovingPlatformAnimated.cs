using Godot;
using System;

public partial class MovingPlatformAnimated : Node3D
{
	[Export] AnimationPlayer animationPlayer;
	
	// --- déclenché par ButtonRound1 ---
	
	public void PlayAnim()
	{
		animationPlayer.Play("block_ping_pong");
	}
}
