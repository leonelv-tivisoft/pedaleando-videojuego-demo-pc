extends Control

@onready var sld_vfx: HSlider = $VFX/HSlider

# Rango en dB que usaremos
const MIN_DB := -40.0
const MAX_DB := 0.0

func _ready() -> void:
	# Inicializar el valor del slider según el bus
	var vfx_idx := AudioServer.get_bus_index("VFX")
	var current_db := AudioServer.get_bus_volume_db(vfx_idx)

	# Asegúrate de que el slider tenga min = 0, max = 1 en el Inspector
	sld_vfx.min_value = 0.0
	sld_vfx.max_value = 1.0

	# Convertir dB actual a 0..1
	var v := inverse_lerp(MIN_DB, MAX_DB, clamp(current_db, MIN_DB, MAX_DB))
	sld_vfx.value = v

	# Conectar el cambio de valor
	sld_vfx.value_changed.connect(_on_vfx_slider_changed)

func _on_vfx_slider_changed(value: float) -> void:
	var vfx_idx: int = AudioServer.get_bus_index("VFX")
	# Pasar de 0..1 a dB
	var db: float = lerp(MIN_DB, MAX_DB, value)
	AudioServer.set_bus_volume_db(vfx_idx, db)
