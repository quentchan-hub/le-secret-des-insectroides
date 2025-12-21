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
		if (!Visible & gameState.nbCoins > 0)
			{
				Visible = true;
			}
	}
	
	//public void _on_trigger_area_ui_heart_coin_area_entered(Area3D area)
	//{
		//if (area.Name == "Area3DPlayer")
		//this.Visible = true;
	//}
}
