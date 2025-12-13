using Godot;
using System;

public partial class WeakPointArea : Area3D
{
	// ============================================
	// VARIABLES
	// ============================================
	
	[Export] private BossCombat bossCombat; // Référence au script principal du boss
	[Export] public PackedScene player;
	
	// ============================================
	// INITIALISATION
	// ============================================
	
	public override void _Ready()
	{
		
		if (bossCombat == null)
		{
			GD.PrintErr("BossWeakPoint: Impossible de trouver le script BossCombat sur le parent!");
			return;
		}
		
		// Connecter le signal quand un corps entre dans la zone
		BodyEntered += OnBodyEntered;
	}
	
	// ============================================
	// DÉTECTION DU JOUEUR
	// ============================================
	
	private void OnBodyEntered(Node3D body)
	{
		// Vérifier que le boss existe
		if (bossCombat == null)
			return;
		
		// Vérifier que c'est bien le joueur
		if (body is not PlayerBotCtrl player)
			return;
		
		// Vérifier que le joueur tombe (vélocité Y négative)
		// Cela signifie qu'il saute SUR le boss, pas qu'il le touche par le côté
		if (player.Velocity.Y >= 0)
			return;
		
		// Déterminer les dégâts et le stun selon l'état du boss
		int damage;
		bool shouldStun;
		
		// Vérifier si le boss est vulnérable au stun
		bool isVulnerable = bossCombat.isVulnerableToStun;
		
		if (isVulnerable)
		{
			// Boss en l'air ET vulnérable (Décollage&Vol, MitraillageEnVol, Coup Special)
			damage = 2;
			shouldStun = true;
			GD.Print("Point faible touché EN L'AIR ! 2 dégâts + STUN");
		}
		else
		{
			// Boss au sol OU en descente (Vol&Atterrissage)
			damage = 1;
			shouldStun = false;
			GD.Print("Point faible touché au sol. 1 dégât, pas de stun.");
		}
		
		// Envoyer les dégâts au boss
		bossCombat.BossTakeDamage(damage, shouldStun);
		
		//on réutilise tout simplement la variable bounce du player
		player.bounce = true;
		
		
		//// Faire rebondir le joueur (comme sur les ennemis normaux)
		//// On utilise la même mécanique que dans le script du joueur
		//var bounceField = player.GetType().GetField("bounce", 
			//System.Reflection.BindingFlags.NonPublic | 
			//System.Reflection.BindingFlags.Instance);
		//
		//if (bounceField != null)
			//bounceField.SetValue(player, true);
	}
}
