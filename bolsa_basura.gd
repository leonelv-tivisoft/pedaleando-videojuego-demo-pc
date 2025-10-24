extends StaticBody3D

@onready var BolsaBasura: StaticBody3D = $"."
@onready var OutLineMesh: MeshInstance3D = $"Trashbag - Full_001/MeshInstance3D"

var selected = false
var OutlineWidht = 0.02

func _ready() -> void:
	get_tree().get_first_node_in_group("Player").interact_object.connect(_set_select)
	
	OutLineMesh.visible = false 
	
	
	
func _process(delta):
	OutLineMesh.visible = selected
	
	if selected: BolsaBasura.position.y = OutlineWidht
	else: BolsaBasura.position.y = 0
	
func _set_select(object):
	selected = self == object
