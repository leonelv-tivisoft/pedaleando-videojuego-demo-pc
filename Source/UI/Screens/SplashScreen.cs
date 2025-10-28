using Godot;

namespace PedaleandoGame.UI.Screens
{
	public partial class SplashScreen : Control
	{
		private Button _skipButton;

		public override void _Ready()
		{
			_skipButton = GetNode<Button>("OMITIR");
			_skipButton.Hide(); // Asegurarse de que no aparezca al inicio
		}

		private void OnSkipTimerTimeout()
		{
			_skipButton.Show(); // Mostrar el botón después del tiempo
		}

		private void GoToMenu()
		{
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
	}
}
