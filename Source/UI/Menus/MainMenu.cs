using Godot;
using PedaleandoGame.Core.Localization;

namespace PedaleandoGame.UI.Menus
{
	public partial class MainMenu : Control
	{
		[Export] private AudioStreamPlayer AudioPlayer { get; set; }
		private Button _btnPlay;
		private Button _btnOptions;
		private Button _btnExit;

		public override void _Ready()
		{
			_btnPlay = GetNodeOrNull<Button>("Menu/JUGAR");
			_btnOptions = GetNodeOrNull<Button>("Menu/HISTORIA");
			_btnExit = GetNodeOrNull<Button>("Menu/SALIR");

			ApplyLocalizedTexts();

			// Update if language changes while menu is open
			var loc = LocalizationManager.Instance;
			if (loc != null)
			{
				loc.LanguageChanged += OnLanguageChanged;
			}

			// Ensure button signals are connected (in case scene connections are missing)
			if (_btnPlay != null)
				_btnPlay.Pressed += OnJugarPressed;
			if (_btnOptions != null)
				_btnOptions.Pressed += OnHistoriaPressed;
			if (_btnExit != null)
				_btnExit.Pressed += OnSalirPressed;
		}

		private void OnLanguageChanged(string newLanguage)
		{
			ApplyLocalizedTexts();
		}

		private void ApplyLocalizedTexts()
		{
			var loc = LocalizationManager.Instance;
			if (loc == null) return;

			if (_btnPlay != null)
				_btnPlay.Text = loc.GetText("MENU_PLAY_DEMO");
			if (_btnOptions != null)
				_btnOptions.Text = loc.GetText("MENU_OPTIONS");
			if (_btnExit != null)
				_btnExit.Text = loc.GetText("MENU_EXIT");
		}

		private void OnJugarPressed()
		{
			GetTree().ChangeSceneToFile("res://Nivel1_Demo.tscn");
		}

		private void OnHistoriaPressed()
		{
			// TODO: Implement historia logic
		}

		private void OnSalirPressed()
		{
			var cd = GetNodeOrNull<ConfirmationDialog>("ExitDialog");
			if (cd != null)
			{
				var loc = LocalizationManager.Instance;
				if (loc != null)
				{
					cd.Title = loc.GetText("EXIT_TITLE");
					cd.DialogText = loc.GetText("EXIT_CONFIRM");
					cd.GetOkButton().Text = loc.GetText("EXIT_OK");
					cd.GetCancelButton().Text = loc.GetText("EXIT_CANCEL");
				}
				cd.Visible = true;
			}
		}
	}
}
