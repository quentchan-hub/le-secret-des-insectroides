using Godot;
using System;

public partial class GameState : Node
{
	
	public bool facingDoor = false;
	public bool aLaCle = false;
	public bool aLaCleRose = false;
	public bool aLaBomba = false;
	public bool aLeRessort = false;
	public bool aLeFouet = false; 
	public int destroyedMobs = 0;
	public int nbCoins = 0;
	public static string LastDamageSource { get; set; }
	public int scoreLvl1 = 0;
	public Control preludeStudio; 
	public Node3D introJeu;
	public bool zoneFeu = false; 
	public Vector3 playerStartPosition;

	
	public void PrintScore()
	{
		
		scoreLvl1 += destroyedMobs * 100;
		scoreLvl1 += nbCoins * 10;
		
		GD.Print("Score final " + scoreLvl1);
		
	}
	
	public void PlayerSpawn()
	{
		var player = GetNode<PlayerBotCtrl>("/root/World1/PlayerBotCtrl");
		player.GlobalPosition = playerStartPosition;
	}
	
}
