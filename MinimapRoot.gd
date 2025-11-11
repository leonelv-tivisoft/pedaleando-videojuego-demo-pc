extends Node3D

@onready var vp: SubViewport = $MinimapVP
@onready var cam: Camera3D = $MinimapCamera
@export var player: Node3D

func _ready():
	# Conecta el SubViewport al mundo del juego
	vp.world_3d = get_viewport().world_3d
	# Activa la cámara para que el minimapa renderice
	cam.current = true

func _process(_delta):
	if not player:
		return
	# La cámara sigue al jugador desde arriba
	var pos = player.global_position
	cam.global_position = Vector3(pos.x, 200.0, pos.z)
