extends Control

@onready var jugar_button: Button = $JugarDemo
@onready var salir_button: Button = $Salir
@onready var hover_sound: AudioStreamPlayer = $AudioStreamPlayer

var tween: Tween

func _ready():
	# Conectar señales para ambos botones
	jugar_button.mouse_entered.connect(_on_button_mouse_entered.bind(jugar_button))
	jugar_button.mouse_exited.connect(_on_button_mouse_exited.bind(jugar_button))
	jugar_button.pressed.connect(_on_jugar_pressed)

	salir_button.mouse_entered.connect(_on_button_mouse_entered.bind(salir_button))
	salir_button.mouse_exited.connect(_on_button_mouse_exited.bind(salir_button))
	salir_button.pressed.connect(_on_salir_pressed)

func _on_button_mouse_entered(button: Button):
	if hover_sound.stream:
		hover_sound.play()
	if tween: tween.kill()
	tween = get_tree().create_tween()
	tween.tween_property(button, "scale", Vector2(1.1, 1.1), 0.2)

func _on_button_mouse_exited(button: Button):
	if tween: tween.kill()
	tween = get_tree().create_tween()
	tween.tween_property(button, "scale", Vector2.ONE, 0.2)

func _on_jugar_pressed():
	# cambiar a la escena del juego
	get_tree().change_scene_to_file("res://Game.tscn")

func _on_salir_pressed():
	get_tree().quit()
