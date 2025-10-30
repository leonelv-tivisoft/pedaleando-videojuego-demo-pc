using Godot;
using PedaleandoGame.World.Placement;
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

		// Auto-attachment of placement physics so new trash types inherit the falling/float behavior
		[ExportGroup("Placement Physics")]
		[Export] public bool AutoAttachPlacementPhysics { get; set; } = true;
		[Export] public NodePath WaterSurfaceNodePath { get; set; }
		[Export(PropertyHint.Range, "0,1,0.01")] public float ProbabilityFloat { get; set; } = 0.5f;
		[Export(PropertyHint.Range, "0,3,0.01")] public float FloatOffsetMax { get; set; } = 0.6f;
		[Export] public float StartHeightY { get; set; } = 30f;
		[Export(PropertyHint.Layers3DPhysics)] public uint GroundMask { get; set; } = 1; // terrain
		[Export(PropertyHint.Layers3DPhysics)] public uint RockMask { get; set; } = 4;   // rocks

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

			// Elevación visual si está seleccionada: evitar mover el cuerpo si hay física de colocación
			bool hasPlacementPhysics = false;
			foreach (var child in GetChildren())
			{
				if (child is TrashPlacementPhysics) { hasPlacementPhysics = true; break; }
			}
			if (!hasPlacementPhysics && _bodyName != null)
			{
				// Sin física de caída: pequeño offset visual
				_bodyName.Position = new Vector3(
					_bodyName.Position.X,
					_selected ? OutlineWidth : _bodyName.Position.Y,
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

			// Ensure we have the placement physics component once in the hierarchy
			if (AutoAttachPlacementPhysics)
			{
				EnsurePlacementPhysics();
			}
		}

		private void EnsurePlacementPhysics()
		{
			// Avoid duplicates if added by the spawner
			foreach (var child in GetChildren())
			{
				if (child is TrashPlacementPhysics)
					return;
			}

			var comp = new TrashPlacementPhysics
			{
				Name = "TrashPlacementPhysics",
				StartHeightY = StartHeightY,
				WaterSurfaceNodePath = WaterSurfaceNodePath,
				ProbabilityFloat = ProbabilityFloat,
				FloatOffsetMax = FloatOffsetMax,
				GroundMask = GroundMask,
				RockMask = RockMask
			};
			AddChild(comp);
		}

	}
}
