# res://dialogue_box.gd
class_name DialogueBox
extends Control

@export var type_speed: float = 0.02            # segundos por carácter
@export var show_name_as_title: bool = true     # usa "name" como título

@onready var panel: Panel           = $"Panel"
@onready var title: Label           = $"Panel/Title"
@onready var text: RichTextLabel    = $"Panel/Text"      # Debe ser RichTextLabel
@onready var hint: Label            = $"Panel/Hint"

var _lines: Array[Dictionary] = []   # [{ "name": String, "text": String }, ...]
var _idx: int = 0
var _typing: bool = false
var _on_finish: Callable = Callable()

func _ready() -> void:
	# (opcional) validaciones amistosas
	if text == null:
		push_error("DialogueBox: 'Panel/Text' no encontrado o no es RichTextLabel.")
		return

	focus_mode = Control.FOCUS_ALL
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_process_unhandled_input(true)

	visible = false
	text.bbcode_enabled = true
	text.text = ""
	hint.visible = false

func start_dialogue(lines: Array[Dictionary], on_finish: Callable = Callable()) -> void:
	_lines = lines
	_idx = 0
	_on_finish = on_finish
	visible = true
	_show_line()

func skip_all() -> void:
	_finish()

func _show_line() -> void:
	if _idx >= _lines.size():
		_finish()
		return

	var line: Dictionary = _lines[_idx]
	if show_name_as_title:
		title.text = str(line.get("name",""))
	text.text = ""
	hint.visible = false
	_typing = true

	var s: String = str(line.get("text",""))
	var tw := create_tween()
	for i in s.length():
		var ch := s.substr(i, 1)
		tw.tween_callback(func(): text.append_text(ch))
		tw.tween_interval(type_speed)
	tw.tween_callback(func():
		_typing = false
		hint.visible = true
	)

func _unhandled_input(event: InputEvent) -> void:
	if not visible:
		return

	var next_pressed: bool = (
		event.is_action_pressed("dialog_next") or      # ESPACIO (añádelo al Input Map)
		event.is_action_pressed("ui_accept") or        # Enter / A
		(event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT)
	)

	if next_pressed:
		if _typing:
			var s: String = str(_lines[_idx].get("text",""))
			text.text = s
			_typing = false
			hint.visible = true
		else:
			_idx += 1
			_show_line()
		accept_event()  # no dejes que pase al juego

func _finish() -> void:
	visible = false
	text.text = ""
	hint.visible = false
	if _on_finish.is_valid():
		_on_finish.call()
