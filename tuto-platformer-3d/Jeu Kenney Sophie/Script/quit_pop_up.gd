extends Control

@onready var bouton_valider: Button = $PopUpWindow/ArrangementBoutons/BoutonValider
@onready var bouton_annuler: Button = $PopUpWindow/ArrangementBoutons/BoutonAnnuler


func _ready() -> void:
	# Cache le pop-up au démarrage
	visible = false
	
func montre_popup():
	# Affiche le pop-up et libère la souris
	visible = true
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	get_tree().paused = true
	#bouton_valider.grab_focus()  # Permet de capturer les entrées clavier pour le bouton "Oui"
	
	
func cache_popup():
	visible = false
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	get_tree().paused = false
	get_viewport().set_input_as_handled()  # Force la recapture des entrées
	
	
	
func _input(event: InputEvent) -> void:
	if visible:
		if event.is_action_pressed("ui_accept"):
			_on_yes_button_pressed()
		elif event.is_action_pressed("ui_cancel"):
			_on_no_button_pressed()

func _on_yes_button_pressed():
	get_tree().quit()
	
func _on_no_button_pressed():
	cache_popup()
