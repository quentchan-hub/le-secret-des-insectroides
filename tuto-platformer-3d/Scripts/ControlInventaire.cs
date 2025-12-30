using Godot;
using System;

public partial class ControlInventaire : Control
{
	GameState gameState;
	
	private bool gameStarted;
	private bool showInventaire;
	
	[Export] TextureRect item1;
	[Export] TextureRect item2;
	[Export] TextureRect item3;
	[Export] TextureRect item4;
	[Export] TextureRect item5;
	[Export] TextureRect item6;
	
	[Export] ColorRect fondItem1;
	[Export] ColorRect fondItem2;
	[Export] ColorRect fondItem3;
	[Export] ColorRect fondItem4;
	[Export] ColorRect fondItem5;
	[Export] ColorRect fondItem6;
	
	[Export] TextureRect toucheItem1;
	[Export] TextureRect toucheItem2;
	[Export] TextureRect toucheItem3;
	[Export] TextureRect toucheItem4;
	[Export] TextureRect toucheItem5;
	[Export] TextureRect toucheItem6;
	
	[Export] ColorRect fondToucheItem1;
	[Export] ColorRect fondToucheItem2;
	[Export] ColorRect fondToucheItem3;
	[Export] ColorRect fondToucheItem4;
	[Export] ColorRect fondToucheItem5;
	[Export] ColorRect fondToucheItem6;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		gameStarted = false;
		
		this.Visible = false;
		item1.Visible = false;
		item2.Visible = false;
		item3.Visible = false;
		item4.Visible = false;
		item5.Visible = false;
		item6.Visible = false;
		
		fondItem1.SelfModulate = new Color(1, 1, 1, 0f);
		fondItem2.SelfModulate = new Color(1, 1, 1, 0f);
		fondItem3.SelfModulate = new Color(1, 1, 1, 0f);
		fondItem4.SelfModulate = new Color(1, 1, 1, 0f);
		fondItem5.SelfModulate = new Color(1, 1, 1, 0f);
		fondItem6.SelfModulate = new Color(1, 1, 1, 0f);
		
		toucheItem1.Visible = false;
		toucheItem2.Visible = false;
		toucheItem3.Visible = false;
		toucheItem4.Visible = false;
		toucheItem5.Visible = false;
		toucheItem6.Visible = false;
		
		fondToucheItem1.SelfModulate = new Color(1, 1, 1, 0f);
		fondToucheItem2.SelfModulate = new Color(1, 1, 1, 0f);
		fondToucheItem3.SelfModulate = new Color(1, 1, 1, 0f);
		fondToucheItem4.SelfModulate = new Color(1, 1, 1, 0f);
		fondToucheItem5.SelfModulate = new Color(1, 1, 1, 0f);
		fondToucheItem6.SelfModulate = new Color(1, 1, 1, 0f);

	}
	
	public void _on_prelude_studio_game_started()		// connecter à PreludeStudio
	{
		gameStarted = true;
	}
	
	public void _on_boss_coxane_intro_intro_boss_on(bool _on)
	{
		if (_on)
			showInventaire = false;
		else
			showInventaire = true;
			
		// Pour progression peut s'écrire en une ligne :
		// showInventaire = !Visible 
	}
	
	
	public override void _Process(double delta)
	{
		if (gameStarted && showInventaire)
		{
			if (gameState.aLaCle)
			{
				this.Visible = true;
			} 
			else
			{
				this.Visible = false;
			}
			
			if (gameState.aLaCle && !item2.Visible)
			{
				toucheItem2.Visible = true;
				fondToucheItem2.SelfModulate = new Color(1, 1, 1, 1);
				fondItem2.SelfModulate  = new Color(144, 144, 144, 0.8f);
				item2.Visible = true;
			}
				
			if (gameState.aLaCleRose && !item3.Visible)
			{
				toucheItem3.Visible = true;
				fondToucheItem3.SelfModulate = new Color(1, 1, 1, 1);
				fondItem3.SelfModulate = new Color(144, 144, 144, 0.8f);
				item3.Visible = true;
			}
			
			if (gameState.aLaBomba & !item4.Visible)
			{
				toucheItem4.Visible = true;
				fondToucheItem4.SelfModulate = new Color(1, 1, 1, 1);
				fondItem4.SelfModulate = new Color(144, 144, 144, 0.8f);
				item4.Visible = true;
			}
			
			if(gameState.aLeRessort & !item5.Visible)
			{
				toucheItem5.Visible = true;
				fondToucheItem5.SelfModulate = new Color(1, 1, 1, 1);
				fondItem5.SelfModulate = new Color(144, 144, 144, 0.8f);
				item5.Visible = true;
			}
			
			if (gameState.aLeFouet & !item1.Visible)
			{
				toucheItem1.Visible = true;
				fondToucheItem1.SelfModulate = new Color(1, 1, 1, 1);
				fondItem1.SelfModulate = new Color(144, 144, 144, 0.8f);
				item1.Visible = true;
			}
			
		}
	}
	
	private void _on_boss_combat_bombe_utilisey()
	{
		toucheItem4.Visible = false;
		fondToucheItem4.SelfModulate = new Color(1, 1, 1, 0);
		fondItem4.SelfModulate = new Color(144, 144, 144, 0f);
		item4.Visible = false;
	}
	
}
