using Godot;
using System;

public partial class WeakPointVisual : Node3D
{
	// Vitesse de pulsation
	[Export] public float pulseSpeed = 3f;
	
	// Échelle min et max
	[Export] public float scaleMin = 0.8f;
	[Export] public float scaleMax = 1.2f;
	
	private float timer = 0f;
	private Vector3 baseScale;
	
	public override void _Ready()
	{
		// Sauvegarder l'échelle de base
		baseScale = Scale;
	}
	
	public override void _Process(double delta)
	{
		// Ne faire pulser que si visible
		if (!Visible)
			return;
		
		// Incrémenter le timer
		timer += (float)delta * pulseSpeed;
		
		// Calculer le facteur de pulsation (oscillation sinusoïdale)
		float pulseFactor = scaleMin + (scaleMax - scaleMin) * (Mathf.Sin(timer) * 0.5f + 0.5f);
		
		// Appliquer à l'échelle
		Scale = baseScale * pulseFactor;
	}
}
