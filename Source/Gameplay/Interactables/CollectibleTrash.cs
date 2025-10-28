using Godot;
using PedaleandoGame.Core.Interfaces;
using PedaleandoGame.Managers;

namespace PedaleandoGame.Gameplay.Interactables
{
	/// <summary>
	/// Base class for all collectible trash items
	/// Provides common functionality for trash collection
	/// </summary>
	public abstract partial class CollectibleTrash : StaticBody3D, IInteractable
	{
		[Export] protected int PickupAmount { get; set; } = 1;
		[Export] protected AudioStreamPlayer3D PickupSound { get; set; }
		[Export] protected GameManager GameManager { get; set; }

		public override void _Ready()
		{
			base._Ready();
			if (GameManager == null)
			{
				GD.PushWarning($"GameManager not set on {Name}. Please set it in the Inspector.");
			}
		}

		public virtual void Interact(Node player)
		{
			PlayPickupSound();
			AddTrashToGameManager();
			QueueFree();
		}

		protected virtual void PlayPickupSound()
		{
			PickupSound?.Play();
		}

		protected virtual void AddTrashToGameManager()
		{
			if (!Engine.IsEditorHint())
			{
				GameManager?.AddTrash(PickupAmount);
			}
		}
	}
}
