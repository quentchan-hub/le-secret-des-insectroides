using Godot;
using System;

public partial class PieceOr : Node3D
{
	GameState gameState;
	public SoundManager soundManager;

	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}
	
	public override void _Process(double delta)
	{
		RotateY((float)delta * 5);
	}
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		if (!body.IsInGroup("Player"))
			return;
		else
		{
			soundManager.PlayCoinPick();
			gameState.nbCoins++;
			QueueFree();
		}
	}
}
