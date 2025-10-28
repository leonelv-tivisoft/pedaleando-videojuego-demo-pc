using Godot;

namespace PedaleandoGame.Gameplay.Interactables
{
    /// <summary>
    /// Encapsulates all visual components of a trash bag
    /// Following Interface Segregation Principle
    /// </summary>
    public partial class TrashBagVisuals : Resource
    {
        [Export] public MeshInstance3D OutlineMesh { get; set; }
        [Export] public Label3D PromptLabel { get; set; }
    }
}