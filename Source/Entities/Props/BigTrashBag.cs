using Godot;
// using PedaleandoGame.Entities.Player; // Avoid aliasing; use fully-qualified name to disambiguate

namespace PedaleandoGame.Entities.Props
{
	public partial class BigTrashBag : Trash
	{
		private StaticBody3D _bagBody;

		// Prompt settings (matches legacy GDScript defaults)
		[Export] public override float PromptOffsetY { get; set; } = 2.3f;
		[Export] public override float PromptMinShowDist { get; set; } = 0.5f;
		[Export] public override float PromptMaxShowDist { get; set; } = 6.0f;

		public override void _Ready()
		{
			AddToGroup("Interactable");

			_bodyName = this;
			_outlineMesh = GetNodeOrNull<MeshInstance3D>("Trashbag - Full_001/MeshInstance3D");
			_promptLabel = GetNodeOrNull<Label3D>("PromptLabel");

			SetInitialVisualState();
		}

		
	}
}
