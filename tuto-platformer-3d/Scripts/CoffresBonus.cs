using Godot;
using System;

public partial class CoffresBonus : Node
{
	[Export] private Coffre coffreRessort;
	[Export] private Coffre coffreBomba;

	[Export] private Label bonusDebloque;

	private GameState gameState;

	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");

		InitialiserCoffres();
		
		// ------- Réception Signal Objectif Toutes Pièces Rammassées Atteint -------
		
		gameState.Connect(
			GameState.SignalName.ObjectifPiecesAtteint,
			new Callable(this, nameof(_on_objectif_pieces_atteint))
		);

		// ------- Réception Signal Coffre choisi (voir méthode _on_coffre_ouvert) -------

		coffreRessort.Connect(
			Coffre.SignalName.CoffreOuvert,
			new Callable(this, nameof(_on_coffre_ouvert))
		);

		coffreBomba.Connect(
			Coffre.SignalName.CoffreOuvert,
			new Callable(this, nameof(_on_coffre_ouvert))
		);
	}

	// ---------------- INIT ----------------

	private void InitialiserCoffres()
	{
		coffreRessort.Visible = false;
		coffreBomba.Visible = false;

		coffreRessort.ProcessMode = Node.ProcessModeEnum.Disabled;
		coffreBomba.ProcessMode = Node.ProcessModeEnum.Disabled;
	}

	// ---------------- Apparition des Coffres Bonus ----------------

	private void _on_objectif_pieces_atteint()
	{
		GD.Print("CoffresBonus dit ok j'affiche les coffres");
		coffreRessort.ProcessMode = Node.ProcessModeEnum.Inherit;
		coffreBomba.ProcessMode = Node.ProcessModeEnum.Inherit;

		coffreRessort.Visible = true;
		coffreBomba.Visible = true;

		bonusDebloque.Visible = true;
		GetTree().CreateTimer(3.0f).Timeout += () =>
		{
			bonusDebloque.Visible = false;
		};
		
	}

	// ---------------- Choix du Coffre ----------------

	private void _on_coffre_ouvert(Coffre coffreOuvert)
	{
		if (!IsInstanceValid(coffreOuvert))
			return;

		if (coffreOuvert == coffreRessort)
		{
			gameState.aLeRessort = true;
			SupprimerCoffre(coffreBomba);
		}
		else if (coffreOuvert == coffreBomba)
		{
			gameState.aLaBomba = true;
			SupprimerCoffre(coffreRessort);
		}
	}


	// ---------------- Suppression Autre Coffre ----------------

	private void SupprimerCoffre(Coffre coffre)
	{
		if (!IsInstanceValid(coffre))
			return;

		coffre.QueueFree();
	}
}
