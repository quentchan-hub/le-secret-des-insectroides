using Godot;
using System;

public partial class Coins : Node3D
{
	/* 	Petite particularité de ce Script : 
	
		un Signal emit d'ICI vers un noeud recepteur (gameState) 
		
		=> EmitSignal(...)

								ET
	
		connecter au noeud recepteur depuis ICI 
		
		=> Connect(..., new Callable(gameState, ...)

	*/


	[Signal] public delegate void NbPiecesEventHandler(int total);

	private int totalPieces;

	public override void _Ready()
	{
		GameState gameState = GetNode<GameState>("/root/GameState");

		Connect(SignalName.NbPieces,									// Connexion au GameState
			new Callable(gameState, nameof(GameState._on_nb_pieces)));

		CompterPieces();
	}

	private void CompterPieces()
	{
		totalPieces = 0;

		foreach (Node Child in GetChildren())
		{
			if (Child is PieceOr)
				totalPieces++;
		}
		
		EmitSignal(SignalName.NbPieces, totalPieces);					// Envoi signal
	}
}
