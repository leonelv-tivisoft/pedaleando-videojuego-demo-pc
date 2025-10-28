using Godot;
using System;
using System.Collections.Generic;
using PedaleandoGame.Core.Dialogue;

namespace PedaleandoGame.UI.Dialogue
{
    public partial class DialogueBox : Control
    {
        [Signal]
        public delegate void DialogueFinishedEventHandler();

        private Label _textLabel;
        private Label _nameLabel;
        private Queue<DialogueLine> _dialogueLines;
        private Action _onComplete;

        public override void _Ready()
        {
            _textLabel = GetNode<Label>("TextLabel");
            _nameLabel = GetNode<Label>("NameLabel");
        }

        public void StartDialogue(IEnumerable<DialogueLine> lines, Action onComplete = null)
        {
            _dialogueLines = new Queue<DialogueLine>(lines);
            _onComplete = onComplete;
            ShowNextLine();
        }

        public void ShowNextLine()
        {
            if (_dialogueLines.Count > 0)
            {
                var line = _dialogueLines.Dequeue();
                _nameLabel.Text = line.Name;
                _textLabel.Text = line.Text;
            }
            else
            {
                _onComplete?.Invoke();
                EmitSignal(SignalName.DialogueFinished);
            }
        }
    }

    // Using shared DialogueLine from Core.Dialogue
}