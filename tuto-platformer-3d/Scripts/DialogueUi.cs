using Godot;
using System;

public partial class DialogueUi : CanvasLayer
{
	[Signal] public delegate void DialogueTermineyEventHandler(); // Création d'un signal
	[Export] private Label speakerName; 
	[Export] private Label dialogue;
	[Export] private Panel panel;
	[Export] public string speakerNameText = "Test boutchou";
	[Export] public string[] dialogueContent = new string[]
	{
		"Première Ligne de dialogue",
		"Deuxième Ligne",
		"3ème ligne"
	};
	
	private int index = 0;
	
	public override void _Ready()
	{
		speakerName.Text = speakerNameText;
		dialogue.Text = dialogueContent[index];
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept") 
		|| @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			index++;
			if (index < dialogueContent.Length)
			{
				dialogue.Text = dialogueContent[index];
			}
			else
			{
				//index = 0;
				//dialogue.Text = dialogueContent[index];
				EmitSignal(SignalName.DialogueTerminey);
				panel.Visible = false;
			}
				
		}
	}
	
	

}

	
