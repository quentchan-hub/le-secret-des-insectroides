using Godot;
using System;

public partial class ButtonDemarrer : Button
{
	[Export] AnimationPlayer animationPlayer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GrabFocus();
		MouseEntered += AuSurvolSouris;
	}
	
	public void AuSurvolSouris()
	{
		GrabFocus();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	//private async void _on_button_down()
	//{
		//animationPlayer.Play("LancementJeu");
		//await ToSignal(GetTree().CreateTimer(1.2f), "timeout");
		//GetTree().ChangeSceneToFile("res://Scenes/Niveau/world_1.tscn");
	//}
}
