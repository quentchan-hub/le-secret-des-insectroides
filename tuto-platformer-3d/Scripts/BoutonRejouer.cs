using Godot;
using System;

public partial class BoutonRejouer : Button
{
	private void _on_button_button_down()
	{
		GetTree().ReloadCurrentScene();
	}
}
