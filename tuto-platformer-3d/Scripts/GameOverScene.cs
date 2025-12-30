using Godot;
using System;

public partial class GameOverScene : Control
{
	[Signal] public delegate void BackFromDeathEventHandler();
	
	[Export] Control prelude;
	[Export] Node3D introJeu;
	[Export] PlayerBotCtrl playerBotCtrl;
	[Export] Marker3D playerStartLVL1;
	
	GameState gameState;
	public SoundManager soundManager;
	
	private bool isInFight = false;

	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}
	
	private void _on_boss_combat_boss_fight_started()
	{
		isInFight = true;
	}
	
	private void _on_button_continue_button_down()
	{
		
		playerBotCtrl.GlobalPosition = RespawnManager.LastRespawnPoint;
		playerBotCtrl.AddLife(3);

		Visible = false;
		soundManager.EcouterExclusivement(soundManager.mainThemeMusic);
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		if (isInFight == true)
		{
			EmitSignal(SignalName.BackFromDeath);
		}
		GetTree().Paused = false;
		

	}
	
	private void _on_button_restart_button_down()
	{
		Visible = false;
		gameState.aLaCle = false;
		gameState.aLaCleRose = false;
		gameState.aLaBomba = false;

		GetTree().Paused = false;
		CallDeferred("ReloadScene"); 
		
	}
	
	public void ReloadScene()
	{
		RespawnManager.Reset();
		gameState.HardReset();
		GetTree().ReloadCurrentScene();
	}
	
	private void _on_button_quit_button_down()
	{
		GetTree().Quit();
	}
}
