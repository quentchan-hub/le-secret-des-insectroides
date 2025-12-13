using Godot;
using System;

public partial class OptionWindow : Control
{
	public SoundManager soundManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void _on_close_button_button_down()
	{
		this.Visible = false;
	}
	
	public void _on_music_volume_slider_value_changed(float value)
	{
		soundManager.SetMusicVolume(value);
		GD.Print("la valeurdu slide est de : " + value);
	}
	
	public void _on_music_mute_button_toggled(bool ToggledOn)
	{
		soundManager.MuteMusic(ToggledOn);
	}
	
	public void _on_fx_volume_slider_value_changed(float value)
	{
		soundManager.SetFXVolume(value);
		soundManager.PlayPlayerDamaged();
	}
	
	public void _on_fx_mute_button_toggled(bool ToggledOn)
	{
		soundManager.MuteFX(ToggledOn);
	}

	
}
