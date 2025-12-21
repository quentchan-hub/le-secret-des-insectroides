using Godot;
using System;

public partial class WeakPointArea : Area3D
{
	// ============================================
	// VARIABLES
	// ============================================
	
	[Export] private BossCombat bossCombat; // Référence au script principal du boss
	[Export] public PackedScene player;
	
	private Vector3 ejectBounce; 			// joueur dégagé après chaque impact
	
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
		if (bossCombat == null)						// Vérifier que le boss existe
			return;
		
		if (body is not PlayerBotCtrl player)		// Vérifier que c'est bien le joueur
			return;
		
		if (!bossCombat.CanTakeDamage)				// Prise de dommage géré dans BossCombat
			return;
		
		if (player.Velocity.Y >= 0)					// Actif si joueur tombe sur Boss, pas sur le côté
			return;
		
		// Déterminer les dégâts et le stun selon l'état du boss
		int damage;
		bool shouldStun;
		
		
		bool isVulnerable = bossCombat.isVulnerableToStun;	// Vulnérabilité au stun
		
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
		
	}
}
