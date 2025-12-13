using Godot;
using System;

public partial class MobSharkFinal : Marker3D
{
	[Export] private AnimationPlayer _animationShark;
	[Export] private Area3D _sharkDetectionArea;
	[Export] private Area3D _areaDamage;
	[Export] public PlayerBotCtrl player;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animationShark.Play("StartPose");
	}

	private void _on_shark_detection_area_body_entered(Node3D body)
	{
		if (body.Name == "PlayerBotCtrl")
		{
			_animationShark.Play("Saut");
		}
	}
	
	private void _on_shark_detection_area_body_exited(Node3D body)
	{
		if (body.Name == "PlayerBotCtrl")
		{
			_animationShark.Play("StartPose");
		}
	}
	
	private void _on_area_damage_area_entered(Area3D area)
	{
		if (area.Name == "Area3DDommage")
		{
			player.TakeDamages();
			GD.Print("1 coup infligé par Sharky ! Vie restante : " + player.life);
		}
	}
	

	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
