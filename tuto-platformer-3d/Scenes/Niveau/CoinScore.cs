using Godot;
using System;

public partial class CoinScore : Label
{
	public GameState gameState;
	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		
	}

	public override void _Process(double delta)
	{
		Text = $"{gameState.nbCoins}/{gameState.totalPiecesOr}";
	}
}
