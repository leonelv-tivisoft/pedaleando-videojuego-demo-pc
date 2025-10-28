using Godot;
using PedaleandoGame.Managers;

namespace PedaleandoGame.Entities.Props
{
    public partial class OldTyresStack : StaticBody3D
    {
        private StaticBody3D _llantaVieja;
        private MeshInstance3D _outlineMesh;
        private Label3D _promptLabel;
        private bool _selected;
        private const float OutlineWidth = 0.05f;

        [Export] public float PromptOffsetY { get; set; } = 1.5f;
        [Export] public float PromptMinShowDist { get; set; } = 0.5f;
        [Export] public float PromptMaxShowDist { get; set; } = 6.0f;
        [Export] public int PickupAmount { get; set; } = 1;
        [Export] public AudioStreamPlayer3D PickupSfx { get; set; }

        public override void _Ready()
        {
            AddToGroup("Interactable");

            _llantaVieja = this;
            _outlineMesh = GetNode<MeshInstance3D>("old_tyre_005/MeshInstance3D");
            _promptLabel = GetNode<Label3D>("PromptLabel");

            // Estado visual inicial
            if (_outlineMesh != null)
            {
                _outlineMesh.Visible = false;
            }

            if (_promptLabel != null)
            {
                _promptLabel.Visible = false;
            }

            // Conectar selección desde Player si usa señales
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null && player.HasSignal("InteractObject"))
            {
                player.Connect("InteractObject", new Callable(this, nameof(SetSelect)));
            }
        }

        public override void _Process(double delta)
        {
            // Outline según selección
            if (_outlineMesh != null)
            {
                _outlineMesh.Visible = _selected;
            }

            // Elevación si está seleccionada
            if (_llantaVieja != null)
            {
                _llantaVieja.Position = new Vector3(
                    _llantaVieja.Position.X,
                    _selected ? OutlineWidth : 0.0f,
                    _llantaVieja.Position.Z
                );
            }

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            if (_promptLabel == null)
            {
                return;
            }

            var camera = GetViewport().GetCamera3D();
            if (camera == null)
            {
                _promptLabel.Visible = false;
                return;
            }

            var dist = GlobalPosition.DistanceTo(camera.GlobalPosition);
            var show = _selected && dist >= PromptMinShowDist && dist <= PromptMaxShowDist;
            _promptLabel.Visible = show;

            if (show)
            {
                var basePos = GlobalTransform.Origin;
                _promptLabel.GlobalTransform = new Transform3D(
                    _promptLabel.GlobalTransform.Basis,
                    basePos + new Vector3(0.0f, PromptOffsetY, 0.0f)
                );
            }
        }

        private void SetSelect(Node obj)
        {
            _selected = (this == obj);
        }

        public void Interact(Node player)
        {
            if (PickupSfx != null)
            {
                PickupSfx.Play();
            }

            // Obtener el GameManager a través del autoload
            var gameManager = GetNode<IGameManager>("/root/GameManager");
            if (gameManager != null)
            {
                gameManager.AddTrash(PickupAmount);
            }

            QueueFree();
        }
    }
}