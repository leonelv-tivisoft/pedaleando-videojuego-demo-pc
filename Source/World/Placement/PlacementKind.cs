using System;

namespace PedaleandoGame.World.Placement
{
    /// <summary>
    /// Tipos de ubicación válida para objetos "Trash".
    /// Ground = Suelo/Arena; OnRock = Sobre rocas; WaterSurface = Superficie del agua; Underwater = Bajo el agua.
    /// Permite flags para habilitar múltiples tipos.
    /// </summary>
    [Flags]
    public enum PlacementKind
    {
        Ground = 1 << 0,        // Suelo / Arena
        OnRock = 1 << 1,        // Sobre rocas
        WaterSurface = 1 << 2,  // Superficie del agua (flotando)
        Underwater = 1 << 3     // Debajo del agua
    }
}
