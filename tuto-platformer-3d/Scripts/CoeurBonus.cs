using Godot;
using System;

public partial class CoeurBonus : Node3D
{
	public SoundManager soundManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RotateY((float)delta * 5);
	}
	
	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body is PlayerBotCtrl playerScript)
		{
			soundManager.PlayHeartPick();
			playerScript.AddLife(1);
			QueueFree();
		}
		
	}
	
	
}
