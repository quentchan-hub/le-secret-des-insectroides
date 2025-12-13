using Godot;
using System;

public partial class ButtonOptions : Button
{
	[Export] Control optionWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		optionWindow.Visible = false;
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
	public void _on_pressed()
	{
		optionWindow.Visible = true;
	}
}
