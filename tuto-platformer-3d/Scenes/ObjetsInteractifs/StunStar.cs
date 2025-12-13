using Godot;
using System;

public partial class StunStar : Sprite3D
{
	// ============================================
	// VARIABLES
	// ============================================
	
	// Vitesse de rotation : 180° par seconde
	// En 3 secondes, l'étoile fera 1.5 tour complet (540°)
	private float rotationSpeed = 180f; // Degrés par seconde
	
	// Timer pour faire tourner l'ensemble des étoiles autour du centre
	private float orbitTimer = 0f;
	
	// ============================================
	// INITIALISATION
	// ============================================
	
	public override void _Ready()
	{
		// S'assurer que le Sprite3D est bien configuré
		Billboard = BaseMaterial3D.BillboardModeEnum.Enabled; // Toujours face caméra
		Modulate = new Color(1f, 1f, 0f); // Jaune doré
		
		// Optionnel : ajouter une petite animation de spawn
		Scale = Vector3.Zero;
		
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "scale", Vector3.One, 0.2f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
	}
	
	// ============================================
	// BOUCLE PRINCIPALE
	// ============================================
	
	public override void _Process(double delta)
	{
		// Faire tourner l'étoile sur elle-même
		// On tourne autour de l'axe Y (vertical)
		Vector3 rotation = RotationDegrees;
		rotation.Y += rotationSpeed * (float)delta;
		RotationDegrees = rotation;
		
		// Faire tourner toutes les étoiles autour du centre (orbit)
		// Le parent (stunStarsSpawnPoint) gère déjà le positionnement en cercle
		// Ici on fait juste tourner le groupe entier
		if (GetParent() != null)
		{
			orbitTimer += (float)delta * 90f; // 90° par seconde pour l'orbite
			
			// Appliquer la rotation au parent (toutes les étoiles tournent ensemble)
			Node3D parent = GetParent<Node3D>();
			if (parent != null)
			{
				Vector3 parentRot = parent.RotationDegrees;
				parentRot.Y = orbitTimer;
				parent.RotationDegrees = parentRot;
			}
		}
	}
}
