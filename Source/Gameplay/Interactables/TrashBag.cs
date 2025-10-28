using Godot;
using PedaleandoGame.Core;
using PedaleandoGame.Managers;

namespace PedaleandoGame.Gameplay.Interactables
{
    /// <summary>
    /// Represents an interactable trash bag in the game world that can be collected by the player
    /// Follows Single Responsibility Principle by handling only trash bag behavior
    /// </summary>
    public partial class TrashBag : StaticBody3D, IInteractable
    {
        private readonly float DEFAULT_OUTLINE_WIDTH = 0.05f;
        
        [Export] private TrashBagVisuals _visuals;
        [Export] private TrashBagProperties _properties;
        [Export] private AudioStreamPlayer3D _pickupSound;
        
        private bool _isSelected;
        
        public override void _Ready()
        {
            InitializeInteractable();
            ConnectToPlayerSignals();
            SetInitialVisualState();
        }

        public override void _Process(double delta)
        {
            UpdateVisualState();
            UpdatePromptVisibility();
        }

        public void Interact(Node player)
        {
            PlayPickupSound();
            AddTrashToGameManager();
            QueueFree();
        }

        #region Private Methods
        
        private void InitializeInteractable()
        {
            AddToGroup("Interactable");
        }

        private void ConnectToPlayerSignals()
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null && player.HasSignal("interact_object"))
            {
                player.Connect("interact_object", Callable.From<Node>(SetSelected));
            }
        }

        private void SetInitialVisualState()
        {
            if (_visuals?.OutlineMesh != null)
                _visuals.OutlineMesh.Visible = false;
            if (_visuals?.PromptLabel != null)
                _visuals.PromptLabel.Visible = false;
        }

        private void UpdateVisualState()
        {
            if (_visuals?.OutlineMesh != null)
                _visuals.OutlineMesh.Visible = _isSelected;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (this != null)
            {
                Position = new Vector3(
                    Position.X,
                    _isSelected ? DEFAULT_OUTLINE_WIDTH : 0.0f,
                    Position.Z
                );
            }
        }

        private void UpdatePromptVisibility()
        {
            if (_visuals?.PromptLabel == null) return;

            var camera = GetViewport().GetCamera3D();
            if (camera == null)
            {
                _visuals.PromptLabel.Visible = false;
                return;
            }

            var distance = GlobalPosition.DistanceTo(camera.GlobalPosition);
            var isInRange = distance >= _properties.MinShowDistance && 
                           distance <= _properties.MaxShowDistance;
            
            _visuals.PromptLabel.Visible = _isSelected && isInRange;

            if (_visuals.PromptLabel.Visible)
            {
                UpdatePromptPosition();
            }
        }

        private void UpdatePromptPosition()
        {
            var basePosition = GlobalTransform.Origin;
            _visuals.PromptLabel.GlobalTransform = new Transform3D(
                _visuals.PromptLabel.GlobalTransform.Basis,
                basePosition + new Vector3(0.0f, _properties.PromptOffsetY, 0.0f)
            );
        }

        private void SetSelected(Node selectedObject)
        {
            _isSelected = (this == selectedObject);
        }

        private void PlayPickupSound()
        {
            _pickupSound?.Play();
        }

        private void AddTrashToGameManager()
        {
            if (!Engine.IsEditorHint())
            {
                var gameManager = GetNode<IGameManager>("/root/GameManager");
                gameManager?.AddTrash(_properties.PickupAmount);
            }
        }
        
        #endregion
    }
}