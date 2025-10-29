using Godot;
using PedaleandoGame.Managers;

namespace PedaleandoGame.Entities.Props
{
    public abstract partial class Trash : StaticBody3D
    {   
        protected StaticBody3D _bodyName;
        protected MeshInstance3D _outlineMesh;
        protected Label3D _promptLabel;
        protected const float OutlineWidth = 0.05f;
        protected bool _selected;
        [Export] public int PickupAmount { get; set; } = 1;
        [Export] public abstract float PromptOffsetY { get; set; }
		[Export] public abstract float PromptMinShowDist { get; set; }
		[Export] public abstract float PromptMaxShowDist { get; set; }

        [Export] protected AudioStreamPlayer3D PickupSound { get; set; }

        public void SetSelect(Node obj)
        {
            _selected = (this == obj);
        }

        public void interact(Node player)
        {
            Interact(player);
        }

        public void Interact(Node player)
        {
            if (PickupSound != null)
            {
                PickupSound.Play();
            }

            // Obtener el GameManager a través del autoload
            var gameManager = GetNode<GameManager>("/root/GameManager");
            GD.Print($"coge accion la llanta?");
            if (gameManager != null)
            {
                GD.Print($"game manager not null");
                gameManager.AddTrash(PickupAmount);
            }

            QueueFree();
        }

        protected void UpdatePrompt()
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
        public override void _ExitTree()
        {
            // Desuscribir para evitar fugas si se instancia y elimina frecuentemente
            var playerNode = GetTree().GetFirstNodeInGroup("Player");
            if (playerNode is PedaleandoGame.Entities.Player.Player player)
            {
                player.InteractObject -= SetSelect;
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
            if (_bodyName != null)
            {
                _bodyName.Position = new Vector3(
                    _bodyName.Position.X,
                    _selected ? OutlineWidth : 0.0f,
                    _bodyName.Position.Z
                );
            }

            UpdatePrompt();
        }
        protected void SetInitialVisualState()
        {
            // Estado visual inicial
            if (_outlineMesh != null)
            {
                _outlineMesh.Visible = false;
            }

            if (_promptLabel != null)
            {
                _promptLabel.Visible = false;
            }

            // Conectar selección desde Player usando el evento C# del signal (evita problemas de nombre/args)
            var playerNode = GetTree().GetFirstNodeInGroup("Player");
            if (playerNode is PedaleandoGame.Entities.Player.Player player)
            {
                player.InteractObject += SetSelect;
            }
        }

    }
}
