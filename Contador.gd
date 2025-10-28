# res://Contador.gd (adjúntalo al CanvasLayer "Contador")
extends CanvasLayer

@export var label_count: Label      # arrastra "ContadorBasuras"
@export var label_goal:  Label         # arrastra el Label que dice "/20"
@export var prefix:      String = ""   # si quieres "x" o nada
@export var show_with_goal: bool = true

var game_manager

func _ready() -> void:
	visible = true  # o false si quieres ocultarlo hasta que empiece el juego
	game_manager = get_node("/root/GameManager")
	# Estado inicial
	_update_text(game_manager.get("CurrentCount"), game_manager.get("CurrentGoal"))
	# Conecta para actualizar cuando cambie el conteo
	game_manager.connect("CountChanged", _on_count_changed)

func _on_count_changed(current: int, goal: int) -> void:
	_update_text(current, goal)

func _update_text(current: int, goal: int) -> void:
	# "00" formateado a 2 dígitos
	if label_count:
		label_count.text = "%s%02d" % [prefix, current]
	if label_goal:
		label_goal.text  = "/%02d" % goal
