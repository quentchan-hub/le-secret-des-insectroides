using Godot;
using System;

public partial class UiControlHeartCoin : Control
{
	public GameState gameState;

	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		this.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
	
	public void _on_piece_or_show_coin_score()	 	// Signal emit par PieceOr
	{
		if (!Visible & gameState.nbCoins > 0)
		{
			Visible = true;
		}
	}
	
}
