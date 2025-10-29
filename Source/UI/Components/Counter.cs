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
		[Export] public NodePath GameManagerPath { get; set; } // optional injection

		private GameManager _gameManager;
		private Node _gameManagerNodeFallback; // if not C#, fallback to GDScript autoload

		public override void _Ready()
		{
			Visible = true;

			// Autowire labels if not set in the Inspector (scene has nodes: "ContadorBasuras" and "Label")
			if (LabelCount == null)
			{
				LabelCount = GetNodeOrNull<Label>("ContadorBasuras");
			}
			if (LabelGoal == null)
			{
				LabelGoal = GetNodeOrNull<Label>("Label");
			}
			// Fallback: pick first Label children if still null
			if (LabelCount == null || (ShowWithGoal && LabelGoal == null))
			{
				foreach (var child in GetChildren())
				{
					if (child is Label lbl)
					{
						if (LabelCount == null)
						{
							LabelCount = lbl;
							continue;
						}
						if (LabelGoal == null)
						{
							LabelGoal = lbl;
							break;
						}
					}
				}
			}

			// Resolve GameManager via injection path or autoload
			if (GameManagerPath != null && !GameManagerPath.IsEmpty)
			{
				var gm = GetNodeOrNull(GameManagerPath);
				_gameManager = gm as GameManager;
				if (_gameManager == null)
				{
					_gameManagerNodeFallback = gm;
				}
			}
			else
			{
				var gm = GetNodeOrNull("/root/GameManager");
				_gameManager = gm as GameManager;
				if (_gameManager == null)
				{
					_gameManagerNodeFallback = gm;
				}
			}

			if (_gameManager != null)
			{
				UpdateText(_gameManager.CurrentCount, _gameManager.CurrentGoal);
				_gameManager.CountChanged += OnCountChanged;
			}
			else if (_gameManagerNodeFallback != null)
			{
				// Attempt to pull initial values if exposed (optional). Otherwise just wait for signal
				UpdateTextFromFallback();
				if (_gameManagerNodeFallback.HasSignal("count_changed"))
				{
					_gameManagerNodeFallback.Connect("count_changed", new Callable(this, nameof(OnCountChangedFromSignal)));
				}
			}
			else
			{
				GD.PushWarning("[Counter] GameManager not found. UI will not update.");
			}
		}

		private void OnCountChanged(int current, int goal)
		{
			UpdateText(current, goal);
		}

		private void OnCountChangedFromSignal(int current, int goal)
		{
			UpdateText(current, goal);
		}

		private void UpdateText(int current, int goal)
		{
			if (LabelCount != null)
			{
				LabelCount.Text = ShowWithGoal ? $"{Prefix}{current:D2}" : $"{Prefix}{current:D2}"; // prefix applied; goal handled separately
			}
			
			if (LabelGoal != null)
			{
				LabelGoal.Visible = ShowWithGoal;
				if (ShowWithGoal)
				{
					LabelGoal.Text = $"/{goal:D2}";
				}
			}
		}

		private void UpdateTextFromFallback()
		{
			// If GDScript GameManager exposed "count" and "goal" variables, read them; otherwise remain as-is
			int current = 0;
			int goal = 0;
			if (_gameManagerNodeFallback != null)
			{
				if (_gameManagerNodeFallback.HasMethod("get") && _gameManagerNodeFallback.HasMeta("deprecated_gd"))
				{
					// optional: unused; but keep structure for future
				}

				// Try property access by name if available
				var countProp = _gameManagerNodeFallback.Get("count");
				var goalProp = _gameManagerNodeFallback.Get("goal");
				if (countProp.VariantType != Variant.Type.Nil)
				{
					current = (int)countProp;
				}
				if (goalProp.VariantType != Variant.Type.Nil)
				{
					goal = (int)goalProp;
				}
			}
			UpdateText(current, goal);
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			// Clean up event subscription
			if (_gameManager != null)
			{
				_gameManager.CountChanged -= OnCountChanged;
			}
			else if (_gameManagerNodeFallback != null && _gameManagerNodeFallback.HasSignal("count_changed"))
			{
				_gameManagerNodeFallback.Disconnect("count_changed", new Callable(this, nameof(OnCountChangedFromSignal)));
			}
		}
	}
}
