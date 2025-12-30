using Godot;
using System;

public partial class EndLevel : Area3D
{
	GameState gameState;
	[Export] public Control endLevelScreen;
	[Export] public Button playAgain;
	[Export] public Label scoreLabel;
	
	private string karma;
	
	public override void _Ready()
	{
		endLevelScreen.Hide();
		gameState = GetNode<GameState>("/root/GameState");
		
	}
	
	private void _on_body_entered(Node3D body)
	{
		if (!body.IsInGroup("Player"))
			return;
		
		GD.Print(body.Name);
		GD.Print("bravo ! niveau terminé !");

		gameState.PrintScore();
		
		int finalScoreLvl1 = gameState.scoreLvl1;
		
		if (gameState.bossDead == true)
		{
			karma = "ennemi";
		}
		else
		{
			karma = "allié";
		}
		
		scoreLabel.Text = $"Score Final : {finalScoreLvl1} \n Vous êtes considéré comme un {karma} des insectroïdes";
		
		endLevelScreen.Show();
	
		
		Input.MouseMode = Input.MouseModeEnum.Visible; // fait apparaitre la souris
		playAgain.GrabFocus(); // permet d'appuyer sur le bouton avec la touche Entrée
	}
}
