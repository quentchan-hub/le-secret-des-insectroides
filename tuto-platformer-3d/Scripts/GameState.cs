using Godot;
using System;

public partial class GameState : Node
{
	/* 
		
		Autoload — GameState
		Chemin : "/root/GameState"
		Gestion globale de l’état du jeu (score, objectifs, inventaire).
		Communication 100 % via signaux (pattern Observer Godot).
		
			
			Préambule sur les Autoloads
		
		Ce script est un autoload et n'est relié à aucun Noeud dans l'arbre.
		Après l'avoir créer on lui donne une "place" dans le jeu en allant dans
		Projet -> Paramètres du projet -> Onglet "Généraux" -> Chargement automatique.
		
			----------------------------------------------------
			
			Focus sur les Signaux dans Godot
		
		Pour épouser le style Godot en terme de communication internodal
		on va utiliser les signaux pour conserver le maximum de stabilité 
		et afin de répartir les responsabilités de chacun des scripts. 
		
		La plupart du temps on pourra utiliser les signaux proposés
		dans l'éditeur, mais parfois il faudra faire ses signaux soi-même
		et pour les récupérer on peut à nouveau utiliser l'éditeur sauf : 
		
		- pour les noeuds chargés dynamiquement
		- pour les autoloads
		
		Dans ce cas là on utilisera la méthode suivante (exemple au _Ready): 
		
		NoeudEmetteur noeudEmetteur = GetNode<NoeudEmetteur>("/root/.../NoeudEmetteur")
		
		noeudEmetteur.Connect(NoeudEmetteur.SignalName.NomDuSignal,
			new Callable(this, nameof(_on_nom_du_signal)));

		puis 
		
		public void _on_nom_du_signal(NoeudDuSignal paramètreSignal)
		{
			(...)
		}
		
		-> Connect = relie un signal à une méthode
		-> Callable = une référence à une méthode, attachée à un objet
		-> new = Toujours car méthode qu'on a créé =/= methode intégrée godot
	
	*/
	
	

	// ===============================
	// ÉTAT GLOBAL
	// ===============================

	public bool facingDoor = false;
	public bool facingDoor2 = false;

	public bool aLaCle = false;
	public bool aLaCleRose = false;

	public bool aLaBomba = false;
	public bool aLeRessort = false;
	public bool aLeFouet = false;

	public int destroyedMobs = 0;
	public int nbCoins = 0;
	public int scoreLvl1 = 0;

	public int totalPiecesOr = 0;
	public bool objectifPiecesOrAtteint = false;
	
	public bool bossDead = false;

	// ===============================
	// SIGNAUX
	// ===============================

	[Signal] public delegate void ObjectifPiecesAtteintEventHandler();


	// ===============================
	// SCORE
	// ===============================

	public void PrintScore()
	{
		scoreLvl1 += destroyedMobs * 100;
		scoreLvl1 += nbCoins * 10;

		GD.Print("Score final : " + scoreLvl1);
	}

	// ===============================
	// RÉCEPTION DES SIGNAUX
	// ===============================

	// Reçoit le nombre total de pièces instanciées depuis Coins
	public void _on_nb_pieces(int total)  // <- méthode mentionnée dans la connection
	{
		totalPiecesOr = total;
		//nbCoins = Mathf.Clamp(nbCoins, 0, totalPiecesOr);
	}
	
	public override void _Process(double delta)
	{	
		// D'abord petite sécurité pour le Signal qui était émit 
		// au Hard Reset (Bouton "Redémarrer" de GameOver) car 
		//	dans HardReset() -> objectifPiecesOrAtteint = false;
		// et nbCoins = 0; totalPiecesOr = 0; donc toutes conditions
		// réunies pour émission du Signal or CoffresBonus est détruit
		// dans le même temps. Bref on sécurise.
		
		if (totalPiecesOr <= 0)
			return;
			
		// Puis émission du Signal vers CoffresBonus
		if (!objectifPiecesOrAtteint && nbCoins >= totalPiecesOr)
		{
			objectifPiecesOrAtteint = true;
			GD.Print("objectif atteint");
			EmitSignal(SignalName.ObjectifPiecesAtteint);
		}
	}
	
	// ===============================
	//  HARD RESET
	// ===============================
	
	public void HardReset()
	{
		facingDoor = false;
		facingDoor2 = false;

		aLaCle = false;
		aLaCleRose = false;

		aLaBomba = false;
		aLeRessort = false;
		aLeFouet = false;

		destroyedMobs = 0;
		nbCoins = 0;
		scoreLvl1 = 0;

		totalPiecesOr = 0;
		objectifPiecesOrAtteint = false;
	}

}
