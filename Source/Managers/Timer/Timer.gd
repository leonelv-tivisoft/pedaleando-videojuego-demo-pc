extends CanvasLayer

@export var start_seconds: int = 120               # 2 minutos (ajústalo si quieres)
@onready var label: Label = $Box/TiempoLabel
@onready var tick: Timer = $Timer                  # Timer: wait_time=1.0, one_shot=false

# --- Sprite del reloj ---
@onready var icon: AnimatedSprite2D = $AnimatedSprite2D  # Referencia directa al AnimatedSprite2D
@export var animation_name: StringName = "default" # Nombre de la anim en el AnimatedSprite2D
var frames: int = 6                                # Por defecto 6, si la anim tiene otra cantidad, la detectamos

# --- Estado ---
var remaining: int
var current_frame: int = 0
var secs_from_last_frame: int = 0

func _ready() -> void:
	add_to_group("TimerHUD")  # para poder encontrarlo fácil desde otros scripts (p.ej. recolectables)

	# Sprite - ya no necesitamos verificar el path porque es una referencia directa
	if icon:
		icon.stop()  # Stop any auto-playing animation
		icon.animation = animation_name
		if icon.sprite_frames and animation_name != StringName():
			frames = max(icon.sprite_frames.get_frame_count(animation_name), 1)
		icon.frame = 0

	# Timer
	tick.timeout.connect(_on_tick_timeout)

	reset_timer()
	tick.start()

func reset_timer() -> void:
	remaining = start_seconds
	current_frame = 0
	secs_from_last_frame = 0
	if icon:
		icon.frame = 0
	_update_label()

func _on_tick_timeout() -> void:
	if remaining <= 0:
		return

	remaining -= 1
	secs_from_last_frame += 1
	_update_label()

	# Avance de frame dinámico: bloque = tiempo_actual / nº_frames
	if icon and frames > 0:
		var interval := _current_interval()
		if secs_from_last_frame >= interval and current_frame < frames - 1:
			current_frame += 1
			icon.frame = current_frame
			secs_from_last_frame = 0

	if remaining == 0:
		_time_up()

func _current_interval() -> int:
	# Evita 0 y ajusta a lo que quede de tiempo
	var segments: int = max(frames, 1)
	var interval: int = int(round(float(max(remaining, 1)) / float(segments)))
	return max(interval, 1)


func _update_label() -> void:
	var m: int = remaining / 60
	var s: int = remaining % 60
	label.text = "%02d:%02d" % [m, s]


func _time_up() -> void:
	tick.stop()
	# Lleva el sprite al último frame (opcional)
	if icon:
		icon.frame = frames - 1
	print("¡Tiempo agotado!")
	# Aquí tu lógica de fin de nivel/pantalla/etc.

# --- API pública: llamar desde tus assets/recolectables ---
func add_time(sec: int) -> void:
	remaining = max(remaining + sec, 0)
	_update_label()

	# Opcional: reiniciar el conteo del bloque para que el avance del frame
	# empiece limpio con el nuevo intervalo
	secs_from_last_frame = 0

	# Si el tiempo estaba en 0 y reanudaste, vuelve a arrancar el timer
	if remaining > 0 and tick.is_stopped():
		tick.start()
