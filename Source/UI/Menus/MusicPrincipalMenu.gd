extends AudioStreamPlayer

@onready var sld_music: HSlider = $"../SoundSldiers/Music/HSlider"
@onready var sld_vfx: HSlider   = $"../SoundSldiers/VFX/HSlider"

func _ready() -> void:
	# Rango 0..1 mapea a dB: -40 dB (casi mudo) a 0 dB (100%)
	sld_music.value_changed.connect(func(v):
		AudioServer.set_bus_volume_db(
			AudioServer.get_bus_index("Music"),
			lerp(-40.0, 0.0, v)
		)
	)
	sld_vfx.value_changed.connect(func(v):
		AudioServer.set_bus_volume_db(
			AudioServer.get_bus_index("VFX"),
			lerp(-40.0, 0.0, v)
		)
	)
