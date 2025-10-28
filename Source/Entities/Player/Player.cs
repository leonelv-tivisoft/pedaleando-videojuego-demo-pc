using Godot;
using System;

namespace PedaleandoGame.Entities.Player
{
    public partial class Player : CharacterBody3D
    {
        [Export] public bool DisableOccludersWhenUnderwater { get; set; } = true;

        // Controles existentes
        [Export] public float Sensibilidad { get; set; } = 0.003f;
        [Export] public float Velocidad { get; set; } = 5.0f;
        [Export] public float Gravedad { get; set; } = 9.8f;
        [Export] public float FuerzaSalto { get; set; } = 5.0f;

        [Signal] public delegate void InteractObjectEventHandler(Node collider);

        private RayCast3D _rayCast3D;
        private Camera3D _camera;
        private Node3D _pivot;
        private ConfirmationDialog _exitDialog;

        // Agua / Submarino
        [Export] public Area3D WaterArea { get; set; }
        [Export] public Node3D WaterSurface { get; set; }
        [Export] public float SurfaceMargin { get; set; } = 0.25f;
        [Export] public ShaderMaterial WaterMaterial { get; set; }
        [Export] public WorldEnvironment WorldEnv { get; set; }
        [Export] public Godot.Environment EnvDefault;
        [Export] public Godot.Environment EnvUnderwater;

        [ExportGroup("Swim")]
        [Export] public float SwimSpeed { get; set; } = 5.0f;
        [Export] public float SwimAccel { get; set; } = 6.0f;
        [Export] public float SwimDecel { get; set; } = 5.0f;
        [Export] public float SwimVerticalSpeed { get; set; } = 3.2f;
        [Export] public float Buoyancy { get; set; } = 2.8f;
        [Export] public float WaterGravity { get; set; } = 0.6f;
        [Export] public float WaterDrag { get; set; } = 2.0f;
        [Export] public float MaxVerticalSpeed { get; set; } = 4.0f;

        private Vector3 _direction = Vector3.Zero;
        private float _rotationX = 0.0f;

        private bool _inWater = false;
        private bool _underwater = false;
        private int _waterCounter = 0;

        public override void _Ready()
        {
            AddToGroup("Player");

            _rayCast3D = GetNode<RayCast3D>("Pivot/PlayerCamera/RayCast3D");
            _camera = GetNode<Camera3D>("Pivot/PlayerCamera");
            _pivot = GetNode<Node3D>("Pivot");
            _exitDialog = GetNode<ConfirmationDialog>("../ExitDialog");

            Input.MouseMode = Input.MouseModeEnum.Captured;

            if (WaterArea != null)
            {
                WaterArea.BodyEntered += OnWaterBodyEntered;
                WaterArea.BodyExited += OnWaterBodyExited;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                // Rotación horizontal del jugador completo
                RotateY(-mouseMotion.Relative.X * Sensibilidad);
                
                // Rotación vertical de la cámara (pivot)
                _rotationX -= mouseMotion.Relative.Y * Sensibilidad;
                _rotationX = Mathf.Clamp(_rotationX, Mathf.DegToRad(-80), Mathf.DegToRad(80));
                _pivot.Rotation = new Vector3(_rotationX, 0, 0);
            }

            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_exitDialog.Visible)
                {
                    _exitDialog.Hide();
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
                else
                {
                    _exitDialog.Show();
                    _exitDialog.GrabFocus();
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                }
            }
        }

        public override void _Process(double delta)
        {
            if (_rayCast3D.IsColliding())
            {
                var collider = _rayCast3D.GetCollider();
                EmitSignal("InteractObject", collider);
            }
            else
            {
                EmitSignal("InteractObject", null);
            }

            if (Input.IsActionJustPressed("interact") && _rayCast3D.IsColliding())
            {
                var collider = _rayCast3D.GetCollider();
                if (collider != null && collider.HasMethod("interact"))
                {
                    collider.Call("interact", this);
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            UpdateUnderwaterFlag();

            if (_inWater)
            {
                SwimPhysics(delta);
            }
            else
            {
                WalkPhysics(delta);
            }
        }

        private void WalkPhysics(double delta)
        {
            _direction = Vector3.Zero;

            if (Input.IsActionPressed("Adelante"))
                _direction -= Transform.Basis.Z;
            if (Input.IsActionPressed("Atras"))
                _direction += Transform.Basis.Z;
            if (Input.IsActionPressed("Izquierda"))
                _direction -= Transform.Basis.X;
            if (Input.IsActionPressed("Derecha"))
                _direction += Transform.Basis.X;

            _direction = _direction.Normalized();

            if (!IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y - Gravedad * (float)delta, Velocity.Z);
            }
            else
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                if (Input.IsActionJustPressed("Saltar"))
                {
                    Velocity = new Vector3(Velocity.X, FuerzaSalto, Velocity.Z);
                }
            }

            var horizontalVelocity = _direction * Velocidad;
            Velocity = new Vector3(horizontalVelocity.X, Velocity.Y, horizontalVelocity.Z);

            MoveAndSlide();
        }

