using Godot;

namespace PedaleandoGame.UI.Screens
{
	public partial class SplashScreen : Control
	{
		private Button _skipButton;
		private VideoStreamPlayer _video;
		private Timer _skipTimer;

		public override void _Ready()
		{
			_skipButton = GetNode<Button>("OMITIR");
			_video = GetNodeOrNull<VideoStreamPlayer>("VideoStreamPlayer");
			_skipTimer = GetNodeOrNull<Timer>("SkipTimer");

			if (_skipButton != null)
				_skipButton.Hide(); // asegurarse de que no aparezca al inicio
		}

		private void OnSkipTimerTimeout()
		{
			_skipButton?.Show(); // mostrar el botón después del tiempo
		}

		private void GoToMenu()
		{
			// Main menu está en la raíz del proyecto
			GetTree().ChangeSceneToFile("res://MainMenu.tscn");
		}

		private void OnVideoStreamPlayerFinished()
		{
			GoToMenu();
		}

		private void OnOmitirPressed()
		{
			GoToMenu();
		}

		// Ganchos con nombres snake_case para coincidir con las conexiones del .tscn
		private void _on_skip_timer_timeout() => OnSkipTimerTimeout();
		private void _on_video_stream_player_finished() => OnVideoStreamPlayerFinished();
		private void _on_omitir_pressed() => OnOmitirPressed();
	}
}
