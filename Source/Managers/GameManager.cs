using Godot;

namespace PedaleandoGame.Managers
{
    /// <summary>
    /// Manages the game state including trash collection and goals
    /// Follows Single Responsibility Principle by focusing on core game state
    /// </summary>
    public partial class GameManager : Node, IGameManager
    {
        private int _count = 0;
        private int _goal = 20;

        // C# event for C# subscribers
        public event System.Action<int, int> CountChanged;

        // Godot signal for GDScript and editor connections (backwards compatible name)
        [Signal] public delegate void count_changedEventHandler(int current, int goal);

        public int CurrentCount => _count;
        public int CurrentGoal => _goal;

        public override void _Ready()
        {
            // Emit initial state in case something is already listening
            CountChanged?.Invoke(_count, _goal);
            EmitSignal("count_changed", _count, _goal);
        }

        /// <summary>
        /// Resets the game state with a new collection goal
        /// </summary>
        /// <param name="newGoal">The new trash collection goal (defaults to 20)</param>
        public void Reset(int newGoal = 20)
        {
            _count = 0;
            _goal = newGoal;
            CountChanged?.Invoke(_count, _goal);
            EmitSignal("count_changed", _count, _goal);
        }

        /// <summary>
        /// Adds the specified amount of trash to the current count
        /// </summary>
        /// <param name="amount">Amount of trash to add (defaults to 1)</param>
        public void AddTrash(int amount = 1)
        {
            _count += amount;
            // Log for debugging/telemetry
            GD.Print($"[GameManager] Trash collected: {_count}/{_goal}");
            CountChanged?.Invoke(_count, _goal);
            EmitSignal("count_changed", _count, _goal);
        }

        // Backwards compatible methods for GDScript callers (snake_case)
        public void add_trash(int amount = 1) => AddTrash(amount);
        public void reset(int new_goal = 20) => Reset(new_goal);
    }
}