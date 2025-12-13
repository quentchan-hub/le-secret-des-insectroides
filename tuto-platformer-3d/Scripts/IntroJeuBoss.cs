using Godot;
using System;

public partial class IntroJeuBoss : Node3D
{
	[Export] AnimationPlayer animIntro;
	[Export] AnimationPlayer animBoss;
	[Export] ColorRect colorRect;
	public SoundManager soundManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		soundManager = GetNode<SoundManager>("/root/World1/SoundManager");
		soundManager.EcouterExclusivement(soundManager.introJeuThemeMusic);
		animIntro.Play("CheminBoss");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		//GetTree().Paused = true;
		colorRect.Visible = false;
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if (Input.IsActionJustPressed("skip_intro"))
		//{
			//_on_dialogue_ui_dialogue_terminey();
		//}
	}
		
	
	public async void _on_dialogue_ui_dialogue_terminey()
	{
		//GD.Print("play cheminBoss 2");
		animIntro.Play("CheminBoss2");
		await ToSignal(GetTree().CreateTimer(4.0f), "timeout");
		colorRect.Visible = true;
		float alpha = 0f; // transparence
		colorRect.SelfModulate = new Color (0, 0, 0, alpha);
		alpha = Mathf.Lerp(0.0f, 1.0f, 2.0f);
		
		DisablingProcess();
		
	}
	
	public void DisablingProcess()
	{
		
		//// Methode 1 propre efficace. Eteint et désactive la scène.
		
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.Disabled;
		
		//GetTree().Paused = false;
		soundManager.EcouterExclusivement(soundManager.mainThemeMusic);
		
		//// Methode 2 bourrin ! en cas de conflit entre UIs
		//this.QueueFree();  
		//GD.Print("IntroJeuBoss supprimé");
	}
	
	public void DecolVol()
	{
		animBoss.Play("Décollage&Vol");
	}
	
	public void Vol()
	{
		animBoss.Play("Vol");
	}
	
		public void StopVol()
	{
		animBoss.Stop();
	}
	
	public void VolAtter()
	{
		animBoss.Play("Vol&Atterrissage");
	}
	
	public void Atter()
	{
		animBoss.Play("Atterrissage");
	}

}
