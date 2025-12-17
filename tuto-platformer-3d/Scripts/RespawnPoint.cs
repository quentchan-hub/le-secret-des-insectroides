using Godot;
using System;

public partial class RespawnPoint : Marker3D
{
	[Export] Label checkPointAnnouncement; 
	private bool _checkPointactivated = false;
	
	
	//[Export] public CharacterBody3D player;
	//public Vector3 lastSpawnPoint;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		checkPointAnnouncement.Visible = false;
		_checkPointactivated = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public async void _on_area_respawn_point_1_body_entered(Node3D body)
	{
		if (body.IsInGroup("Player") && _checkPointactivated == false)
		{
			RespawnManager.SetRespawn(GlobalPosition);
			_checkPointactivated = true;
			checkPointAnnouncement.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
			checkPointAnnouncement.Visible = false;
		}
	}
	
	
}
