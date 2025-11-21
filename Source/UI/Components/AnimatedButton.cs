using Godot;

namespace PedaleandoGame.UI.Components
{
	public partial class AnimatedButton : Button
	{
		[Export] public Vector2 HoverScale { get; set; } = new Vector2(1.1f, 1.1f);
		[Export] public Vector2 PressedScale { get; set; } = new Vector2(0.9f, 0.9f);

		public override void _Ready()
		{
			MouseEntered += OnButtonEnter;
			MouseExited += OnButtonExit;
			ButtonDown += OnButtonPressed;
			
			CallDeferred(nameof(InitPivot));
		}

		private void InitPivot()
		{
			PivotOffset = Size / 2.0f;
		}

		private void OnButtonEnter()
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "scale", HoverScale, 0.1f)
				.SetTrans(Tween.TransitionType.Sine);
		}

		private void OnButtonExit()
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "scale", Vector2.One, 0.1f)
				.SetTrans(Tween.TransitionType.Sine);
		}

		private void OnButtonPressed()
		{
			var tween = CreateTween();
			tween.TweenProperty(this, "scale", PressedScale, 0.06f)
				.SetTrans(Tween.TransitionType.Sine);
			tween.TweenProperty(this, "scale", HoverScale, 0.12f)
				.SetTrans(Tween.TransitionType.Sine);
		}
	}
}
