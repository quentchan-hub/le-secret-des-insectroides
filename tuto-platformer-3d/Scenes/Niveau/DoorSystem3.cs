using Godot;
using System;

public partial class DoorSystem3 : Node3D
{
	[Export] AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		animationPlayer.Play("close");
	}

	public void _on_boss_combat_combat_fini()
	{
		animationPlayer.Play("open");
	}
}
