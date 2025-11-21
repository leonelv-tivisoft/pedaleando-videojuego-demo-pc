extends Node

@export var hover_sound: AudioStream
@export var click_sound: AudioStream

var player: AudioStreamPlayer

func _ready() -> void:
	player = AudioStreamPlayer.new()
	add_child(player)
	player.bus = "VFX"  # O "UI", según tu proyecto
	player.volume_db = 0.0

func play_hover():
	if hover_sound:
		player.stream = hover_sound
		player.play()

func play_click():
	if click_sound:
		player.stream = click_sound
		player.play()
