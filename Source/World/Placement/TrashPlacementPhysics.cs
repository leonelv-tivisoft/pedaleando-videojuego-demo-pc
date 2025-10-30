using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Hace que un objeto caiga desde una altura inicial y se estabilice en el suelo.
    /// Si entra al agua: según flotabilidad (aleatoria por instancia), flota cerca de la superficie o se hunde hasta el fondo.
    /// Este nodo debe ser hijo del objeto a posicionar; moverá la posición global de su padre (Node3D).
    /// </summary>
    public partial class TrashPlacementPhysics : Node
    {
        [Export] public bool Enabled { get; set; } = true;

        // Caída inicial
        [Export] public float StartHeightY { get; set; } = 30f;
        [Export] public float Gravity { get; set; } = 9.8f;
        [Export] public float MaxFallSpeed { get; set; } = 30f;
        [Export] public float GroundSnapMargin { get; set; } = 0.05f;

        // Agua / Flotabilidad
        [Export] public NodePath WaterSurfaceNodePath { get; set; }
        [Export(PropertyHint.Range, "0,1,0.01")] public float ProbabilityFloat { get; set; } = 0.5f;
        [Export(PropertyHint.Range, "0,3,0.01")] public float FloatOffsetMax { get; set; } = 0.6f;
        [Export] public float WaterDamping { get; set; } = 6.0f;
        [Export] public float FloatStiffness { get; set; } = 10.0f; // fuerza hacia superficie (flotador)
        [Export] public float MaxSinkSpeed { get; set; } = 3.0f;

        // Colisiones de terreno
        [Export(PropertyHint.Layers3DPhysics)] public uint GroundMask { get; set; } = 1; // suelo
        [Export(PropertyHint.Layers3DPhysics)] public uint RockMask { get; set; } = 4; // rocas

        private Node3D _parent3D;
        private Node3D _waterNode;
        private bool _isFloater;
        private float _floatOffset;
        private float _vy;
        private bool _settled;

        public override void _Ready()
        {
            _parent3D = GetParent() as Node3D;
            if (_parent3D == null)
            {
                GD.PushWarning("[TrashPlacementPhysics] Parent no es Node3D; componente inactivo.");
                Enabled = false;
                return;
            }

            SetPhysicsProcess(true);

            if (WaterSurfaceNodePath != null && !WaterSurfaceNodePath.IsEmpty)
            {
                _waterNode = GetNodeOrNull<Node3D>(WaterSurfaceNodePath);
            }

            // Y inicial
            var p = _parent3D.GlobalTransform.Origin;
            p.Y = StartHeightY;
            _parent3D.GlobalTransform = new Transform3D(_parent3D.GlobalTransform.Basis, p);

            // Flotabilidad aleatoria por instancia
            _isFloater = GD.Randf() < ProbabilityFloat;
            _floatOffset = FloatOffsetMax > 0 ? (float)GD.RandRange(0.0, FloatOffsetMax) : 0f;
            _vy = 0f;
            _settled = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Enabled || _settled || _parent3D == null) return;

            var dt = (float)delta;
            var pos = _parent3D.GlobalTransform.Origin;
            var space = _parent3D.GetWorld3D()?.DirectSpaceState;
            if (space == null) return;

            // Altura de superficie de agua (si existe)
            bool hasWater = _waterNode != null;
            float waterY = hasWater ? _waterNode.GlobalTransform.Origin.Y : 0f;
            bool inWater = hasWater && pos.Y < waterY; // Aproximación: bajo el plano de agua sólo si hay agua

            if (inWater)
            {
                if (_isFloater)
                {
                    // Flotar hacia superficie + offset
                    float targetY = waterY + _floatOffset;
                    float dy = targetY - pos.Y;
                    // Movimiento crítico-amortiguado simple hacia target (tipo muelle con amortiguación)
                    _vy += FloatStiffness * dy * dt;
                    _vy -= WaterDamping * _vy * dt;
                    pos.Y += _vy * dt;
                }
                else
                {
                    // Hundirse con damping
                    _vy = Mathf.MoveToward(_vy, -MaxSinkSpeed, WaterDamping * dt);
                    pos.Y += _vy * dt;
                }
            }
            else
            {
                // Caída libre en aire
                _vy = Mathf.Clamp(_vy - Gravity * dt, -MaxFallSpeed, MaxFallSpeed);
                pos.Y += _vy * dt;
            }

            // Clamp contra el suelo (suelo o rocas)
            var from = new Vector3(pos.X, pos.Y + 0.5f, pos.Z);
            var to = new Vector3(pos.X, pos.Y - 1000f, pos.Z);
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollisionMask = GroundMask | RockMask;
            var hit = space.IntersectRay(query);
            if (hit.Count > 0)
            {
                var hitPos = (Vector3)hit["position"];
                if (pos.Y <= hitPos.Y + GroundSnapMargin)
                {
                    pos.Y = hitPos.Y;
                    _vy = 0f;
                    // Si no está en agua o es hundidor que ya tocó fondo, lo consideramos asentado
                    if (!inWater || !_isFloater)
                    {
                        _settled = true;
                    }
                }
            }

            _parent3D.GlobalTransform = new Transform3D(_parent3D.GlobalTransform.Basis, pos);
        }
    }
}
