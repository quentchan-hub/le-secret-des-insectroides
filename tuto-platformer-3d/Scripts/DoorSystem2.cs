using Godot;
using System;

public partial class DoorSystem2 : Node3D
{
	private AnimationPlayer animationPlayer;
	GameState GameStateScript;
	private string etatPorte = "fermey";

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("LargeDoor/AnimationPlayer");
		GameStateScript = GetNode<GameState>("/root/GameState");
		animationPlayer.Play("close");
	}
		

	private void _on_area_3d_area_entered(Area3D area)
	{
		GD.Print("devant la porte");
		GameStateScript.facingDoor = true;
	}
	
	public override void _Process(double delta)
	{
		if (GameStateScript.facingDoor && etatPorte == "fermey" && GameStateScript.aLaCleRose == true && Input.IsActionJustPressed("action"))
		{
			animationPlayer.Play("open");
			GD.Print("la porte s'ouvre");
			etatPorte = "Ouverte";
			GD.Print("La porte est " + etatPorte);
		} 
	}
}
