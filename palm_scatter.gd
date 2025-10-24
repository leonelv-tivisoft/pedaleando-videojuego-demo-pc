@tool
extends MultiMeshInstance3D

# =========================
#   SCATTER + COLLIDERS
#   Godot 4.4.1 - GDScript
# =========================

# ---- ENTRADAS ----
@export var terrain: Node3D                                # Terreno con colisión (StaticBody3D o similar)
@export_flags_3d_physics var ground_mask := 1              # Capa de colisión del terreno (normalmente 1)
@export var instances: int = 150                           # Cuántas instancias
@export var area_size: Vector3 = Vector3(60, 0, 40)        # Rectángulo (X,Z) centrado en ESTE nodo
@export var seed: int = 12345

# Contacto con el suelo
@export var align_bottom_to_ground: bool = true            # Alinear la BASE del mesh al suelo
@export var embed_in_ground: float = -0.03                 # Hundir/elevar sobre la normal (negativo = hundir)

@export_group("Filtros")
@export var max_slope_deg: float = 22.0                    # Pendiente máxima (grados)
@export var water_level_y: float = 0.0                     # Altura del agua (Y)
@export var water_margin: float = 0.25                     # Margen por encima del agua

@export_group("Variación")
@export var min_scale: float = 0.85
@export var max_scale: float = 1.35
@export var random_yaw: bool = true
@export var tilt_with_normal: bool = true                  # Inclinación suave según normal
@export var tilt_strength: float = 0.25                    # 0..1 (0 = sin tilt, 1 = igualar normal)

@export_group("Separación")
@export var min_distance: float = 2.0                      # Separación mínima entre instancias (m)
@export var cell_size: float = 0.0                         # 0 = auto (min_distance * 0.7)
@export var separation_uses_scale: bool = false            # Si true, distancia *= escala

@export_group("Colisión (por palmera)")
@export var generate_colliders: bool = true
@export var collider_radius: float = 0.30                  # Radio del cilindro del tronco
@export var collider_height: float = 2.6                   # Altura del cilindro
@export var collider_scales_with_instance: bool = true     # Escalar collider con la instancia
@export var collider_align_with_normal: bool = false       # Inclinar collider con la normal
@export var collider_layer: int = 2                        # Capa del collider (árboles)
@export var collider_mask: int = 1                         # Qué capas detecta (ej. player en 1)

@export_group("Acción")
@export var regenerate: bool = false : set = _set_regen

func _set_regen(v: bool) -> void:
	if not v: return
	regenerate = false
	_scatter()

func _ready() -> void:
	if Engine.is_editor_hint():
		# _scatter()  # Descomenta si quieres auto-generar al abrir
		pass

# --- helpers para colisionadores ---
func _get_colliders_root() -> Node3D:
	var n := $Colliders if has_node("Colliders") else null
	if n == null:
		n = Node3D.new()
		n.name = "Colliders"
		add_child(n, true)
	return n

func _clear_children(node: Node) -> void:
	for c in node.get_children():
		c.queue_free()

