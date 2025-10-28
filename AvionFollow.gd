# res://AvionFollow.gd
extends PathFollow3D

@export var speed: float = 60.0              # m/s a lo largo de la ruta
@export var loop_path: bool = true           # repetir al llegar al final
@export_range(0.0, 45.0) var bank_deg := 18  # inclinación (alabeo) máxima en giros
@export var wobble_amp: float = 1.5          # bamboleo suave (estética)
@export var wobble_freq: float = 0.35        # Hz del bamboleo

@onready var plane: Node3D = $Plane        # tu modelo de avión (hijo)
var _prev_forward := Vector3.FORWARD
var _t := 0.0

func _ready() -> void:
	rotation_mode = PathFollow3D.ROTATION_ORIENTED
	_prev_forward = -global_transform.basis.z
	# Si tu modelo "mira" hacia otro eje, ajusta una sola vez:
	# plane.rotate_y(PI)  # ejemplo si el avión mira -Z en vez de +Z

func _physics_process(delta: float) -> void:
	_t += delta

	# Avanzar sobre la curva
	var path := get_parent() as Path3D
	var length := path.curve.get_baked_length()
	progress += speed * delta
	if progress > length:
		if loop_path:
			progress = fmod(progress, length)
		else:
			progress = length  # te quedas al final (o haz queue_free())

# Calcular alabeo en función del giro (derivado de cambio de dirección)
	var fwd: Vector3 = -global_transform.basis.z
	var turn: float = _prev_forward.cross(fwd).y           # >0 giro izq, <0 giro der
	var target_bank: float = clampf(turn * bank_deg * 10.0, -bank_deg, bank_deg)

	# Pequeño bamboleo estético
	target_bank += sin(_t * TAU * wobble_freq) * wobble_amp

	# Aplicar alabeo SOLO al hijo visual para no romper la orientación del Follow
	if plane:
		plane.rotation_degrees.z = lerp(plane.rotation_degrees.z, -target_bank, 5.0 * delta)

	_prev_forward = fwd
