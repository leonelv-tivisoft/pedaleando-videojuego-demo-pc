# res://GameManager.gd
# [DEPRECATED] This file is deprecated and will be removed. Use Source/Managers/GameManager.cs instead.
# TODO: Remove this file after full migration to C#
extends Node

signal count_changed(current: int, goal: int)

var count: int = 0
var goal: int = 20

func _ready() -> void:
	# Emite el estado inicial por si alguien ya está escuchando
	emit_signal(&"count_changed", count, goal)

func reset(new_goal: int = 20) -> void:
	count = 0
	goal = new_goal
	emit_signal(&"count_changed", count, goal)

func add_trash(amount: int = 1) -> void:
	count += amount
	emit_signal(&"count_changed", count, goal)
