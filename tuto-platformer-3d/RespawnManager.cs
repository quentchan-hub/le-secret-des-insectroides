using Godot;
using System;

public partial class RespawnManager : Node
{
	public static Vector3 LastRespawnPoint;

	public static void SetRespawn(Vector3 pos)
	{
		LastRespawnPoint = pos;
	}
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
