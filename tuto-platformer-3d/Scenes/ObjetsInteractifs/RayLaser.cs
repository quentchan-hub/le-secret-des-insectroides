using Godot;
using System;

public partial class RayLaser : RayCast3D
{
	[Export] GpuParticles3D impactSol;
	[Export] MeshInstance3D meshRay;
	public Vector3 hitPoint;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (IsColliding())
		{
			hitPoint = GetCollisionPoint();
			float distance = GlobalPosition.DistanceTo(hitPoint);
			
			//étendre le mesh laser en y, donc on prend scale initial puis on applique la valeur 
			// distance en scale /2 car le scale applique la distance ds 2 côtés du mesh 
			Vector3 scale = meshRay.Scale;
			scale.Y = distance / 2.0f;
			meshRay.Scale = scale;
			
			//ensuite on applique juste un décalage du mesh car il est centré sur l'origine
			Vector3 meshPos = meshRay.Position;
			meshPos.Y = distance / 2.0f;
			meshRay.Position = meshPos;
			if (!impactSol.Emitting)
			impactSol.GlobalPosition = hitPoint;
			impactSol.Emitting = true;
			
			
		}
		else
		{
			impactSol.Emitting = false;
		}
	}

	
}
