using Godot;

namespace PedaleandoGame.Entities.MobileObject
{
    /// <summary>
    /// Base class for objects that move along a Path3D using a PathFollow3D.
    /// Single responsibility: path traversal and looping behavior.
    /// </summary>
    public abstract partial class PathFollower : PathFollow3D
    {
        [Export] public float Speed { get; set; } = 60.0f; // units per second along the path
        [Export] public bool LoopPath { get; set; } = true; // loop when reaching the end

        private Path3D _path;

        public override void _Ready()
        {
            _path = GetParent() as Path3D;
        }

        public override void _PhysicsProcess(double delta)
        {
            MoveAlongPath((float)delta);
            OnAfterMove(delta);
        }

        /// <summary>
        /// Template hook for subclasses to apply visuals (e.g., banking) after movement.
        /// </summary>
        /// <param name="delta">Physics delta</param>
        protected virtual void OnAfterMove(double delta) { }

        protected void MoveAlongPath(float delta)
        {
            if (_path == null || _path.Curve == null)
                return;

            var length = _path.Curve.GetBakedLength();
            Progress += Speed * delta;

            if (Progress > length)
            {
                if (LoopPath && length > 0.0f)
                {
                    Progress = Mathf.PosMod(Progress, length);
                }
                else
                {
                    Progress = length;
                }
            }
        }
    }
}
