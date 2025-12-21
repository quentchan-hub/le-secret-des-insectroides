using Godot;
using System;

public partial class SceneBossLvl1 : Node3D
{
	[Export] Area3D porteBoss1;
	//[Export] BossIntroSceneFinal bossIntroSceneFinal;
	[Export] BossCoxaneIntro bossCoxaneIntro;
	[Export] BossCombat bossCombat;
	[Export] PlayerBotCtrl playerBotCtrl;
	[Export] Marker3D positionJoueurCombat;
	[Export] Control controlInventaire;
	[Export] Control uIControlHeartCoin;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bossCoxaneIntro.Deactivate();
		bossCoxaneIntro.Connect(BossCoxaneIntro.SignalName.IntroFinie,
		new Callable(this, nameof(TransitionVersCombat)));
		
		bossCombat.Deactivate();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void _on_porte_boss_1_area_entered(Area3D area)
	{
		if (area.Name == "Area3DPlayer")
		{
			MovePlayerToBossFight();					// teleporte le perso dans l'arène
			
			bossCoxaneIntro.Activate();					// démarre la  séquence d'intro du combat
			
			playerBotCtrl.Visible = false;				// disparition personnage
			controlInventaire.Visible = false;			// et ui game
			uIControlHeartCoin.Visible = false;			// pendant la cinématique
			
			CallDeferred("QuitAreaPorteBoss");
		}
	}
	
	public void MovePlayerToBossFight()
	{
		Vector3 rotInit = playerBotCtrl.Rotation;
		playerBotCtrl.GlobalPosition = positionJoueurCombat.GlobalPosition;
		playerBotCtrl.Rotation = rotInit + new Vector3(0, Mathf.DegToRad(-90),0);
	}
	
	
	public void QuitAreaPorteBoss()
	{
		porteBoss1.ProcessMode = Area3D.ProcessModeEnum.Disabled;
	}
	
	
	public void TransitionVersCombat()
	{
		bossCoxaneIntro.Deactivate();
		bossCombat.Activate();
		controlInventaire.Visible = true;
		uIControlHeartCoin.Visible = true;
		playerBotCtrl.Visible = true;
	}
}
