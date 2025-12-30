using Godot;
using System;

public partial class DoorSystem3 : Node3D
{
	[Export] AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		animationPlayer.Play("close");
	}
	
	public void _on_scene_boss_lvl_1_hodor()
	{
		animationPlayer.Play("open");
	}
	public void _on_scene_boss_lvl_1_clodor()
	{
		animationPlayer.Play("close");
	}
	
	public void _on_boss_combat_combat_fini()
	{
		animationPlayer.Play("open");
	}
}
