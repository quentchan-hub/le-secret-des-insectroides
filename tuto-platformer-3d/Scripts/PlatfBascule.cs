using Godot;
using System;

public partial class PlatfBascule : Node3D
{
	[Export] private Area3D _zoneDetection;
	[Export] private AnimationPlayer _animationPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		if (body.Name == "PlayerBotCtrl")
		{
			_animationPlayer.Play("Baskule");
		}
	}
}
