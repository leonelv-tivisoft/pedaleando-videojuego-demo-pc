# res://OutLineAssets.gd (llanta)
extends StaticBody3D

# === TUS REFERENCIAS EXISTENTES ===
@onready var llanta_vieja_005: StaticBody3D = self
@onready var OutLineMesh: MeshInstance3D = $old_tyre_005/MeshInstance3D

var selected: bool = false
var OutLineWidth: float = 0.05

# === NUEVO: rótulo "Pulsa E" ===
@onready var prompt_label: Label3D = $PromptLabel
@export var prompt_offset_y: float = 1.0              # cuánto sube el rótulo
@export var prompt_min_show_dist: float = 0.5         # no mostrar si el jugador está “encima”
@export var prompt_max_show_dist: float = 6.0         # no mostrar si está muy lejos

# Opcional: sumar al contador al recoger (si tienes GameManager)
@export var pickup_amount: int = 1
@export var pickup_sfx: AudioStreamPlayer3D

func _ready() -> void:
	add_to_group("Interactable")  # para que el Player te detecte con RayCast
	if OutLineMesh:
		OutLineMesh.visible = false
	if prompt_label:
		prompt_label.visible = false

	# (opcional) si conectabas la selección por señal del player:
	var p := get_tree().get_first_node_in_group("Player")
	if p and p.has_signal("interact_object"):
		p.interact_object.connect(_set_select)

func _process(_delta: float) -> void:
	# Outline según selección (tu lógica)
	if OutLineMesh:
		OutLineMesh.visible = selected

	# Desplaza la pila en Y al seleccionar (tu lógica)
	if llanta_vieja_005:
		llanta_vieja_005.position.y = (OutLineWidth if selected else 0.0)

	# Actualiza rótulo
	_update_prompt()

func _update_prompt() -> void:
	if not prompt_label:
		return

	# Solo mostrar el texto cuando está seleccionado y a distancia razonable
	var cam := get_viewport().get_camera_3d()
	if not cam:
		prompt_label.visible = false
		return

	var dist := global_position.distance_to(cam.global_position)
	var show := selected and dist >= prompt_min_show_dist and dist <= prompt_max_show_dist
	prompt_label.visible = show

	if show:
		# Reposiciona un poquito por encima del objeto
		var base_pos := global_transform.origin
		prompt_label.global_transform.origin = base_pos + Vector3(0.0, prompt_offset_y, 0.0)
		# Si NO usas billboard, descomenta para que mire a cámara:
		# prompt_label.look_at(cam.global_position, Vector3.UP, true)

# Llamado desde tu Player (por señal o raycast)
func _set_select(object) -> void:
	selected = (self == object)

# Interacción (E) – el Player llama a esto si estás bajo el retículo
func interact(_player: Node) -> void:
	if pickup_sfx:
		pickup_sfx.play()

	var gm := get_tree().get_first_node_in_group("GameManager")
	if gm and gm.has_method("add_trash"):
		gm.add_trash(pickup_amount)

	queue_free()
