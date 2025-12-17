using Godot;
using System;

public partial class BossHealthBar : ProgressBar
{
	
	private int minHealth = 0;
	private int maxHealth = 16;
	private int currentHealth = 16;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MinValue = minHealth;
		MaxValue = maxHealth;
		Value = currentHealth;
	}
	
	public void UpdateBossLife(int current)
	{
		currentHealth = Mathf.Clamp(current, minHealth, maxHealth);
		Value = currentHealth;
	}
	
}
