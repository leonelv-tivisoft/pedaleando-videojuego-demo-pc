using System.Collections.Generic;

namespace PedaleandoGame.Core.Dialogue
{
    /// <summary>
    /// Represents a single line of dialogue with speaker and text
    /// Shared between all dialogue-related components
    /// </summary>
    public class DialogueLine
    {
        public string Name { get; }
        public string Text { get; }

        public DialogueLine(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                { "name", Name },
                { "text", Text }
            };
        }
    }
}