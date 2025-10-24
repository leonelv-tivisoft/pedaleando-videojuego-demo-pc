# UnderwaterSwitch_OnMesh.gd  (Godot 4.x)
extends MeshInstance3D

@export var camera: Camera3D
@export_range(0.0, 5.0, 0.01) var surface_margin := 0.2
var mat: ShaderMaterial

func _ready() -> void:
	mat = _get_shader_material()
	if mat == null:
		push_error("UnderwaterSwitch: asigna un ShaderMaterial al agua (override u override por superficie).")

func _process(_dt: float) -> void:
	if mat == null or camera == null:
		return
	var under := camera.global_transform.origin.y < global_transform.origin.y - surface_margin
	mat.set_shader_parameter("is_underwater", under)

func _get_shader_material() -> ShaderMaterial:
	var m := material_override
	if m is ShaderMaterial: return m
	m = get_surface_override_material(0)
	if m is ShaderMaterial: return m
	if mesh:
		m = mesh.surface_get_material(0)
		if m is ShaderMaterial: return m
	return null
