using Godot;

public partial class QuitMenu : Control
{
	[Export] public Button boutonValider;
	[Export] public Button boutonAnnuler;
	
	public override void _Ready()
	{
		// permet de fonctionner même dans le mode pause
		ProcessMode = ProcessModeEnum.WhenPaused;
		
		// Cache le pop-up au démarrage
		Visible = false;
	}
	
	public void MontrePopup()
	{
		// Affiche le pop-up et libère la souris
		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
		//boutonValider.GrabFocus();  // Permet de capturer les entrées clavier pour le bouton "Oui"
	}
	
	public void CachePopup()
	{
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
		GetViewport().SetInputAsHandled();  // Force la recapture des entrées
	}
	
	//public override void _Input(InputEvent @event)
	//{
		//if (Visible)
		//{
			//if (@event.IsActionPressed("ui_accept"))
			//{
				//_on_bouton_valider_pressed();
			//}
			//else if (@event.IsActionPressed("ui_cancel"))
			//{
				//_on_bouton_annuler_pressed();
			//}
		//}
	//}
	
	private void _on_music_mute_button_toggled(bool mute)
	{
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), mute);
	}
	private void _on_bouton_valider_pressed()
	{
		GetTree().Quit();
	}
	
	private void _on_bouton_annuler_pressed()
	{
		CachePopup();
	}
}
