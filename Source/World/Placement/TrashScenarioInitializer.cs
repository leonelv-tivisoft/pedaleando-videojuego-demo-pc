using System.Collections.Generic;
using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Inicializa la escena generando y colocando basura al inicio.
    /// Colocar este nodo en el nivel y configurar los tipos.
    /// </summary>
    public partial class TrashScenarioInitializer : Node
    {
        [Export] public NodePath ScenarioGeneratorPath { get; set; }
        [Export] public bool ClearChildrenOnStart { get; set; } = false;
        [Export] public bool DebugLogging { get; set; } = true; // Logs de creación y posiciones

        [Export] public Godot.Collections.Array<TrashTypeConfig> Types { get; set; } = new();

        [Export] public NodePath ParentForInstancesPath { get; set; } // Dónde instanciar (si no, este mismo nodo)

        // Física de caída / agua
    [Export] public NodePath WaterSurfaceNodePath { get; set; }
        [Export(PropertyHint.Range, "0,1,0.01")] public float ProbabilityFloat { get; set; } = 0.5f;
        [Export(PropertyHint.Range, "0,3,0.01")] public float FloatOffsetMax { get; set; } = 0.6f;
        [Export] public float StartHeightY { get; set; } = 30f;
        [Export(PropertyHint.Layers3DPhysics)] public uint GroundMask { get; set; } = 1;
        [Export(PropertyHint.Layers3DPhysics)] public uint RockMask { get; set; } = 4;

        private ScenarioGenerator _generator;
        private Node _parent;

        public override void _Ready()
        {
            _generator = GetNodeOrNull<ScenarioGenerator>(ScenarioGeneratorPath) ?? GetNodeOrNull<ScenarioGenerator>("../ScenarioGenerator");
            _parent = GetNodeOrNull<Node>(ParentForInstancesPath) ?? this;

            if (ClearChildrenOnStart)
            {
                // Limpia instancias previas (solo hijos directos)
                foreach (var child in _parent.GetChildren())
                    if (child is Node c && c != this) c.QueueFree();
            }

            GenerateAndSpawn();
        }

        private void GenerateAndSpawn()
        {
            if (_generator == null || Types == null || Types.Count == 0)
            {
                if (DebugLogging)
                    GD.Print("[TrashScenarioInitializer] Abort: faltan generator o types.");
                return;
            }
            if (DebugLogging)
                GD.Print($"[TrashScenarioInitializer] Generando escenario… tipos={Types.Count}");

            var plans = _generator.GenerateScenario(Types);
            if (DebugLogging)
                GD.Print($"[TrashScenarioInitializer] Planes generados: {plans.Count}");

            int totalPositions = 0;
            foreach (var plan0 in plans)
            {
                totalPositions += plan0.Positions?.Count ?? 0;
                if (DebugLogging)
                {
                    GD.Print($"  - Tipo: {plan0.Type?.ScenePath} -> {plan0.Positions?.Count ?? 0} posiciones");
                }
            }
            if (DebugLogging)
                GD.Print($"[TrashScenarioInitializer] Total posiciones: {totalPositions}");
            foreach (var plan in plans)
            {
                if (string.IsNullOrEmpty(plan.Type.ScenePath))
                {
                    if (DebugLogging)
                        GD.PushWarning("[TrashScenarioInitializer] ScenePath vacío en TrashTypeConfig.");
                    continue;
                }

                var packed = GD.Load<PackedScene>(plan.Type.ScenePath);
                if (packed == null)
                {
                    GD.PushWarning($"[TrashScenarioInitializer] No se pudo cargar escena: {plan.Type.ScenePath}");
                    continue;
                }

                foreach (var pos in plan.Positions)
                {
                    var inst = packed.Instantiate();
                    if (inst == null)
                    {
                        if (DebugLogging)
                            GD.PushWarning($"[TrashScenarioInitializer] Falló instancia de {plan.Type.ScenePath}");
                        continue;
                    }
                    _parent.AddChild(inst);
                    if (inst is Node3D n3)
                    {
                        // Siempre iniciar en altura fija y dejar que la física de caída resuelva Y
                        var spawn = new Vector3(pos.X, StartHeightY, pos.Z);
                        n3.GlobalTransform = new Transform3D(n3.GlobalTransform.Basis, spawn);

                        // Añadir componente de caída/flotabilidad
                        var fallComp = new TrashPlacementPhysics
                        {
                            StartHeightY = StartHeightY,
                            WaterSurfaceNodePath = WaterSurfaceNodePath,
                            ProbabilityFloat = ProbabilityFloat,
                            FloatOffsetMax = FloatOffsetMax,
                            GroundMask = GroundMask,
                            RockMask = RockMask
                        };
                        n3.AddChild(fallComp);

                        if (DebugLogging)
                            GD.Print($"[TrashScenarioInitializer] Spawned {plan.Type.ScenePath} at X={spawn.X:0.00}, Y={spawn.Y:0.00}, Z={spawn.Z:0.00} (falling)");
                    }
                    else
                    {
                        GD.PushWarning($"[TrashScenarioInitializer] La raíz de {plan.Type.ScenePath} no es Node3D. No se puede asignar posición 3D.");
                    }
                }
            }
            if (DebugLogging)
                GD.Print("[TrashScenarioInitializer] Spawning completado.");
        }
    }
}
