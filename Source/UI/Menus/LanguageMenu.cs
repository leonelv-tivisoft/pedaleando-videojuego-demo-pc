using Godot;
using System;
using PedaleandoGame.Core.Localization;

namespace PedaleandoGame.UI.Menus
{
    public partial class LanguageMenu : Control
    {
        [Export] private Button SpanishButton { get; set; }
        [Export] private Button EnglishButton { get; set; }
        [Export] private string MainMenuScene { get; set; } = "res://MainMenu.tscn";

        public override void _Ready()
        {
            if (SpanishButton != null)
                SpanishButton.Pressed += () => OnLanguageSelected("es");
            
            if (EnglishButton != null)
                EnglishButton.Pressed += () => OnLanguageSelected("en");
        }

        private void OnLanguageSelected(string languageCode)
        {
            LocalizationManager.Instance.ChangeLanguage(languageCode);
            GetTree().ChangeSceneToFile(MainMenuScene);
        }
    }
}