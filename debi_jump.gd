extends Node3D

@onready var anim: AnimationPlayer = $AnimationDebiJump

# ---- Parámetros editables en el Inspector ----
@export var play_after_seconds: float = 3.0      # cuánto esperar antes de reproducir la animación
@export var autohide_after_seconds: float = 0.0  # 0 = no ocultar automáticamente
@export var autostart: bool = true               # ¿hacerlo automáticamente en _ready()?

func _ready() -> void:
	if autostart:
		await get_tree().create_timer(play_after_seconds).timeout
		anim.play("Idle_Debi")
		if autohide_after_seconds > 0.0:
			await get_tree().create_timer(autohide_after_seconds).timeout
			visible = false

# ---- API: llamar cuando quieras desde otro script/nodo ----

# Reproduce la animación después de 'delay' segundos
func play_after(delay: float, animation_name: String = "Idle_Debi") -> void:
	_play_after_internal(delay, animation_name)

func _play_after_internal(delay: float, animation_name: String) -> void:
	await get_tree().create_timer(delay).timeout
	anim.play(animation_name)

# Oculta ESTE nodo después de 'delay' segundos
func hide_after(delay: float) -> void:
	await get_tree().create_timer(delay).timeout
	visible = false

# (opcional) volver a mostrar
func show_now() -> void:
	visible = true
