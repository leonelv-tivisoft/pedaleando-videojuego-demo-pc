using Godot;

namespace PedaleandoGame.Gameplay.Interactables
{
	/// <summary>
	/// Encapsulates all visual components of a trash bag
	/// Following Interface Segregation Principle
	/// </summary>
	public partial class TrashBagVisuals : Node
	{
		[Export] public NodePath OutlineMeshPath { get; set; }
		[Export] public NodePath PromptLabelPath { get; set; }

		private MeshInstance3D _outlineMesh;
		private Label3D _promptLabel;

		public MeshInstance3D OutlineMesh => _outlineMesh;
		public Label3D PromptLabel => _promptLabel;

		public override void _Ready()
		{
			if (OutlineMeshPath != null)
				_outlineMesh = GetNode<MeshInstance3D>(OutlineMeshPath);
			
			if (PromptLabelPath != null)
				_promptLabel = GetNode<Label3D>(PromptLabelPath);
		}
	}
}
