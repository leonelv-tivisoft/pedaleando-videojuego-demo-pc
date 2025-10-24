extends CharacterBody3D

@export var sensibilidad: float = 0.003
@export var velocidad: float = 5.0

@onready var camara: Camera3D = $Pivot/ PlayerCamera
@onready var pivot: Node3D = $"Pivot"

var direccion = Vector3.ZERO
var rotation_x: float = 0.0 # acumulador para limitar la rotación vertical

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _input(event) -> void:
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		# Rotación horizontal (izq-der) -> el Player
		rotate_y(-event.relative.x * sensibilidad)

		# Rotación vertical (arriba-abajo) -> el Pivot
		rotation_x -= event.relative.y * sensibilidad
		rotation_x = clamp(rotation_x, deg_to_rad(-80), deg_to_rad(80))
		pivot.rotation.x = rotation_x


	# Detectar Escape para liberar/bloquear el mouse
	if event.is_action_pressed("ui_cancel"):
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _physics_process(delta: float) -> void:
	var direccion := Vector3.ZERO

	if Input.is_action_pressed("Adelante"):
		direccion -= transform.basis.z
	if Input.is_action_pressed("Atras"):
		direccion += transform.basis.z
	if Input.is_action_pressed("Izquierda"):
		direccion -= transform.basis.x
	if Input.is_action_pressed("Derecha"):
		direccion += transform.basis.x

	direccion = direccion.normalized()

	velocity = direccion * velocidad
	move_and_slide()
