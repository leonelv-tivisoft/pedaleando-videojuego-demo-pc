extends CharacterBody3D

# apagar oclusores
@export var disable_occluders_when_underwater := true

# ====== CONTROLES EXISTENTES ======
@export var sensibilidad: float = 0.003
@export var velocidad: float = 5.0
@export var gravedad: float = 9.8
@export var fuerza_salto: float = 5.0

signal interact_object

@onready var ray_cast_3d: RayCast3D = $Pivot/PlayerCamera/RayCast3D
@onready var camara: Camera3D = $"Pivot/PlayerCamera"
@onready var pivot: Node3D = $"Pivot"
@onready var exit_dialog: ConfirmationDialog = $"../ExitDialog"

# ====== AGUA / SUBMARINO ======
@export var water_area: Area3D                      # arrastra tu Area3D del agua
@export var water_surface: Node3D                   # arrastra el nodo de la superficie (MeshInstance3D del agua)
@export var surface_margin: float = 0.25            # margen para considerar "debajo de la superficie"

# (opcional) si tu material de agua tiene uniform is_underwater
@export var water_material: ShaderMaterial = null
@export var world_env: WorldEnvironment = null
@export var env_default: Environment = null
@export var env_underwater: Environment = null

# Parámetros de nado (ajústalos al gusto)
@export_group("Swim")
@export var swim_speed: float = 5.0
@export var swim_accel: float = 6.0
@export var swim_decel: float = 5.0
@export var swim_vertical_speed: float = 3.2
@export var buoyancy: float = 2.8            # empuje hacia arriba
@export var water_gravity: float = 0.6       # gravedad residual en agua
@export var water_drag: float = 2.0          # arrastre global
@export var max_vertical_speed: float = 4.0

var direccion := Vector3.ZERO
var rotation_x: float = 0.0

var in_water := false       # dentro del volumen de agua
var underwater := false     # por debajo de la superficie
var _water_counter := 0     # por si hay varios volúmenes solapados

func _ready() -> void:
	
	add_to_group("Player")
	
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	# Conecta señales del Area3D (o hazlo en el editor)
	if water_area:
		water_area.body_entered.connect(_on_water_body_entered)
		water_area.body_exited.connect(_on_water_body_exited)

func _input(event) -> void:
	
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		# Rotación horizontal (player)
		rotate_y(-event.relative.x * sensibilidad)
		# Rotación vertical (pivot)
		rotation_x -= event.relative.y * sensibilidad
		rotation_x = clamp(rotation_x, deg_to_rad(-80), deg_to_rad(80))
		pivot.rotation.x = rotation_x

	# ESC para dialogo
	if event.is_action_pressed("ui_cancel"):
		if exit_dialog.visible:
			exit_dialog.hide()
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
		else:
			exit_dialog.show()
			exit_dialog.grab_focus()
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			

func _process(delta): 
	if ray_cast_3d.is_colliding():
		var collider = ray_cast_3d.get_collider()
		interact_object.emit(collider)
	else: interact_object.emit(null)
	
		# --- NUEVO: Interactuar con objetos (E) ---
	if Input.is_action_just_pressed("interact") and ray_cast_3d.is_colliding():
		var collider = ray_cast_3d.get_collider()
		if collider and collider.has_method("interact"):
			collider.interact(self)

	

func _physics_process(delta: float) -> void:
	# Actualiza flag "underwater" por altura de la cámara respecto a la superficie
	_update_underwater_flag()

	if in_water:
		_swim_physics(delta)
	else:
		_walk_physics(delta)

# ========== CAMINAR ==========
func _walk_physics(delta: float) -> void:
	direccion = Vector3.ZERO
	if Input.is_action_pressed("Adelante"):
		direccion -= transform.basis.z
	if Input.is_action_pressed("Atras"):
		direccion += transform.basis.z
	if Input.is_action_pressed("Izquierda"):
		direccion -= transform.basis.x
	if Input.is_action_pressed("Derecha"):
		direccion += transform.basis.x
	direccion = direccion.normalized()

	# gravedad, salto
	if not is_on_floor():
		velocity.y -= gravedad * delta
	else:
		velocity.y = 0.0
		if Input.is_action_just_pressed("Saltar"):
			velocity.y = fuerza_salto

	# movimiento horizontal
	var horizontal_velocity = direccion * velocidad
	velocity.x = horizontal_velocity.x
	velocity.z = horizontal_velocity.z

	move_and_slide()

# ========== NADAR ==========
func _swim_physics(delta: float) -> void:
	# Dirección en plano XZ (como tu caminar)
	var input_vec := Vector2(
		(Input.get_action_strength("Derecha") - Input.get_action_strength("Izquierda")),
		(Input.get_action_strength("Atras") - Input.get_action_strength("Adelante"))
	)
	var dir := Vector3.ZERO
	if input_vec.length() > 0.0:
		dir = (transform.basis.x * input_vec.x + transform.basis.z * input_vec.y).normalized()

	# velocidad horizontal suave
	var target_h := dir * swim_speed
	var a := swim_accel if dir != Vector3.ZERO else swim_decel
	velocity.x = move_toward(velocity.x, target_h.x, a * delta)
	velocity.z = move_toward(velocity.z, target_h.z, a * delta)

	# control vertical: Space sube, Ctrl/Shift baja (usa tu acción "Saltar" para subir)
	var vinput := int(Input.is_action_pressed("Saltar")) \
			- int(Input.is_action_pressed("Crouch") or Input.is_action_pressed("ui_down"))
	var target_y := vinput * swim_vertical_speed
	velocity.y = move_toward(velocity.y, target_y, swim_accel * delta)


	# flotación + gravedad residual
	velocity.y += (buoyancy - water_gravity) * delta

	# arrastre global (frena todo)
	velocity -= velocity * clamp(water_drag * delta, 0.0, 0.95)

	# límite vertical
	velocity.y = clamp(velocity.y, -max_vertical_speed, max_vertical_speed)

	move_and_slide()

# ========== AGUA: ENTRAR / SALIR ==========
func _on_water_body_entered(body: Node):
	if body == self:
		_water_counter += 1
		in_water = _water_counter > 0
		_apply_underwater_fx(true)

func _on_water_body_exited(body: Node):
	if body == self:
		_water_counter = max(_water_counter - 1, 0)
		in_water = _water_counter > 0
		if not in_water:
			_apply_underwater_fx(false)

func _update_underwater_flag() -> void:
	if water_surface and camara:
		var under := camara.global_transform.origin.y < water_surface.global_transform.origin.y - surface_margin
		if under != underwater:
			underwater = under
			_apply_underwater_fx(under)

func _apply_underwater_fx(under: bool) -> void:
	if water_material:
		water_material.set_shader_parameter("is_underwater", under)
	if world_env and env_default and env_underwater:
		world_env.environment = env_underwater if under else env_default
	if disable_occluders_when_underwater:
		_toggle_occluders(not under)

func _toggle_occluders(enabled: bool) -> void:
	for n in get_tree().get_nodes_in_group("occluders"):
		if n is OccluderInstance3D:
			n.visible = enabled
