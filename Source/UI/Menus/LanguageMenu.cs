using Godot;
using System;
using PedaleandoGame.Core.Localization;

namespace PedaleandoGame.UI.Menus
{
    public partial class LanguageMenu : Control
    {
        private Button SpanishButton;
        private Button EnglishButton;
    // Después de seleccionar idioma, queremos mostrar el Splash (video + omitir)
    [Export] private string NextScene { get; set; } = "res://control.tscn";

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
            GetTree().ChangeSceneToFile(NextScene);
        }
    }
}