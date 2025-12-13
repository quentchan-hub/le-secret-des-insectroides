using Godot;
using System;

public partial class CoffresBonus : Node
{
	[Export] Coffre coffreRessort;
	[Export] Coffre coffreBomba;
	[Export] public Node coinsNode;
	[Export] Label coinScore;
	[Export] Control controlCoins;
	[Export] Label coffresBonusDebloques;
	GameState GameStateScript;
	private int totalPieces;
	public int piecesRecoltees = 0;
	private bool coffreRessortDisparu = false;
	private bool coffreBombaDisparu = false;
	
	
		
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameStateScript = GetNode<GameState>("/root/GameState");
		coffreRessort.ProcessMode = Node3D.ProcessModeEnum.Disabled;
		coffreBomba.ProcessMode = Node3D.ProcessModeEnum.Disabled;
		coffreRessort.Visible = false;
		coffreBomba.Visible = false;
		piecesRecoltees = GameStateScript.nbCoins;
		CompterPieces(); //combien pièces total instanciées in game
		controlCoins.Visible = false;
		coffresBonusDebloques.Visible = false;
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
		if (coffreRessort.etatDuCoffre == "ouvert" & !coffreBombaDisparu)
		{
			GameStateScript.aLeRessort = true;
			coffreBomba.Visible = false;
			coffreBombaDisparu = true;
			GD.Print("coffre Bomb disparu");
		}
		else
		{
			if (coffreBomba.etatDuCoffre == "ouvert" & !coffreRessortDisparu)
			{
				GameStateScript.aLaBomba = true;
				coffreRessort.Visible = false;
				coffreRessortDisparu = true;
				GD.Print("coffre Ressort disparu");
			}
		}
	}
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ApparaitreCoffres();
		DisparaitreAutreCoffre();
		if (!controlCoins.Visible & piecesRecoltees > 0)
			{
				controlCoins.Visible = true;
			}
		coinScore.Text = piecesRecoltees + "/" + totalPieces;
	}
}
