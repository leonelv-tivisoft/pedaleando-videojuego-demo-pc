using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Servicio que descubre zonas de colocación (PlacementZone3D) en la escena y
    /// ofrece utilidades para muestrear ubicaciones válidas de forma agregada.
    /// </summary>
    public partial class MapPlacementService : Node
    {
        private readonly List<PlacementZone3D> _zones = new();

        [Export] public NodePath RootSearch { get; set; } // Si se establece, se buscan zonas sólo bajo este nodo.

        public override void _Ready()
        {
            // Defer para asegurar que el árbol completo esté presente
            CallDeferred(nameof(DiscoverZones));
        }

        public void DiscoverZones()
        {
            _zones.Clear();
            Node root = GetNodeOrNull(RootSearch) ?? GetTree().CurrentScene ?? this;
            AddZonesRecursive(root);
            if (_zones.Count == 0)
                GD.PushWarning("[MapPlacementService] No se encontraron PlacementZone3D en la escena.");
        }

        private void AddZonesRecursive(Node n)
        {
            if (n is PlacementZone3D zone && zone.Enabled)
                _zones.Add(zone);
            foreach (var child in n.GetChildren())
                AddZonesRecursive(child as Node);
        }

        /// <summary>
        /// Intenta obtener una ubicación aleatoria válida para cualquiera de los tipos indicados.
        /// Se pondera por Weight de cada zona que soporte el tipo.
        /// </summary>
        public bool TryGetRandomLocationAny(PlacementKind kinds, float clearance, out Vector3 position)
        {
            position = default;
            var supported = _zones.Where(z => (z.Kind & kinds) != 0 && z.Enabled).ToList();
            if (supported.Count == 0) return false;

            // Ruleta por peso
            var totalWeight = supported.Sum(z => Mathf.Max(0.0001f, z.Weight));
            for (int attempt = 0; attempt < 256; attempt++)
            {
                var pick = GD.Randf() * totalWeight;
                PlacementZone3D chosen = null;
                float acc = 0f;
                foreach (var z in supported)
                {
                    acc += Mathf.Max(0.0001f, z.Weight);
                    if (pick <= acc) { chosen = z; break; }
                }
                chosen ??= supported[^1];

                if (chosen.TrySamplePoint(clearance, out var pos))
                {
                    position = pos;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Genera un lote de ubicaciones evitando estar demasiado cerca entre sí.
        /// </summary>
        public List<Vector3> GetBatchLocations(int count, PlacementKind kinds, float clearance, float minSeparation)
        {
            var result = new List<Vector3>(count);
            var guard = 0;
            while (result.Count < count && guard++ < count * 50)
            {
                if (!TryGetRandomLocationAny(kinds, clearance, out var pos))
                    break;

                bool tooClose = false;
                foreach (var p in result)
                {
                    if (p.DistanceTo(pos) < minSeparation) { tooClose = true; break; }
                }
                if (tooClose) continue;

                result.Add(pos);
            }
            return result;
        }

        /// <summary>
        /// Heurística simple de distancia (ruta visita-vecino) para estimar tiempo.
        /// </summary>
        public static float EstimateRouteSeconds(Vector3 start, IList<Vector3> points, float speedMps)
        {
            if (points == null || points.Count == 0) return 0f;
            var remaining = new List<Vector3>(points);
            var current = start;
            float total = 0f;
            while (remaining.Count > 0)
            {
                int bestIdx = 0;
                float bestDist = current.DistanceTo(remaining[0]);
                for (int i = 1; i < remaining.Count; i++)
                {
                    var d = current.DistanceTo(remaining[i]);
                    if (d < bestDist) { bestDist = d; bestIdx = i; }
                }
                total += bestDist / Mathf.Max(0.01f, speedMps);
                current = remaining[bestIdx];
                remaining.RemoveAt(bestIdx);
            }
            return total;
        }
    }
}
