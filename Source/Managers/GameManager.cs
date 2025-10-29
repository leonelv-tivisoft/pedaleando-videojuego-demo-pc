using Godot;

namespace PedaleandoGame.Managers
{
	/// <summary>
	/// Manages the game state including trash collection and goals
	/// Follows Single Responsibility Principle by focusing on core game state
	/// </summary>
	public partial class GameManager : Node
	{
		private int _count = 0;
		private int _goal = 20;

		public event System.Action<int, int> CountChanged;

		public int CurrentCount => _count;
		public int CurrentGoal => _goal;

		public override void _Ready()
		{
			// Emit initial state in case something is already listening
			CountChanged?.Invoke(_count, _goal);
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
		}

		/// <summary>
		/// Adds the specified amount of trash to the current count
		/// </summary>
		/// <param name="amount">Amount of trash to add (defaults to 1)</param>
		public void AddTrash(int amount = 1)
		{
			_count += amount;
			CountChanged?.Invoke(_count, _goal);
		}
	}
}
