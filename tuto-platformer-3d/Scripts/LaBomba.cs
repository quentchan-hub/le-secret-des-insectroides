using Godot;
using System;

public partial class LaBomba : Node3D
{
	GameState GameStateScript;
	
	public override void _Ready()
	{
		GameStateScript = GetNode<GameState>("/root/GameState");
	}
	
	private void _on_area_3d_bomb_area_entered(Area3D area)
	{
		GD.Print("personnage a la bombe");
		GameStateScript.aLaBomba = true;
	}
}
