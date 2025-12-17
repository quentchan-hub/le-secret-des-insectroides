using Godot;
using System;

public partial class ControlInventaire : Control
{
	GameState gameState;
	[Export] TextureRect item1;
	[Export] TextureRect item2;
	[Export] TextureRect item3;
	[Export] TextureRect item4;
	[Export] TextureRect item5;
	[Export] TextureRect item6;
	
	[Export] TextureRect toucheItem1;
	[Export] TextureRect toucheItem2;
	[Export] TextureRect toucheItem3;
	[Export] TextureRect toucheItem4;
	[Export] TextureRect toucheItem5;
	[Export] TextureRect toucheItem6;
	
	[Export] ColorRect fondItem1;
	[Export] ColorRect fondItem2;
	[Export] ColorRect fondItem3;
	[Export] ColorRect fondItem4;
	[Export] ColorRect fondItem5;
	[Export] ColorRect fondItem6;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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
			fondItem2.SelfModulate  = new Color(144, 144, 144, 0.8f);
			item2.Visible = true;
		}
			
		if (gameState.aLaCleRose && !item3.Visible)
		{
			toucheItem3.Visible = true;
			fondItem3.SelfModulate = new Color(144, 144, 144, 0.8f);
			item3.Visible = true;
		}
		
		if (gameState.aLaBomba & !item4.Visible)
		{
			toucheItem4.Visible = true;
			fondItem4.SelfModulate = new Color(1, 1, 1, 0.8f);
			GD.Print("item4 soit la Bomba visible dans l'inventaire");
			item4.Visible = true;
		}
		
		if(gameState.aLeRessort & !item5.Visible)
		{
			toucheItem5.Visible = true;
			fondItem5.SelfModulate = new Color(1, 1, 1, 0.8f);
			item5.Visible = true;
		}
		
		if (gameState.aLeFouet & !item1.Visible)
		{
			toucheItem1.Visible = true;
			fondItem1.SelfModulate = new Color(1, 1, 1, 0.8f);
			item1.Visible = true;
		}
	}
}
