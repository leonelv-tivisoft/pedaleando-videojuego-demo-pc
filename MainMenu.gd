extends Control


func _ready() -> void:
	$Menu/SALIR.pressed.connect(_on_salir_pressed)
	$ExitDialog.confirmed.connect(_on_exit_dialog_confirmed)

func _on_salir_pressed() -> void:
	$ExitDialog.popup_centered() # muestra la ventana emergente en el centro

func _on_exit_dialog_confirmed() -> void:
	get_tree().quit()
 


func _on_jugar_pressed() -> void:
	pass # Cambiar de escena al primer nivel
	get_tree().change_scene_to_file("res://Nivel1_Demo.tscn")
