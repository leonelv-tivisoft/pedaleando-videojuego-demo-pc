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
			{
				// Localizar el texto del botón
				var loc = PedaleandoGame.Core.Localization.LocalizationManager.Instance;
				if (loc != null)
				{
					_skipButton.Text = loc.GetText("UI_CONTINUE");
					// Actualizar si cambia el idioma mientras está en esta escena
					loc.LanguageChanged += OnLanguageChanged;
				}
				_skipButton.Hide(); // asegurarse de que no aparezca al inicio
			}
		}

		private void OnLanguageChanged(string lang)
		{
			if (_skipButton == null) return;
			var loc = PedaleandoGame.Core.Localization.LocalizationManager.Instance;
			_skipButton.Text = loc != null ? loc.GetText("UI_CONTINUE") : _skipButton.Text;
		}

		private void OnSkipTimerTimeout()
		{
			_skipButton?.Show(); // mostrar el botón después del tiempo
		}

		private void GoToMenu()
		{
			// Ir al menú principal en C#
			GetTree().ChangeSceneToFile("res://Source/UI/Menus/MainMenu.tscn");
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
