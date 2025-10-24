extends ConfirmationDialog

func _ready() -> void:
	
	# Captura la señal del boton "Si"
	self.confirmed.connect(_on_exit_confirmed)
	
	# Captura la señal del boton "No"
	get_cancel_button().pressed.connect(_on_exit_canceled)
	

	# Estilo para botones
	var btn_style = StyleBoxFlat.new()
	btn_style.bg_color = Color(0.3, 0.6, 1.0)
	btn_style.set_corner_radius_all(10)

	# Acceder a los botones correctamente
	var ok_btn = get_ok_button()
	var cancel_btn = get_cancel_button()

	ok_btn.add_theme_stylebox_override("hover", btn_style)
	cancel_btn.add_theme_stylebox_override("hover", btn_style)
	
func _on_exit_confirmed():
	get_tree().quit()     # Cerramos el juego
	
func _on_exit_canceled():
	hide() # Se oculta el dialogo
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED) # Se vuelve a capturar el mouse
