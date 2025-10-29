using Godot;
using PedaleandoGame.Managers;
using PedaleandoGame.Entities.Player; // For C# event subscription to Player signal

namespace PedaleandoGame.Entities.Props
{
	public partial class OldTyresStack :  Trash
	{

		[Export] public override float PromptOffsetY { get; set; } = 1.5f;
		[Export] public override float PromptMinShowDist { get; set; } = 0.5f;
		[Export] public override float PromptMaxShowDist { get; set; } = 6.0f;

		public override void _Ready()
		{
			AddToGroup("Interactable");

			_bodyName = this;
			_outlineMesh = GetNode<MeshInstance3D>("old_tyre_005/MeshInstance3D");
			_promptLabel = GetNode<Label3D>("PromptLabel");

			SetInitialVisualState();
		}
	}
}
