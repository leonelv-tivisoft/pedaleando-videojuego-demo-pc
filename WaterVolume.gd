# WaterVolume.gd  (en el Area3D del agua)
extends Area3D

@export var water_surface: Node3D   # arrastra el MeshInstance3D de la superficie
signal swimmer_entered(character: CharacterBody3D)
signal swimmer_exited(character: CharacterBody3D)

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)

func _on_body_entered(body: Node) -> void:
	if body is CharacterBody3D:
		emit_signal("swimmer_entered", body)

func _on_body_exited(body: Node) -> void:
	if body is CharacterBody3D:
		emit_signal("swimmer_exited", body)
