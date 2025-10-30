using System;
using Godot;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Configuración por tipo de basura a colocar.
    /// Permite fijar cantidad exacta (FixedCount) o peso (Weight) para reparto ponderado.
    /// </summary>
    [GlobalClass]
    public partial class TrashTypeConfig : Resource
    {
        [Export(PropertyHint.File, "*.tscn,*.scn")] public string ScenePath { get; set; } = string.Empty;
        [Export] public PlacementKind AllowedKinds { get; set; } = PlacementKind.Ground | PlacementKind.OnRock | PlacementKind.WaterSurface | PlacementKind.Underwater;
        [Export(PropertyHint.Range, "0,3,0.05")] public float ClearanceRadius { get; set; } = 0.25f; // Holgura local
        [Export(PropertyHint.Range, "0,50,0.1")] public float MinSeparation { get; set; } = 2.0f;   // Distancia mínima entre instancias de este tipo
        [Export] public int FixedCount { get; set; } = 0; // Si > 0, genera exactamente esta cantidad
        [Export(PropertyHint.Range, "0,100,0.1")] public float Weight { get; set; } = 1.0f; // Peso para reparto aleatorio
    }
}
