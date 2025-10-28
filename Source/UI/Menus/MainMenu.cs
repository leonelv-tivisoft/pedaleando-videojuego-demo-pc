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
            GetTree().ChangeSceneToFile("res://Source/Levels/Level1/Level1Demo.tscn");
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