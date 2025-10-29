using Godot;
using PedaleandoGame.Managers;

namespace PedaleandoGame.UI
{
	public partial class Counter : Label
	{
		private GameManager _gameManager;

		public override void _EnterTree()
		{
			_gameManager = GetNode<GameManager>("/root/GameManager");
		}
		public override void _Ready()
		{
			if (_gameManager == null)
			{
				GD.PushWarning($"GameManager not set on {Name}. Please set it in the Inspector.");
				return;
			}

			UpdateText(_gameManager.CurrentCount, _gameManager.CurrentGoal);
			_gameManager.CountChanged += OnCountChanged;
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			if (_gameManager != null)
			{
				_gameManager.CountChanged -= OnCountChanged;
			}
		}

		private void UpdateText(int current, int goal)
		{
			Text = $"{current}/{goal}";
		}

		private void OnCountChanged(int current, int goal)
		{
			UpdateText(current, goal);
		}
	}
}
