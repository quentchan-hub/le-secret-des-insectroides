using Godot;
using System;

public partial class CoffresBonus : Node
{
	[Export] Coffre coffreRessort;
	[Export] Coffre coffreBomba;
	[Export] public Node coinsNode;
	
	[Export] Control controlCoins;
	[Export] Label coinScore;
	[Export] Label coffresBonusDebloques;
	
	[Export] Timer timerMessage;
	
	GameState gameState;
	
	private int totalPieces;
	public int piecesRecoltees = 0;
	
	private bool coffreRessortDisparu = false;
	private bool coffreBombaDisparu = false;
	
	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		
		coffresBonusDebloques.Visible = false;
		piecesRecoltees = gameState.nbCoins;
		CompterPieces(); //combien pièces total instanciées in game
	}
	
	private void InitialiserCoffres()
	{
		coffreRessort.Visible = false;
		coffreBomba.Visible = false;
		coffreRessort.ProcessMode = Node3D.ProcessModeEnum.Disabled;
		coffreBomba.ProcessMode = Node3D.ProcessModeEnum.Disabled;



	}
	
	public void CompterPieces()
	{
		totalPieces = 0;

		foreach (Node child in coinsNode.GetChildren())
		{
			if (child.Name.ToString().Contains("PieceOr"))
			{
					totalPieces++;
			}
		}
		GD.Print("Nb pieces instanciées Niveau 1 = " + totalPieces);
	}

	
	public async void ApparaitreCoffres()
	{
		if (!coffreBomba.Visible & !coffreRessort.Visible & piecesRecoltees >= totalPieces)
		{
			coffreRessort.ProcessMode = Node3D.ProcessModeEnum.Inherit;
			coffreBomba.ProcessMode = Node3D.ProcessModeEnum.Inherit;
			coffreRessort.Visible = true;
			
			coffreBomba.Visible = true;
			GD.Print("Bravo tu as récupéré toutes les pièces d'or ! ");
			GD.Print("2 coffres Bonus sont apparus, choisis-en 1 !");
			
			coffresBonusDebloques.Visible = true;
			await ToSignal(GetTree().CreateTimer(2.5f),"timeout");
			coffresBonusDebloques.Visible = false;

		}
	}
	
	public void DisparaitreAutreCoffre()
	{
		if (coffreRessort.etatDuCoffre == "ouvert" && !coffreBombaDisparu)
		{
			gameState.aLeRessort = true;
			coffreBomba.Visible = false;
			coffreBombaDisparu = true;
			coffreBomba.QueueFree();
			GD.Print("coffre Bomb disparu");
		}
		else
		{
			if (coffreBomba.etatDuCoffre == "ouvert" && !coffreRessortDisparu)
			{
				gameState.aLaBomba = true;
				coffreRessort.Visible = false;
				coffreRessortDisparu = true;
				coffreRessort.QueueFree();
				GD.Print("coffre Ressort disparu");
			}
		}
	}
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ApparaitreCoffres();
		
		if (!IsInstanceValid(coffreRessort) && !IsInstanceValid(coffreBomba))
		{
			DisparaitreAutreCoffre();
		}
		
		if (!controlCoins.Visible & piecesRecoltees > 0)
		{
			controlCoins.Visible = true;
		}
		coinScore.Text = piecesRecoltees + "/" + totalPieces;
	}
}
