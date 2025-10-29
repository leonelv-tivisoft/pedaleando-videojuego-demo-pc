using Godot;
using PedaleandoGame.Core.Interfaces;
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
		
		[Export] public TrashBagVisuals Visuals { get; set; }
		[Export] public TrashBagProperties Properties { get; set; }
		[Export] public AudioStreamPlayer3D PickupSound { get; set; }
		
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
			if (Visuals?.OutlineMesh != null)
				Visuals.OutlineMesh.Visible = false;
			if (Visuals?.PromptLabel != null)
				Visuals.PromptLabel.Visible = false;
		}

		private void UpdateVisualState()
		{
			if (Visuals?.OutlineMesh != null)
				Visuals.OutlineMesh.Visible = _isSelected;

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
			if (Visuals?.PromptLabel == null) return;

			var camera = GetViewport().GetCamera3D();
			if (camera == null)
			{
				Visuals.PromptLabel.Visible = false;
				return;
			}

			var distance = GlobalPosition.DistanceTo(camera.GlobalPosition);
			var isInRange = distance >= Properties.MinShowDistance && 
						   distance <= Properties.MaxShowDistance;
			
			Visuals.PromptLabel.Visible = _isSelected && isInRange;

			if (Visuals.PromptLabel.Visible)
			{
				UpdatePromptPosition();
			}
		}

		private void UpdatePromptPosition()
		{
			var basePosition = GlobalTransform.Origin;
			Visuals.PromptLabel.GlobalTransform = new Transform3D(
				Visuals.PromptLabel.GlobalTransform.Basis,
				basePosition + new Vector3(0.0f, Properties.PromptOffsetY, 0.0f)
			);
		}

		private void SetSelected(Node selectedObject)
		{
			_isSelected = (this == selectedObject);
		}

		private void PlayPickupSound()
		{
			PickupSound?.Play();
		}

		private void AddTrashToGameManager()
		{
			if (!Engine.IsEditorHint())
			{
				var gameManager = GetNode<GameManager>("/root/GameManager");
				gameManager?.AddTrash(Properties.PickupAmount);
			}
		}
		
		#endregion
	}
}
