using Godot;
using System;
using PedaleandoGame.Core.Localization;

namespace PedaleandoGame.UI.Menus
{
    public partial class LanguageMenu : Control
    {
        private Button SpanishButton;
        private Button EnglishButton;
        [Export] private string MainMenuScene { get; set; } = "res://Source/UI/Menus/MainMenu.tscn";

        public override void _Ready()
        {
            SpanishButton = GetNode<Button>("%SpanishButton");
            EnglishButton = GetNode<Button>("%EnglishButton");

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