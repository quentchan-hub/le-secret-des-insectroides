using Godot;
using System;

public partial class GameOverScene : Control
{
	[Export] Control prelude;
	[Export] Node3D introJeu;
	[Export] PlayerBotCtrl playerBotCtrl;
	GameState gameState;
	public SoundManager soundManager;
	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		
	}

	
	private void _on_button_continue_button_down()
	{
		//gameState.PlayerSpawn();
		playerBotCtrl.GlobalPosition = RespawnManager.LastRespawnPoint;
		playerBotCtrl.AddLife(3);
		//GD.Print("RESTART / COINS = 0");
		//gameState.nbCoins = 0;
		Visible = false;
		soundManager.EcouterExclusivement(soundManager.mainThemeMusic);
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}
	
	private void _on_button_restart_button_down()
	{
		Visible = false;
		GetTree().Paused = false;
		CallDeferred("ReloadScene"); 
		
	}
	
	public void ReloadScene()
	{
		GetTree().ReloadCurrentScene();
	}
	
	private void _on_button_quit_button_down()
	{
		GetTree().Quit();
	}
}
