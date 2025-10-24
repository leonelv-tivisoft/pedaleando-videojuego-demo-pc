extends MultiMeshInstance3D

func _ready():
	var count = 10  # cantidad de copias
	multimesh.instance_count = count
	
	for i in range(count):
		var transform = Transform3D()
		transform.origin = Vector3(i * 2, 0, 0)  # separa en el eje X
		multimesh.set_instance_transform(i, transform)
