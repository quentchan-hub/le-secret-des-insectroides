using Godot;
using System;

public partial class BoutonNiveauSuivant: Button
{
	private void _on_button_down()
	{
		ChangeScene("res://Scenes/Niveau2/niveau_2.tscn"); // Appel de fonction
	}

	private void ChangeScene(string scenePath) // Fonction séparée
	{
		// Effet de fondu (optionnel)
		var fadeOut = GetTree().CreateTween();
		var colorRect = new ColorRect();
		colorRect.Color = new Color(0, 0, 0, 0);
		colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		GetTree().Root.AddChild(colorRect);
		fadeOut.TweenProperty(colorRect, "modulate:a", 1.0f, 0.5f);
		fadeOut.TweenCallback(Callable.From(() => 
		{
			GetTree().ChangeSceneToFile(scenePath);
		}));
	}
}
