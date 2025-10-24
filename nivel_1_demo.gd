extends Node3D

@export var intro_duration: float = 3.0

@onready var intro_camera: Camera3D = $IntroCamera
@onready var player_camera: Camera3D = $Player/Pivot/PlayerCamera
@onready var mesh_capsula: MeshInstance3D = $Player/MeshInstance3D
@onready var dialogue: DialogueBox = $"CanvasLayer2/DialogueBox"
@onready var player: CharacterBody3D = $Player

# ⬇️ Contenedor del HUD (logo + contador)
@onready var hud_counter: CanvasLayer = $"Contador"   # usa %Contador si lo marcas como Unique Name

var _intro_done := false
var _dialog_done := false

func _ready() -> void:
	intro_camera.current = true
	player_camera.current = false
	mesh_capsula.visible = true

	# 🔒 bloquear control jugador
	player.set_physics_process(false)
	player.set_process_input(false)
	player.set_process_unhandled_input(false)

	# ⬇️ ocultar HUD al inicio
	if hud_counter:
		hud_counter.visible = false

	# Intro + diálogo en paralelo
	start_intro()

	var lines: Array[Dictionary] = [
		{"name":"SALUDOS",  "text":"¡Bienvenido a la costa!"},
		{"name":"TUTORIAL", "text":"Usa WASD para moverte y ESPACIO para saltar."},
		{"name":"TUTORIAL", "text":"Necesitamos tu ayuda para recoger la basura en la playa y el mar..."}
	]
	dialogue.start_dialogue(lines, func():
		_dialog_done = true
		_try_start_game()
	)

func start_intro() -> void:
	var t := create_tween()
	t.tween_property(intro_camera, "global_position", player_camera.global_position, intro_duration)\
	 .set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	t.tween_property(intro_camera, "global_rotation", player_camera.global_rotation, intro_duration)\
	 .set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	t.finished.connect(func():
		_intro_done = true
		_try_start_game()
	, CONNECT_ONE_SHOT)

func _try_start_game() -> void:
	if not (_intro_done and _dialog_done):
		return

	# Cambio de cámara sin tirón
	intro_camera.global_transform = player_camera.global_transform
	player_camera.current = true
	intro_camera.current = false

	# Vista jugador
	mesh_capsula.visible = false
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

	# 🔓 habilitar control del jugador
	player.set_physics_process(true)
	player.set_process_input(true)
	player.set_process_unhandled_input(true)

	# 👁️ mostrar HUD (logo + contador)
	if hud_counter:
		hud_counter.visible = true
