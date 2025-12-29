using Godot;
using System;


public partial class DoorSystem2 : Node3D
{
	[Export] Label avertissementPorte;
	[Export] Label avertissementPorte2;
	private AnimationPlayer animationPlayer;
	GameState gameState;
	private string etatPorte = "fermey";

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("LargeDoor/AnimationPlayer");
		gameState = GetNode<GameState>("/root/GameState");
		animationPlayer.Play("close");
		avertissementPorte.Visible = false;
		avertissementPorte2.Visible = false;
	}
		

	private async void _on_area_3d_area_entered(Area3D area)
	{
		gameState.facingDoor2 = true;
		if (!gameState.aLaCleRose)
		{
			avertissementPorte.Visible = true;
			await ToSignal(GetTree().CreateTimer(1.5f),"timeout");
			avertissementPorte.Visible = false;
		}
		else if (gameState.aLaCleRose && etatPorte == "fermey")
		{
			avertissementPorte2.Visible = true;
			await ToSignal(GetTree().CreateTimer(1.5f),"timeout");
			avertissementPorte2.Visible = false;
		}
		else
		{
			avertissementPorte.Visible = false;
			avertissementPorte2.Visible = false;
		}
	}
	
	public override void _Process(double delta)
	{
		if (gameState.facingDoor2 && etatPorte == "fermey" && gameState.aLaCleRose == true && Input.IsActionJustPressed("action"))
		{
			animationPlayer.Play("open");
			etatPorte = "Ouverte";
		} 
	}
}
