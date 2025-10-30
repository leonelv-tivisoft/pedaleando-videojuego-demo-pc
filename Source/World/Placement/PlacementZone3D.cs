using System;
using System.Collections.Generic;
using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Área de diseño (Area3D) donde está permitido ubicar basura (Trash).
    /// - El diseñador define el volumen mediante un CollisionShape3D con BoxShape3D (caja).
    /// - Este nodo muestrea puntos válidos dentro de dicho volumen según el tipo de área.
    /// - "Clearance (Holgura)": radio libre alrededor del punto para evitar solapes.
    /// - "MaxSlopeDeg (Pendiente máx)": filtro por inclinación del terreno.
    /// </summary>
    public partial class PlacementZone3D : Area3D
    {
        [Export] public PlacementKind Kind { get; set; } = PlacementKind.Ground; // Tipo principal de esta zona
        [Export] public bool Enabled { get; set; } = true;
        [Export] public float Weight { get; set; } = 1.0f; // Peso para selección aleatoria

        // Configuración de terreno/roca
        [Export(PropertyHint.Layers3DPhysics)] public uint GroundMask { get; set; } = 1 << 0;
        [Export(PropertyHint.Layers3DPhysics)] public uint RockMask { get; set; } = 1 << 2;
        [Export(PropertyHint.Range, "0,89,0.5")] public float MaxSlopeDeg { get; set; } = 35f;
        [Export(PropertyHint.Range, "0,0.5,0.01")] public float SurfaceOffset { get; set; } = 0.05f;

        // Configuración de agua
        [Export] public NodePath WaterSurfaceNode { get; set; }
        [Export] public float WaterSurfaceY { get; set; } = 0f;
        [Export(PropertyHint.Range, "0,20,0.05")] public float MinUnderwaterDepth { get; set; } = 0.5f;
        [Export(PropertyHint.Range, "0,50,0.05")] public float MaxUnderwaterDepth { get; set; } = 3.0f;

    // Fallback en caso de que no existan colisiones para suelo/roca
    [Export] public bool UseNodeYAsFallback { get; set; } = true;

        private Node3D _waterNode;

        public override void _Ready()
        {
            _waterNode = GetNodeOrNull<Node3D>(WaterSurfaceNode);
        }

        /// <summary>
        /// Intenta muestrear un punto válido dentro de esta zona.
        /// Retorna true si encuentra una posición que respeta Holgura (Clearance) y demás filtros.
        /// </summary>
        public bool TrySamplePoint(float clearanceRadius, out Vector3 position, int raycastPaddingMeters = 5)
        {
            position = default;
            if (!Enabled) return false;

            var space = GetWorld3D()?.DirectSpaceState;
            if (space == null) return false;

            if (!TryGetGlobalAabbFromBox(out var aabb, out var topY))
            {
                GD.PushWarning($"[PlacementZone3D] '{Name}' no tiene CollisionShape3D con BoxShape3D hijo. No se puede muestrear.");
                return false;
            }

            for (int i = 0; i < 96; i++) // más intentos para robustez
            {
                var pXZ = UniformRandomInAabbXZ(aabb);

                switch (Kind)
                {
                    case PlacementKind.Ground:
                    {
                        Vector3 candidate;
                        if (RaycastDown(space, pXZ, topY + raycastPaddingMeters, GroundMask, out var hitPos, out var hitNormal))
                        {
                            if (Vector3.Up.AngleTo(hitNormal) * Mathf.RadToDeg(1) > MaxSlopeDeg)
                                continue;
                            candidate = hitPos + hitNormal * SurfaceOffset;
                        }
                        else if (UseNodeYAsFallback)
                        {
                            var fallbackY = GlobalTransform.Origin.Y + SurfaceOffset;
                            candidate = new Vector3(pXZ.X, fallbackY, pXZ.Y);
                        }
                        else continue;

                        // Ajuste solicitado: Ground a Y=0 exacto
                        candidate.Y = 6.0f;
                        if (!HasClearance(space, candidate, clearanceRadius))
                            continue;

                        position = candidate;
                        return true;
                    }

                    case PlacementKind.OnRock:
                    {
                        Vector3 candidate;
                        if (RaycastDown(space, pXZ, topY + raycastPaddingMeters, RockMask, out var hitPos, out var hitNormal))
                        {
                            if (Vector3.Up.AngleTo(hitNormal) * Mathf.RadToDeg(1) > MaxSlopeDeg)
                                continue;
                            candidate = hitPos + hitNormal * SurfaceOffset;
                        }
                        else if (UseNodeYAsFallback)
                        {
                            var fallbackY = GlobalTransform.Origin.Y + SurfaceOffset;
                            candidate = new Vector3(pXZ.X, fallbackY, pXZ.Y);
                        }
                        else continue;

                        // Ajuste: OnRock también a Y=0 por petición
                        candidate.Y = 6.0f;
                        if (!HasClearance(space, candidate, clearanceRadius))
                            continue;

                        position = candidate;
                        return true;
                    }

                    case PlacementKind.WaterSurface:
                    {
                        // Ajuste: Superficie de agua a Y=0
                        var candidate = new Vector3(pXZ.X, 0f, pXZ.Y);

                        if (!HasClearance(space, candidate, clearanceRadius))
                            continue;

                        position = candidate;
                        return true;
                    }

                    case PlacementKind.Underwater:
                    {
                        var ySurf = GetWaterSurfaceY();
                        var depth = Mathf.Lerp(MinUnderwaterDepth, MaxUnderwaterDepth, GD.Randf());
                        var candidate = new Vector3(pXZ.X, ySurf - depth, pXZ.Y);
                        // Límite: máximo profundidad -5 (Y >= -5)
                        if (candidate.Y < -5f) candidate.Y = -5f;

                        if (!HasClearance(space, candidate, clearanceRadius))
                            continue;

                        position = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Altura Y de la superficie del agua (desde nodo o valor fijo).
        /// </summary>
        private float GetWaterSurfaceY() => _waterNode != null ? _waterNode.GlobalTransform.Origin.Y : WaterSurfaceY;

        /// <summary>
        /// Obtiene un AABB global exacto a partir de un BoxShape3D hijo (usando sus 8 vértices transformados a mundo).
        /// </summary>
        private bool TryGetGlobalAabbFromBox(out Aabb global, out float topY)
        {
            global = new Aabb();
            topY = 0f;

            var shapeNode = FindChildBoxShape();
            if (shapeNode == null) return false;

            var box = shapeNode.Shape as BoxShape3D;
            if (box == null) return false;

            var ext = box.Size * 0.5f;
            var corners = new Vector3[]
            {
                new(-ext.X, -ext.Y, -ext.Z), new(ext.X, -ext.Y, -ext.Z),
                new(-ext.X,  ext.Y, -ext.Z), new(ext.X,  ext.Y, -ext.Z),
                new(-ext.X, -ext.Y,  ext.Z), new(ext.X, -ext.Y,  ext.Z),
                new(-ext.X,  ext.Y,  ext.Z), new(ext.X,  ext.Y,  ext.Z)
            };

            var xform = shapeNode.GlobalTransform;
            Vector3 min = xform * corners[0];
            Vector3 max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                var w = xform * corners[i];
                min = new Vector3(Mathf.Min(min.X, w.X), Mathf.Min(min.Y, w.Y), Mathf.Min(min.Z, w.Z));
                max = new Vector3(Mathf.Max(max.X, w.X), Mathf.Max(max.Y, w.Y), Mathf.Max(max.Z, w.Z));
            }

            global = new Aabb(min, max - min);
            topY = max.Y;
            return true;
        }

        private CollisionShape3D FindChildBoxShape()
        {
            foreach (var child in GetChildren())
            {
                if (child is CollisionShape3D cs && cs.Shape is BoxShape3D)
                    return cs;
            }
            return null;
        }

        /// <summary>
        /// Muestrea coordenadas XZ uniformes dentro del AABB (espacio global).
        /// </summary>
        private static Vector2 UniformRandomInAabbXZ(Aabb aabb)
        {
            var x = (float)(aabb.Position.X + GD.Randf() * aabb.Size.X);
            var z = (float)(aabb.Position.Z + GD.Randf() * aabb.Size.Z);
            return new Vector2(x, z);
        }

        /// <summary>
        /// Raycast vertical hacia abajo para encontrar la superficie (suelo/roca) usando la máscara dada.
        /// </summary>
        private static bool RaycastDown(PhysicsDirectSpaceState3D space, Vector2 xz, float startY, uint mask,
            out Vector3 hitPos, out Vector3 hitNormal)
        {
            hitPos = default;
            hitNormal = Vector3.Up;

            var from = new Vector3(xz.X, startY, xz.Y);
            var to = new Vector3(xz.X, startY - 1000f, xz.Y);

            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollisionMask = mask;

            var result = space.IntersectRay(query);
            if (result.Count == 0) return false;

            hitPos = (Vector3)result["position"];
            hitNormal = (Vector3)result["normal"];
            return true;
        }

        /// <summary>
        /// Comprueba "Clearance (Holgura)" mediante un test de esfera contra cuerpos.
        /// </summary>
        private static bool HasClearance(PhysicsDirectSpaceState3D space, Vector3 pos, float radius)
        {
            if (radius <= 0f) return true;

            var sphere = new SphereShape3D { Radius = radius };
            var xform = new Transform3D(Basis.Identity, pos);
            var query = new PhysicsShapeQueryParameters3D
            {
                Shape = sphere,
                Transform = xform,
                CollisionMask = ~0u,
                CollideWithAreas = false,
                CollideWithBodies = true
            };

            var hits = space.IntersectShape(query, 4);
            return hits.Count == 0;
        }
    }
}
