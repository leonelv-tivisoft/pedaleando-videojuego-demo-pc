extends CanvasLayer

@onready var fade_rect: ColorRect = $ColorRect

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Arranca el nivel con la pantalla negra
	fade_rect.modulate.a = 1.0
	
	pass # Hace el fundido de negro a transparente en 2 segundos
	var tween = create_tween()
	tween.tween_property(fade_rect, "modulate:a", 0.0, 2.0)
	
