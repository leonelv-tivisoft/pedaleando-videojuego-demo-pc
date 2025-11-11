extends Camera3D

@export var player_path: NodePath
@export var bounds_area: NodePath            # Asigna aquí el Area3D o StaticBody3D del mapa
@export var follow_smooth: float = 0.15
@export var lock_rotation: bool = true

@onready var player: Node3D = get_node_or_null(player_path)
@onready var bounds_node: Node3D = get_node_or_null(bounds_area)

var _locked_y: float

func _ready() -> void:
	if player == null:
		push_warning("MinimapCamera: 'player_path' no asignado.")
	if bounds_node == null:
		push_warning("MinimapCamera: 'bounds_area' no asignado.")
	_locked_y = global_position.y

	if lock_rotation:
		rotation_degrees = Vector3(-90.0, 0.0, 0.0)


func _process(delta: float) -> void:
	if player == null:
		return

	var target_x: float = player.global_position.x
	var target_z: float = player.global_position.z

	# Si hay límites del mapa, aplicamos clamp
	if bounds_node:
		var shape: CollisionShape3D = bounds_node.get_node_or_null("CollisionShape3D")
		if shape and shape.shape is BoxShape3D:
			var box: BoxShape3D = shape.shape as BoxShape3D
			var extents: Vector3 = box.size * 0.5
			var center: Vector3 = shape.global_transform.origin

			var min_x: float = center.x - extents.x
			var max_x: float = center.x + extents.x
			var min_z: float = center.z - extents.z
			var max_z: float = center.z + extents.z

			target_x = clampf(target_x, min_x, max_x)
			target_z = clampf(target_z, min_z, max_z)

	# Movemos la cámara
	var target: Vector3 = Vector3(target_x, _locked_y, target_z)
	if follow_smooth > 0.0:
		var t: float = clampf(follow_smooth * delta * 60.0, 0.0, 1.0)
		global_position = global_position.lerp(target, t)
	else:
		global_position = target
