using Godot;

namespace PedaleandoGame.UI.Menus
{
    public partial class MainMenu : Control
    {
        [Export] private AudioStreamPlayer AudioPlayer { get; set; }

        public override void _Ready()
        {
            // Initialize any audio or other resources here
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
            var dialog = GetNode<Control>("ExitDialog");
            if (dialog != null)
            {
                dialog.Visible = true;
            }
        }
    }
}