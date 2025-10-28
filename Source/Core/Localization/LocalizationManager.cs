using Godot;
using System.Collections.Generic;

namespace PedaleandoGame.Core.Localization
{
    /// <summary>
    /// Manages the game's localization system
    /// </summary>
    public partial class LocalizationManager : Node
    {
        public static LocalizationManager Instance { get; private set; }

        [Signal]
        public delegate void LanguageChangedEventHandler(string newLanguage);

        private const string DEFAULT_LANGUAGE = "es";
        private const string LANGUAGE_PREF_KEY = "settings/language";

        private Dictionary<string, Dictionary<string, string>> _translations;
        private string _currentLanguage;

        public override void _Ready()
        {
            Instance = this;
            _translations = new Dictionary<string, Dictionary<string, string>>();
            LoadTranslations();
            InitializeLanguage();
        }

        private void LoadTranslations()
        {
            // Load translations from JSON files in Localization folder
            var supportedLanguages = new[] { "es", "en" };
            foreach (var lang in supportedLanguages)
            {
                var file = FileAccess.Open($"res://Localization/{lang}/translations.json", FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    var json = file.GetAsText();
                    var jsonObj = Json.ParseString(json).AsGodotDictionary();
                    _translations[lang] = new Dictionary<string, string>();
                    
                    foreach (var entry in jsonObj)
                    {
                        _translations[lang][entry.Key.ToString()] = entry.Value.ToString();
                    }
                }
            }
        }

        private void InitializeLanguage()
        {
            if (ProjectSettings.HasSetting(LANGUAGE_PREF_KEY))
            {
                _currentLanguage = ProjectSettings.GetSetting(LANGUAGE_PREF_KEY).ToString();
            }
            else
            {
                _currentLanguage = DEFAULT_LANGUAGE;
                ProjectSettings.SetSetting(LANGUAGE_PREF_KEY, _currentLanguage);
                ProjectSettings.Save();
            }
        }

        public void ChangeLanguage(string languageCode)
        {
            if (_translations.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
                ProjectSettings.SetSetting(LANGUAGE_PREF_KEY, languageCode);
                ProjectSettings.Save();
                EmitSignal(SignalName.LanguageChanged, languageCode);
            }
        }

        public string GetText(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var langDict))
            {
                if (langDict.TryGetValue(key, out var text))
                {
                    return text;
                }
            }
            return $"[Missing: {key}]";
        }

        public string CurrentLanguage => _currentLanguage;
    }
}