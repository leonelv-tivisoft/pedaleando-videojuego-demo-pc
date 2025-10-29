# res://OutLineAssets.gd (PilaLlanta)
extends StaticBody3D

# === REFERENCIAS EXISTENTES (mantenidas) ===
@onready var llanta_vieja_005: StaticBody3D = self
@onready var OutLineMesh: MeshInstance3D = $"old_tyre_005/MeshInstance3D"

var selected: bool = false
var OutLineWidth: float = 0.05

# === Rótulo "Pulsa E" (mantenido) ===
@onready var prompt_label: Label3D = $PromptLabel
@export var prompt_offset_y: float = 1.5            # cuánto sube el rótulo
@export var prompt_min_show_dist: float = 0.5        # no mostrar si el jugador está “encima”
@export var prompt_max_show_dist: float = 6.0        # no mostrar si está muy lejos

# === Contador (nuevo, sin afectar visuales) ===
@export var pickup_amount: int = 1
@export var pickup_sfx: AudioStreamPlayer3D

func _ready() -> void:
	add_to_group("Interactable")  # para que el Player te detecte con RayCast

	# Estado visual inicial (igual que antes)
	if OutLineMesh:
		OutLineMesh.visible = false
	if prompt_label:
		prompt_label.visible = false

	# Conectar selección desde Player si usa señales
	var p := get_tree().get_first_node_in_group("Player")
	if p and p.has_signal("interact_object"):
		p.interact_object.connect(_set_select)

func _process(_delta: float) -> void:
	# Outline según selección
	if OutLineMesh:
		OutLineMesh.visible = selected

	# Elevación si está seleccionada
	if llanta_vieja_005:
		llanta_vieja_005.position.y = (OutLineWidth if selected else 0.0)

	# Actualizar texto “Pulsa E”
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
		var base_pos := global_transform.origin
		prompt_label.global_transform.origin = base_pos + Vector3(0.0, prompt_offset_y, 0.0)
		# Si no usas billboard, puedes hacer que mire a la cámara:
		# prompt_label.look_at(cam.global_position, Vector3.UP, true)

# Llamado desde Player
func _set_select(object) -> void:
	selected = (self == object)

# Interacción (E)
func interact(_player: Node) -> void:
	if pickup_sfx:
		pickup_sfx.play()

	# Aumentar contador
	if typeof(IGameManager) != TYPE_NIL and IGameManager.has_method("add_trash"):
		IGameManager.add_trash(pickup_amount)

	queue_free()
