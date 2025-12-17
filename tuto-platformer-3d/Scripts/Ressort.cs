using Godot;
using System;

public partial class Ressort : Node3D
{
	GameState gameState;
	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
	}
	
	private void _on_area_3d_ressort_area_entered(Area3D area)
	{
		GD.Print("personnage a le ressort");
		gameState.aLeRessort = true;
	}
}
