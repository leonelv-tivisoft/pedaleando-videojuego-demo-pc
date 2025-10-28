using Godot;
using PedaleandoGame.Managers;

namespace PedaleandoGame.UI.Components
{
    public partial class Counter : CanvasLayer
    {
        [Export] public Label LabelCount { get; set; }
        [Export] public Label LabelGoal { get; set; }
        [Export] public string Prefix { get; set; } = "";
        [Export] public bool ShowWithGoal { get; set; } = true;

        private IGameManager _gameManager;

        public override void _Ready()
        {
            Visible = true;
            
            // Get the singleton instance
            _gameManager = GetNode<IGameManager>("/root/GameManager");
            
            // Update initial state
            UpdateText(_gameManager.CurrentCount, _gameManager.CurrentGoal);
            
            // Connect to count changed event
            _gameManager.CountChanged += OnCountChanged;
        }

        private void OnCountChanged(int current, int goal)
        {
            UpdateText(current, goal);
        }

        private void UpdateText(int current, int goal)
        {
            if (LabelCount != null)
            {
                LabelCount.Text = $"{Prefix}{current:D2}";
            }
            
            if (LabelGoal != null)
            {
                LabelGoal.Text = $"/{goal:D2}";
            }
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            // Clean up event subscription
            if (_gameManager != null)
            {
                _gameManager.CountChanged -= OnCountChanged;
            }
        }
    }
}