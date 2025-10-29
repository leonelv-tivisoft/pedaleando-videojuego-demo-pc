extends Control


func _ready() -> void:
	# Localize button texts
	_apply_localization()
	var loc = get_node_or_null("/root/LocalizationManager")
	if loc:
		loc.connect("LanguageChanged", Callable(self, "_on_language_changed"))

	$Menu/SALIR.pressed.connect(_on_salir_pressed)
	$ExitDialog.confirmed.connect(_on_exit_dialog_confirmed)

func _on_salir_pressed() -> void:
	$ExitDialog.popup_centered() # muestra la ventana emergente en el centro

func _on_exit_dialog_confirmed() -> void:
	get_tree().quit()
 


func _on_jugar_pressed() -> void:
	pass # Cambiar de escena al primer nivel
	get_tree().change_scene_to_file("res://Nivel1_Demo.tscn")

func _apply_localization() -> void:
	var loc = get_node_or_null("/root/LocalizationManager")
	if loc and loc.has_method("GetText"):
		var btn_play: Button = $Menu/JUGAR
		var btn_options: Button = $Menu/HISTORIA
		var btn_exit: Button = $Menu/SALIR
		if btn_play:
			btn_play.text = loc.GetText("MENU_PLAY_DEMO")
		if btn_options:
			btn_options.text = loc.GetText("MENU_OPTIONS")
		if btn_exit:
			btn_exit.text = loc.GetText("MENU_EXIT")

func _on_language_changed(new_lang: String) -> void:
	_apply_localization()
