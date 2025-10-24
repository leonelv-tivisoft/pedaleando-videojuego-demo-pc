extends Control

@onready var skip_button = $OMITIR

func _ready() -> void:
	skip_button.hide() #asegurarse de que no aparezca al inicio


func _on_skip_timer_timeout():
	skip_button.show() #mostrar el boton despues del tiempo

func _go_to_menu():
	get_tree().change_scene_to_file("res://MainMenu.tscn")

func _on_video_stream_player_finished() -> void:
	pass #
	get_tree().change_scene_to_file("res://MainMenu.tscn")


func _on_omitir_pressed() -> void:
	pass # Replace with function body.
	_go_to_menu()
