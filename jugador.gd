extends CharacterBody3D
#VARIABLES CAMINAR#
var velocidad:int = 5

var direccion:Vector3



#MOVER CAMARA VARIABLES#

var sensibilidad:float = 0.001
@onready var camara:Camera3D = $Camera3D

func _physics_process(_delta):
	caminar()
	move_and_slide()
	
func _input(event):
	mover_camara(event)
	




func caminar():
	direccion = transform.basis * Vector3(Input.get_axis("Izquierda","Derecha"),0,Input.get_axis("Adelante","Atras")).normalized()
	
	velocity.x = direccion.x * velocidad
	velocity.z = direccion.z * velocidad
	
func mover_camara(event):
	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * sensibilidad)
		
		camara.rotate_x(-event.relative.y * sensibilidad)
