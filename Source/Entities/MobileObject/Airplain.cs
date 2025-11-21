using Godot;

namespace PedaleandoGame.Entities.MobileObject
{
	/// <summary>
	/// Airplane that follows a Path3D and applies banking/wobble to its visual child.
	/// Depends on PathFollower for traversal; this class focuses on visuals (Open/Closed Principle).
	/// </summary>
	public partial class Airplain : PathFollower
	{
		// Override speed property - ACTUAL VALUE IS SET IN _Ready() METHOD BELOW
		[Export] public override float Speed { get; set; }
		
		// Visual child to animate (the airplane mesh/model)
		[Export] public NodePath PlanePath { get; set; } = new NodePath("Plane");

		// Banking and wobble configuration
		[Export(PropertyHint.Range, "0,45,0.1")] public float BankDeg { get; set; } = 18.0f; // max banking degrees
		[Export] public float WobbleAmp { get; set; } = 1.5f; // aesthetic wobble amplitude
		[Export] public float WobbleFreq { get; set; } = 0.35f; // Hz

		private Node3D _plane;
		private Vector3 _prevForward = Vector3.Forward;
		private double _time;

		public override void _Ready()
		{
			base._Ready();
			_plane = GetNodeOrNull<Node3D>(PlanePath);
			_prevForward = -GlobalTransform.Basis.Z; // current forward
			
			// AIRPLANE SPEED CONFIGURATION - Modify this value to change airplane speed
			// This overrides any inspector values to ensure consistent behavior
			Speed = 15.0f;
		}

		protected override void OnAfterMove(double delta)
		{
			_time += delta;

			// Current forward based on follow orientation
			var fwd = -GlobalTransform.Basis.Z;
			// Positive turn.y indicates left turn; negative for right
			float turn = _prevForward.Cross(fwd).Y;

			// Map turn rate to bank angle
			float targetBank = Mathf.Clamp(turn * BankDeg * 10.0f, -BankDeg, BankDeg);

			// Add aesthetic wobble
			targetBank += Mathf.Sin((float)(_time * Mathf.Tau * WobbleFreq)) * WobbleAmp;

			// Apply bank only to the visual, not to the PathFollow itself
			if (_plane != null)
			{
				var current = _plane.RotationDegrees;
				current.Z = Mathf.Lerp(current.Z, -targetBank, 5.0f * (float)delta);
				_plane.RotationDegrees = current;
			}

			_prevForward = fwd;
		}
	}
}
