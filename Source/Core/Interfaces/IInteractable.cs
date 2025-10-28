using Godot;

namespace PedaleandoGame.Core.Interfaces
{
	/// <summary>
	/// Defines the contract for interactable objects in the game
	/// Following Interface Segregation Principle
	/// </summary>
	public interface IInteractable
	{
		void Interact(Node interactor);
	}
}
