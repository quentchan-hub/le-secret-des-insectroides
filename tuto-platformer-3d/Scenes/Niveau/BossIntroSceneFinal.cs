using Godot;
using System;

public partial class BossIntroSceneFinal : Node3D
{
	[Export] Area3D porteBoss1;
	[Export] public PackedScene bossCoxaneIntro;
	[Export] AnimationPlayer animationPlayerBossGate;
	[Export] CharacterBody3D playerBotCtrl;
	[Export] Marker3D positionJoueurCombat;
	
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
	public void _on_area_3d_area_entered(Area3D area)
	{
		if (area.Name == "Area3DPlayer")
		{
			Vector3 rotInit = playerBotCtrl.Rotation;
			GD.Print("detecte joueur");
			Node instanceIntro = bossCoxaneIntro.Instantiate();
			AddChild(instanceIntro);
			playerBotCtrl.GlobalPosition = positionJoueurCombat.GlobalPosition;
			playerBotCtrl.Rotation = rotInit +new Vector3(0, Mathf.DegToRad(-90),0);
			CallDeferred("QuitArea");
		}
		
	}
	
	public async void QuitArea()
	{
		await ToSignal(GetTree().CreateTimer(3f),"timeout");
		porteBoss1.ProcessMode = Area3D.ProcessModeEnum.Disabled;
	}
}
