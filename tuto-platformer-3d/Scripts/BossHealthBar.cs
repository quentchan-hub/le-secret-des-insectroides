using Godot;
using System;

public partial class BossHealthBar : ProgressBar
{
	[Export] public Color fullLifeColor = Colors.Green;
	[Export] public Color lowLifeColor = Colors.Red;
	
	private int minHealth = 0;
	private int maxHealth = 16;
	private int currentHealth = 16;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MinValue = minHealth;
		MaxValue = maxHealth;
		Value = currentHealth;
		RageModeColor();
	}
	
	public void UpdateBossLife(int current)
	{
		currentHealth = Mathf.Clamp(current, minHealth, maxHealth);
		Value = currentHealth;
		RageModeColor();
	}
	public void RageModeColor()
	{
		if (Value <= 8f)
		{
			Modulate = lowLifeColor;
			
		}
		else
		{
			Modulate = fullLifeColor;
		}
	}
}
