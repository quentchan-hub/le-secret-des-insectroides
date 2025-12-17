using Godot;
using System;

public partial class Mob1 : CharacterBody3D
{
	[Export] int Life = 2;
	[Export] public Node3D[] points; // création d'un tableau dans l'inspecteur 
	// permet de référencer les points de passage des tours de garde 
	// de chaque mob au cas où on en crée plusieurs
	int actualPoint = 0; // 1er point du tableau précédent cad premier 
	// "point de passage" du tour de garde du mob 
	[Export] float speed = 0.5f; // vitesse du personnage
	[Export] AnimationPlayer animPlayer; //animation du perso
	bool canWait = true; //var qui permet au mob de faire des pauses dans son
	// tour de garde
	private bool _damaging = false;
	
	[Export] MeshInstance3D beettleMesh;
	public StandardMaterial3D hitFlashMat;
	
	private SoundManager soundManager;

	
	public override void _Ready()
	{
		animPlayer.Play("walk");
		
		hitFlashMat = new StandardMaterial3D();
		hitFlashMat.AlbedoColor = new Color(1, 1, 1, (float)0.5f);
		
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
	}
	public override void _PhysicsProcess(double delta)
	{
		//la var dir ci-dessous correspond à chaque instant à la direction
		// que prend le mob. elle est représentée par un vecteur (Vector3)
		// calculer en faisant la différence entre la position actuelle
		// appelée "Position" et la position du prochain point de passage
		// appelée "points[actualPoint].Position". 
		// ainsi on peut traduire "actualPoint" par "cibleActuelle".
		
		var dir = (points[actualPoint].Position - Position).Normalized();
		dir.Y = 0;
		
		//ci_dessous la distance restante vers le "actualPoint" est questionnée
		// en dessous de 0.5m le mob fait une petite pause 
		if (Position.DistanceTo(points[actualPoint].Position) >= 0.5f)
		{
			animPlayer.Play("walk");
			Velocity = dir * speed;
			MoveAndSlide(); 
		} else
		{
			if (canWait)
			{
				animPlayer.Play("idle");
				WaitBeforeContinue();
			}
				
		}
		//ci-dessous force le mob à regarder le prochain point actualPoint
		//GlobalTransform -> vecteur Vector3 correspondant à actualPoint
		// dans l'espace mondial (donc non relatif au parent)
		//Origin -> coordonnées du vecteur GlobalTransform -> x, y ,z
		//Vector3.up -> c'est juste l'axe de rotation donc up = axe y ici
		//LookAt(points[actualPoint].GlobalTransform.Origin, Vector3.Up);
		//RotateObjectLocal(Vector3.Up, 3.14f);
		
		////On peut remplacer les lignes ci-dessus par celles ci-dessous 
		////quand les mobs buguent et plantent la tête en haut ou en bas.
		Vector3 targetPos = points[actualPoint].GlobalTransform.Origin;
		targetPos.Y = GlobalTransform.Origin.Y; // Force la même hauteur Y
		LookAt(targetPos, Vector3.Up);
		RotateObjectLocal(Vector3.Up, 3.14f);
	}
	private async void MobDie()
	{
		
		//desactive les animations du personnage
		SetPhysicsProcess(false);
		//joue l'animation poweroff
		animPlayer.Play("poweroff");
		//attendre un moment 1s
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		QueueFree();
		
	}
	private async void WaitBeforeContinue()
	{
		canWait = false;
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		
		actualPoint++;
		if (actualPoint > points.Length - 1)
		{
			actualPoint = 0;
		}
		canWait = true;
	}
	private async void _on_area_3d_bottom_area_entered(Area3D area)
	{
		if (_damaging) return;
		
		if (area.GetParent().HasMethod("TakeDamages"))
		{
			area.GetParent().Call("TakeDamages");
			_damaging = true;
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
			_damaging = false;
		}
	}
	
	private void _on_area_3d_top_area_entered(Area3D area)
	{
		
		FlashHit();
		Life--;
		GD.Print("mob takes dmg!");
		if (Life <= 0)
		{
			soundManager.PlayDeathMob();
			MobDie();
			GD.Print("mob dead!");
			
			var worldScript1 = GetNode<GameState>("/root/GameState");
			worldScript1.destroyedMobs++;
			
		}
		else
		{
		soundManager.PlayMobDamaged();
		}
	}
	
	public async void FlashHit()
	{
		beettleMesh.MaterialOverride = hitFlashMat;
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		beettleMesh.MaterialOverride = null;
	}
	
}
