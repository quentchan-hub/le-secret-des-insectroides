@tool
extends SubViewport

@export_tool_button("Générateur de .png") var sprite_generator = func generer_png():
	var model_name = get_child(1).get_child(0).name
	get_texture().get_image().save_png("res://Mesh/Sprite2D/" + model_name + ".png")
	
