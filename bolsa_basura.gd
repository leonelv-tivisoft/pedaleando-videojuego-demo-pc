# res://OutLineAssets.gd (bolsa_basura)
# [DEPRECATED] This file is deprecated and will be removed. Use Source/Gameplay/Interactables/TrashBag.cs instead.
# TODO: Remove this file after full migration to C#
extends StaticBody3D

# === TUS REFERENCIAS EXISTENTES (mantenidas) ===
@onready var BolsaBasura: StaticBody3D = self
@onready var OutLineMesh: MeshInstance3D = $"Trashbag - Full_001/MeshInstance3D"

var selected: bool = false
var OutLineWidth: float = 0.05

# === Rótulo "Pulsa E" (mantenido) ===
@onready var prompt_label: Label3D = $PromptLabel
@export var prompt_offset_y: float = 2.3            # cuánto sube el rótulo
@export var prompt_min_show_dist: float = 0.5       # no mostrar si el jugador está “encima”
@export var prompt_max_show_dist: float = 6.0       # no mostrar si está muy lejos

# === Contador (nuevo, sin tocar lo visual) ===
@export var pickup_amount: int = 1
@export var pickup_sfx: AudioStreamPlayer3D

func _ready() -> void:
	# Para que el Player (por RayCast o señal) te identifique como interactuable
	add_to_group("Interactable")

	# Estado visual inicial (mantenido)
	if OutLineMesh:
		OutLineMesh.visible = false
	if prompt_label:
		prompt_label.visible = false

	# Si ya tenías conexión por señal desde Player:
	var p := get_tree().get_first_node_in_group("Player")
	if p and p.has_signal("interact_object"):
		p.interact_object.connect(_set_select)

func _process(_delta: float) -> void:
	# Outline según selección (mantenido)
	if OutLineMesh:
		OutLineMesh.visible = selected

	# “Levitar” en Y si está seleccionado (mantenido)
	if BolsaBasura:
		BolsaBasura.position.y = (OutLineWidth if selected else 0.0)

	# Actualizar rótulo “Pulsa E” (mantenido)
	_update_prompt()

func _update_prompt() -> void:
	if not prompt_label:
		return

	var cam := get_viewport().get_camera_3d()
	if not cam:
		prompt_label.visible = false
		return

	var dist := global_position.distance_to(cam.global_position)
	var show := selected and dist >= prompt_min_show_dist and dist <= prompt_max_show_dist
	prompt_label.visible = show

	if show:
		# Reposicionar el rótulo un poco por encima
		var base_pos := global_transform.origin
		prompt_label.global_transform.origin = base_pos + Vector3(0.0, prompt_offset_y, 0.0)
		# Si no usas billboard, puedes hacer que mire a cámara:
		# prompt_label.look_at(cam.global_position, Vector3.UP, true)

# Llamado desde tu Player (por señal o por RayCast)
func _set_select(object) -> void:
	selected = (self == object)

# Interacción (E) – el Player te llama si estás bajo el retículo
func interact(_player: Node) -> void:
	if pickup_sfx:
		pickup_sfx.play()

	# Sumar al contador si el autoload GameManager existe
	if Engine.is_editor_hint():
		# en editor no hacemos nada
		pass
	else:
		if typeof(GameManager) != TYPE_NIL and GameManager.has_method("add_trash"):
			GameManager.add_trash(pickup_amount)

	# Eliminar la bolsa tras recoger
	queue_free()
