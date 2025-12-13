using Godot;
using System;

public partial class PlatfPingPongV : Node3D
{
	private AnimationPlayer _animationPlayer;
	private Area3D _area3D;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("PlatfPiege/AnimationPlayer");
		_area3D = GetNode<Area3D>("PlatfPiege/block-moving2/Area3D");
		_animationPlayer.Play("PingPongV");
	}

}
