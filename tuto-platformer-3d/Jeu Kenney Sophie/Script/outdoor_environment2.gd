extends Node

@export var quit_popup: Control


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):  # Échap
		if quit_popup.visible:
			quit_popup.cache_popup()
		else:
			quit_popup.montre_popup()
