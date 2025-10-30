using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Genera un escenario de posiciones para basura, respetando:
    /// - Tipos permitidos y sus zonas (PlacementZone3D).
    /// - Holguras y distancias mínimas entre objetos.
    /// - Presupuesto de tiempo (estimado) para recoger todo desde un origen.
    /// </summary>
    public partial class ScenarioGenerator : Node
    {
        [Export] public NodePath PlacementServicePath { get; set; }
        [Export] public NodePath PlayerStartNodePath { get; set; }

        [Export(PropertyHint.Range, "0.5,20,0.1")] public float PlayerSpeedMps { get; set; } = 4.0f;
        [Export(PropertyHint.Range, "10,600,1")] public float TimeBudgetSeconds { get; set; } = 180f; // Por defecto 180s, modificable

        [Export] public int TotalObjects { get; set; } = 20;
        [Export] public float GlobalMinSeparation { get; set; } = 1.5f;

    // Límites globales de mundo
    [Export] public bool UseXMinLimit { get; set; } = true;
    [Export(PropertyHint.Range, "-10000,10000,0.1")] public float XMinLimit { get; set; } = -200f;
    [Export] public bool UseXMaxLimit { get; set; } = true;
    [Export(PropertyHint.Range, "-10000,10000,0.1")] public float XMaxLimit { get; set; } = 66f;
    [Export] public bool UseZMinLimit { get; set; } = true;
    [Export(PropertyHint.Range, "-10000,10000,0.1")] public float ZMinLimit { get; set; } = -221f;
    [Export] public bool UseZMaxLimit { get; set; } = true;
    [Export(PropertyHint.Range, "-10000,10000,0.1")] public float ZMaxLimit { get; set; } = 225f;

        [Export] public bool DebugLogging { get; set; } = false;
    [Export] public bool ClampWithJitterInsteadReject { get; set; } = true; // Si true, clampa con jitter en vez de descartar
    [Export(PropertyHint.Range, "0,5,0.01")] public float ClampJitterMax { get; set; } = 0.8f;

        private MapPlacementService _service;
        private Node3D _playerStart;

        public override void _Ready()
        {
            _service = GetNodeOrNull<MapPlacementService>(PlacementServicePath) ?? GetTree().Root.GetNodeOrNull<MapPlacementService>("MapPlacementService");
            if (_service == null)
            {
                _service = new MapPlacementService();
                AddChild(_service);
                _service.DiscoverZones();
            }

            _playerStart = GetNodeOrNull<Node3D>(PlayerStartNodePath);
        }

        public struct SpawnRequest
        {
            public TrashTypeConfig Type;
            public int Count;
        }

        public struct SpawnPlan
        {
            public TrashTypeConfig Type;
            public List<Vector3> Positions;
        }

        /// <summary>
        /// Construye las cantidades por tipo según FixedCount o reparto ponderado hasta TotalObjects.
        /// </summary>
        public List<SpawnRequest> BuildRequests(IList<TrashTypeConfig> types)
        {
            var requests = new List<SpawnRequest>();
            int fixedSum = 0;
            foreach (var t in types)
            {
                if (t.FixedCount > 0)
                {
                    requests.Add(new SpawnRequest { Type = t, Count = t.FixedCount });
                    fixedSum += t.FixedCount;
                }
            }

            int remaining = Mathf.Max(0, TotalObjects - fixedSum);
            if (remaining > 0)
            {
                // Reparto ponderado por Weight
                float totalW = types.Sum(x => Mathf.Max(0.0001f, x.Weight));
                for (int i = 0; i < remaining; i++)
                {
                    float p = GD.Randf() * totalW;
                    float acc = 0f;
                    foreach (var t in types)
                    {
                        acc += Mathf.Max(0.0001f, t.Weight);
                        if (p <= acc)
                        {
                            var idx = requests.FindIndex(r => r.Type == t);
                            if (idx >= 0) { var rr = requests[idx]; rr.Count++; requests[idx] = rr; }
                            else requests.Add(new SpawnRequest { Type = t, Count = 1 });
                            break;
                        }
                    }
                }
            }

            return requests.Where(r => r.Count > 0).ToList();
        }

        /// <summary>
        /// Intenta generar posiciones para todos los tipos cumpliendo restricciones y presupuesto de tiempo.
        /// Hace algunos reintentos con diferente muestreo si no cabe en el tiempo.
        /// </summary>
        public List<SpawnPlan> GenerateScenario(IList<TrashTypeConfig> types, int retries = 4)
        {
            if (_service == null) return new List<SpawnPlan>();
            // Asegura zonas descubiertas incluso si el servicio se inicializó antes que las zonas
            _service.DiscoverZones();
            var start = _playerStart?.GlobalTransform.Origin ?? Vector3.Zero;

            List<SpawnRequest> requests = BuildRequests(types);
            List<SpawnPlan> best = null;
            float bestOver = float.MaxValue;

            for (int attempt = 0; attempt < Mathf.Max(1, retries); attempt++)
            {
                var plans = new List<SpawnPlan>();
                var allPositions = new List<Vector3>();
                int rejectedByBounds = 0;
                int clampedByBounds = 0;

                foreach (var req in requests)
                {
                    var positions = new List<Vector3>();
                    int guard = 0;
                    while (positions.Count < req.Count && guard++ < req.Count * 200)
                    {
                        if (!_service.TryGetRandomLocationAny(req.Type.AllowedKinds, req.Type.ClearanceRadius, out var pos))
                            break;

                        // Límites globales con clamp+jitter
                        bool outOfBounds = false;
                        // X min
                        if (UseXMinLimit && pos.X < XMinLimit) { outOfBounds = true; pos.X = XMinLimit; }
                        // X max
                        if (UseXMaxLimit && pos.X > XMaxLimit) { outOfBounds = true; pos.X = XMaxLimit; }
                        // Z min
                        if (UseZMinLimit && pos.Z < ZMinLimit) { outOfBounds = true; pos.Z = ZMinLimit; }
                        // Z max
                        if (UseZMaxLimit && pos.Z > ZMaxLimit) { outOfBounds = true; pos.Z = ZMaxLimit; }

                        if (outOfBounds)
                        {
                            if (ClampWithJitterInsteadReject)
                            {
                                // Empuja ligeramente hacia el interior para evitar línea perfecta en el borde
                                float j = ClampJitterMax > 0 ? (float)GD.RandRange(0.05, ClampJitterMax) : 0f;
                                if (UseXMinLimit && Mathf.IsEqualApprox(pos.X, XMinLimit)) pos.X += j;
                                else if (UseXMaxLimit && Mathf.IsEqualApprox(pos.X, XMaxLimit)) pos.X -= j;
                                if (UseZMinLimit && Mathf.IsEqualApprox(pos.Z, ZMinLimit)) pos.Z += j;
                                else if (UseZMaxLimit && Mathf.IsEqualApprox(pos.Z, ZMaxLimit)) pos.Z -= j;
                                clampedByBounds++;
                            }
                            else
                            {
                                rejectedByBounds++;
                                continue;
                            }
                        }

                        // distancia mínima global y por tipo
                        bool tooClose = allPositions.Any(p => p.DistanceTo(pos) < GlobalMinSeparation)
                            || positions.Any(p => p.DistanceTo(pos) < req.Type.MinSeparation);
                        if (tooClose) continue;

                        positions.Add(pos);
                        allPositions.Add(pos);
                    }

                    plans.Add(new SpawnPlan { Type = req.Type, Positions = positions });
                }

                // Evalúa tiempo estimado
                float seconds = MapPlacementService.EstimateRouteSeconds(start, allPositions, PlayerSpeedMps);
                if (DebugLogging)
                {
                    GD.Print($"[ScenarioGenerator] attempt={attempt+1}, posiciones={allPositions.Count}, rechazadas_por_bounds={rejectedByBounds}, clamp_bounds={clampedByBounds}, tiempo_est={seconds:0.0}s");
                }
                if (seconds <= TimeBudgetSeconds) return plans; // éxito dentro del presupuesto

                float over = seconds - TimeBudgetSeconds;
                if (over < bestOver)
                {
                    bestOver = over;
                    best = plans;
                }
            }

            // Si no cabe, devuelve el mejor intento (más cercano al presupuesto)
            return best ?? new List<SpawnPlan>();
        }
    }
}
