extends MultiMeshInstance3D

func _ready():
	var mm = multimesh
	for i in range(mm.instance_count):
		var t = Transform3D()
		t.origin = Vector3(randf_range(-20, 20), 0, randf_range(-20, 20))
		mm.set_instance_transform(i, t)