# --- SCATTER PRINCIPAL ---
func _scatter() -> void:
	if multimesh == null or multimesh.mesh == null:
		push_warning("Asigna un MultiMesh y su Mesh antes de regenerar.")
		return
	if terrain == null:
		push_warning("Asigna 'terrain' (nodo de tu terreno con colisión).")
		return

	# Render
	multimesh.transform_format = MultiMesh.TRANSFORM_3D

	# Colliders
	var col_root := _get_colliders_root()
	_clear_children(col_root)

	var rng := RandomNumberGenerator.new()
	rng.seed = seed

	var half_x: float = maxf(area_size.x * 0.5, 0.01)
	var half_z: float = maxf(area_size.z * 0.5, 0.01)
	var center: Vector3 = global_transform.origin

	var dss: PhysicsDirectSpaceState3D = get_world_3d().direct_space_state
	var max_slope_cos: float = cos(deg_to_rad(max_slope_deg))
	var up: Vector3 = Vector3.UP

	# ==== Separación (grid hash en XZ) ====
	var cs: float = cell_size if cell_size > 0.0 else maxf(min_distance * 0.7, 0.1)
	var grid := {}                               # Dictionary<Vector2i, Array[Vector3]>
	var search_r: int = int(ceil(min_distance / cs))
	var min_dist_sq_base: float = min_distance * min_distance

	var placed: int = 0
	var max_tries: int = maxi(instances * 18, 200)
	multimesh.instance_count = instances

	for i in range(max_tries):
		if placed >= instances:
			break

		# Punto aleatorio dentro del rectángulo (XZ)
		var rx: float = rng.randf_range(-half_x, half_x)
		var rz: float = rng.randf_range(-half_z, half_z)

		var start: Vector3 = center + Vector3(rx, 50.0,  rz)    # raycast desde arriba
		var end:   Vector3 = center + Vector3(rx, -1000.0, rz)  # hacia abajo

		var q := PhysicsRayQueryParameters3D.create(start, end)
		q.collision_mask = ground_mask
		var hit := dss.intersect_ray(q)
		if hit.is_empty():
			continue

		var pos: Vector3 = hit.position
		var nrm: Vector3 = hit.normal.normalized()

		# Filtros
		if pos.y < (water_level_y + water_margin): continue
		if nrm.dot(up) < max_slope_cos: continue

		# Variación: escala + yaw
		var s: float = rng.randf_range(min_scale, max_scale)
		var yaw: float = rng.randf_range(0.0, TAU) if random_yaw else 0.0
		var basis := Basis(up, yaw)

		# Inclinación suave hacia la normal
		if tilt_with_normal and tilt_strength > 0.0:
			var axis: Vector3 = up.cross(nrm)
			if axis.length() > 0.0001:
				axis = axis.normalized()
				var angle: float = acos(clamp(up.dot(nrm), -1.0, 1.0)) * clamp(tilt_strength, 0.0, 1.0)
				basis = Basis(axis, angle) * basis

		basis = basis.scaled(Vector3(s, s, s))

		# ===== Separación en XZ (antes de corregir Y) =====
		var effective_min_sq: float = (min_dist_sq_base * s * s) if separation_uses_scale else min_dist_sq_base
		var cell := Vector2i(int(floor(pos.x / cs)), int(floor(pos.z / cs)))
		var too_close := false
		for dy in range(-search_r, search_r + 1):
			for dx in range(-search_r, search_r + 1):
				var key := Vector2i(cell.x + dx, cell.y + dy)
				if not grid.has(key): continue
				var arr: Array = grid[key]
				for p in arr:
					if Vector2(pos.x - p.x, pos.z - p.z).length_squared() < effective_min_sq:
						too_close = true
						break
				if too_close: break
			if too_close: break
		if too_close: continue

		# ===== Corrección de altura (para que no floten) =====
		if align_bottom_to_ground and multimesh.mesh:
			var aabb := multimesh.mesh.get_aabb()
			var origin_to_bottom: float = -aabb.position.y * s
			pos.y -= origin_to_bottom
		if embed_in_ground != 0.0:
			pos += nrm * embed_in_ground

		# --- Colocar instancia de render ---
		multimesh.set_instance_transform(placed, Transform3D(basis, pos))

		# --- Crear collider (opcional) ---
		if generate_colliders:
			var sb := StaticBody3D.new()
			sb.name = "PalmCol_%d" % [placed]
			col_root.add_child(sb, true)

			# Guardar en escena si estás en editor
			if Engine.is_editor_hint():
				sb.owner = get_tree().edited_scene_root

			# Capas/Máscaras
			sb.collision_layer = collider_layer
			sb.collision_mask  = collider_mask

			var shape := CollisionShape3D.new()
			var cyl := CylinderShape3D.new()        # Cambia a CapsuleShape3D si prefieres
			var r: float = collider_radius * (s if collider_scales_with_instance else 1.0)
			var h: float = collider_height * (s if collider_scales_with_instance else 1.0)
			cyl.radius = r
			cyl.height = maxf(h, 0.05)
			shape.shape = cyl
			sb.add_child(shape, true)

			# Centro del cilindro: base + h/2 (partiendo del punto de contacto original)
			var col_pos: Vector3 = hit.position + Vector3(0.0, (h * 0.5) + embed_in_ground, 0.0)
			var col_basis := Basis()                 # vertical por defecto
			if collider_align_with_normal:
				col_basis = Basis().looking_at(nrm, Vector3.UP)
			sb.global_transform = Transform3D(col_basis, col_pos)

		# Registrar en grid (usamos la posición de contacto original para separación XZ)
		if grid.has(cell):
			grid[cell].append(hit.position)
		else:
			grid[cell] = [hit.position]

		placed += 1

	# Recorta al número real colocado
	multimesh.instance_count = placed
	push_warning("Instancias colocadas (colisión: %s): %d" % [str(generate_colliders), placed])
