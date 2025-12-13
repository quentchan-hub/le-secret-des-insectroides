extends Node3D  # Attaché à CamPivot

@export var target: CharacterBody3D  # Référence au nœud Player
@export var mouse_sensitivity: float = 0.1
@export var min_pitch: float = -80.0  # Limite de rotation vers le bas
@export var max_pitch: float = 80.0   # Limite de rotation vers le haut
@export var smooth_speed: float = 5.0  # Vitesse de lissage pour le suivi

@onready var spring_arm: SpringArm3D = $SpringArm3D
@onready var camera: Camera3D = $SpringArm3D/Camera3D

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		# Rotation horizontale (gauche/droite) du CamPivot
		var rot_deg: Vector3 = rotation_degrees
		rot_deg.y -= event.relative.x * mouse_sensitivity
		rotation_degrees = rot_deg

		# Rotation verticale (haut/bas) du CamPivot
		rot_deg = rotation_degrees
		rot_deg.x += event.relative.y * mouse_sensitivity
		rot_deg.x = clamp(rot_deg.x, min_pitch, max_pitch)
		rotation_degrees = rot_deg

func _physics_process(_delta: float) -> void:
	if target:
		# Positionne le CamPivot à la position du personnage
		global_position = target.global_position

		# Le SpringArm3D gère automatiquement la distance et les collisions
		# Pas besoin de calculer manuellement la position de la caméra
