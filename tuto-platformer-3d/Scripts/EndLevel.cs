using Godot;
using System;

public partial class EndLevel : Area3D
{
	GameState GameStateScript;
	[Export] public Control endLevelScreen;
	[Export] public Button playAgain;
	[Export] public Label scoreLabel;
	
	public override void _Ready()
	{
		endLevelScreen.Hide();
		GameStateScript = GetNode<GameState>("/root/GameState");
		
	}
	
	private void _on_area_entered(Area3D area)
	{
		
		GD.Print("bravo ! niveau terminé !");

		GameStateScript.PrintScore();
		
		int finalScoreLvl1 = GameStateScript.scoreLvl1;
		scoreLabel.Text = $"Score Final : {finalScoreLvl1}";
		
		endLevelScreen.Show();
	
		
		Input.MouseMode = Input.MouseModeEnum.Visible; // fait apparaitre la souris
		playAgain.GrabFocus(); // permet d'appuyer sur le bouton avec la touche Entrée
	}
}
