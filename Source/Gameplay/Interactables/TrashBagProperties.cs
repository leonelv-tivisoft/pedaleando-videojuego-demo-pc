using Godot;

namespace PedaleandoGame.Gameplay.Interactables
{
    /// <summary>
    /// Encapsulates configuration properties for trash bag behavior
    /// Following Single Responsibility Principle
    /// </summary>
    public partial class TrashBagProperties : Resource
    {
        [Export] public float PromptOffsetY { get; set; } = 2.3f;
        [Export] public float MinShowDistance { get; set; } = 0.5f;
        [Export] public float MaxShowDistance { get; set; } = 6.0f;
        [Export] public int PickupAmount { get; set; } = 1;
    }
}