        private void SwimPhysics(double delta)
        {
            var inputVec = new Vector2(
                Input.GetActionStrength("Derecha") - Input.GetActionStrength("Izquierda"),
                Input.GetActionStrength("Atras") - Input.GetActionStrength("Adelante")
            );

            Vector3 dir = Vector3.Zero;
            if (inputVec.Length() > 0.0f)
            {
                dir = (Transform.Basis.X * inputVec.X + Transform.Basis.Z * inputVec.Y).Normalized();
            }

            var targetH = dir * SwimSpeed;
            var a = dir != Vector3.Zero ? SwimAccel : SwimDecel;

            Velocity = new Vector3(
                Mathf.MoveToward(Velocity.X, targetH.X, a * (float)delta),
                Velocity.Y,
                Mathf.MoveToward(Velocity.Z, targetH.Z, a * (float)delta)
            );

            int vInput = (Input.IsActionPressed("Saltar") ? 1 : 0) -
                        (Input.IsActionPressed("Crouch") || Input.IsActionPressed("ui_down") ? 1 : 0);

            float targetY = vInput * SwimVerticalSpeed;
            Velocity = new Vector3(
                Velocity.X,
                Mathf.MoveToward(Velocity.Y, targetY, SwimAccel * (float)delta),
                Velocity.Z
            );

            Velocity = new Vector3(
                Velocity.X,
                Velocity.Y + (Buoyancy - WaterGravity) * (float)delta,
                Velocity.Z
            );

            Velocity -= Velocity * Mathf.Clamp(WaterDrag * (float)delta, 0.0f, 0.95f);
            Velocity = new Vector3(
                Velocity.X,
                Mathf.Clamp(Velocity.Y, -MaxVerticalSpeed, MaxVerticalSpeed),
                Velocity.Z
            );

            MoveAndSlide();
        }

        private void OnWaterBodyEntered(Node body)
        {
            if (body == this)
            {
                _waterCounter++;
                _inWater = _waterCounter > 0;
                ApplyUnderwaterFx(true);
            }
        }

        private void OnWaterBodyExited(Node body)
        {
            if (body == this)
            {
                _waterCounter = Math.Max(_waterCounter - 1, 0);
                _inWater = _waterCounter > 0;
                if (!_inWater)
                {
                    ApplyUnderwaterFx(false);
                }
            }
        }

        private void UpdateUnderwaterFlag()
        {
            if (WaterSurface != null && _camera != null)
            {
                bool under = _camera.GlobalTransform.Origin.Y < WaterSurface.GlobalTransform.Origin.Y - SurfaceMargin;
                if (under != _underwater)
                {
                    _underwater = under;
                    ApplyUnderwaterFx(under);
                }
            }
        }

        private void ApplyUnderwaterFx(bool under)
        {
            if (WaterMaterial != null)
            {
                WaterMaterial.SetShaderParameter("is_underwater", under);
            }
            if (WorldEnv != null && EnvDefault != null && EnvUnderwater != null)
            {
                WorldEnv.Environment = under ? EnvUnderwater : EnvDefault;
            }
            if (DisableOccludersWhenUnderwater)
            {
                ToggleOccluders(!under);
            }
        }

        private void ToggleOccluders(bool enabled)
        {
            foreach (var node in GetTree().GetNodesInGroup("occluders"))
            {
                if (node is OccluderInstance3D occluder)
                {
                    occluder.Visible = enabled;
                }
            }
        }
    }
}