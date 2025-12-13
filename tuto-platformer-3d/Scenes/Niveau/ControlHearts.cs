using Godot;
using System;

public partial class ControlHearts : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body is PlayerBotCtrl playerType)
		{
			this.Visible = true;
		}
	}
	
	
}
