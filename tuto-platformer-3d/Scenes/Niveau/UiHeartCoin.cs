using Godot;
using System;

public partial class UiHeartCoin : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void _on_trigger_area_ui_heart_coin_area_entered(Area3D area)
	{
		if (area.Name == "Area3DPlayer")
		this.Visible = true;
	}
}
