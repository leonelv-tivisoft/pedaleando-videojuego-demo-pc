extends ConfirmationDialog

func _ready() -> void:
		

	# Estilo para botones
	var btn_style = StyleBoxFlat.new()
	btn_style.bg_color = Color(0.3, 0.6, 1.0)
	btn_style.set_corner_radius_all(10)

	# Acceder a los botones correctamente
	var ok_btn = get_ok_button()
	var cancel_btn = get_cancel_button()

	ok_btn.add_theme_stylebox_override("hover", btn_style)
	cancel_btn.add_theme_stylebox_override("hover", btn_style)
