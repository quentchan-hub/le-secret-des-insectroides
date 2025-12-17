using Godot;
using System;

public partial class FouetFinal : Node3D
{
	GameState gameState;
	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
	}
	
	private void _on_area_3d_fouet_area_entered(Area3D area)
	{
		GD.Print("personnage a l'item secret = fouet");
		gameState.aLeFouet = true;
	}
}
