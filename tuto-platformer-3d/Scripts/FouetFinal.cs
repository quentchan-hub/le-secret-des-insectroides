using Godot;
using System;

public partial class FouetFinal : Node3D
{
	GameState GameStateScript;
	
	public override void _Ready()
	{
		GameStateScript = GetNode<GameState>("/root/GameState");
	}
	
	private void _on_area_3d_fouet_area_entered(Area3D area)
	{
		GD.Print("personnage a l'item secret = fouet");
		GameStateScript.aLeFouet = true;
	}
}
