using Godot;
using System;


public partial class DoorSystem : Node3D
{
	[Export] Label avertissementPorte;
	[Export] Label avertissementPorte2;
	private AnimationPlayer animationPlayer;
	GameState GameStateScript;
	private string etatPorte = "fermey";

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("LargeDoor/AnimationPlayer");
		GameStateScript = GetNode<GameState>("/root/GameState");
		animationPlayer.Play("close");
		avertissementPorte.Visible = false;
		avertissementPorte2.Visible = false;
	}
		

	private async void _on_area_3d_area_entered(Area3D area)
	{
		GD.Print("devant la porte");
		
		GameStateScript.facingDoor = true;
		if (!GameStateScript.aLaCle)
		{
			avertissementPorte.Visible = true;
			await ToSignal(GetTree().CreateTimer(1.5f),"timeout");
			avertissementPorte.Visible = false;
		}
		else if (GameStateScript.aLaCle && etatPorte == "fermey")
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
		if (GameStateScript.facingDoor && etatPorte == "fermey" && GameStateScript.aLaCle == true && Input.IsActionJustPressed("action"))
		{
			animationPlayer.Play("open");
			GD.Print("la porte s'ouvre");
			etatPorte = "Ouverte";
			GD.Print("La porte est " + etatPorte);
		} 
	}
}
