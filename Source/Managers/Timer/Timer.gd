extends CanvasLayer

@export var start_seconds: int = 120               # 2 minutos (ajústalo si quieres)
@export var enable_clock_animation: bool = false    # Desactivar para ahorrar RAM
@onready var label: Label = $Box/TiempoLabel
@onready var tick: Timer = $Timer                  # Timer: wait_time=1.0, one_shot=false

# --- Sprite del reloj ---
@onready var icon: AnimatedSprite2D = $AnimatedSprite2D  # Referencia directa al AnimatedSprite2D
@export var animation_name: StringName = "default" # Nombre de la anim en el AnimatedSprite2D
var frames: int = 12                               # 12 frames del reloj

# --- Estado ---
var remaining: int
var current_frame: int = 0

func _ready() -> void:
	add_to_group("TimerHUD")  # para poder encontrarlo fácil desde otros scripts (p.ej. recolectables)

	# Sprite - ya no necesitamos verificar el path porque es una referencia directa
	if icon and enable_clock_animation:
		icon.stop()  # Stop any auto-playing animation
		icon.animation = animation_name
		if icon.sprite_frames and animation_name != StringName():
			frames = icon.sprite_frames.get_frame_count(animation_name)
		icon.frame = 0
	elif icon:
		icon.visible = false  # Ocultar el sprite si está desactivado

	# Timer - usar process_mode para reducir overhead
	tick.process_callback = Timer.TIMER_PROCESS_IDLE
	tick.timeout.connect(_on_tick_timeout)

	reset_timer()
	tick.start()

func reset_timer() -> void:
	remaining = start_seconds
	current_frame = 0
	if icon and enable_clock_animation:
		icon.frame = 0
	_update_label()

func _on_tick_timeout() -> void:
	if remaining <= 0:
		return

	remaining -= 1
	_update_label()

	# Calcular frame solo cuando cambia (no cada tick) y si está habilitado
	if icon and enable_clock_animation and frames > 0:
		var new_frame: int = _calculate_current_frame()
		if new_frame != current_frame:
			current_frame = new_frame
			icon.frame = current_frame

	if remaining == 0:
		_time_up()

# Cálculo directo del frame basado en tiempo restante (más eficiente)
func _calculate_current_frame() -> int:
	if start_seconds <= 0:
		return 0
	# Invertir: frame 0 al inicio, último frame cuando queda poco tiempo
	var progress: float = 1.0 - (float(remaining) / float(start_seconds))
	var frame_idx: int = int(progress * float(frames))
	return clamp(frame_idx, 0, frames - 1)

func _update_label() -> void:
	var m: int = remaining / 60
	var s: int = remaining % 60
	label.text = "%02d:%02d" % [m, s]

func _time_up() -> void:
	tick.stop()
	# Lleva el sprite al último frame (opcional)
	if icon and enable_clock_animation:
		icon.frame = frames - 1
	print("¡Tiempo agotado!")
	# Aquí tu lógica de fin de nivel/pantalla/etc.

# --- API pública: llamar desde tus assets/recolectables ---
func add_time(sec: int) -> void:
	remaining = max(remaining + sec, 0)
	_update_label()

	# Si el tiempo estaba en 0 y reanudaste, vuelve a arrancar el timer
	if remaining > 0 and tick.is_stopped():
		tick.start()
