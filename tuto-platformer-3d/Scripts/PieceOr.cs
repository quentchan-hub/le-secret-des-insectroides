using Godot;
using System;

public partial class PieceOr : Node3D
{
	GameState GameStateScript;
	public SoundManager soundManager;
	CoffresBonus coffresBonus; 
	
	
		
	public override void _Ready()
	{
		GameStateScript = GetNode<GameState>("/root/GameState");
		coffresBonus = GetNode<CoffresBonus>("../../InteractItems/CoffresBonus");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
	
	}
	
	public override void _Process(double delta)
	{
		RotateY((float)delta * 5);
	}
	
	private void _on_area_3d_area_entered(Area3D area)
	{
		if (area.Name == "Area3DPlayer")
		{
			soundManager.PlayCoinPick();
			GameStateScript.nbCoins++;
			coffresBonus.piecesRecoltees++;
			QueueFree();
		}
	}
	
	
}
