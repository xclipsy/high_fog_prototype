using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Configurable distance fog parameters for N64/PS1 horror aesthetic.
/// Fog starts closer to create claustrophobic atmosphere and doesn't extend too far.
/// </summary>
public sealed class FogSettings
{
    public float FogStart { get; set; } = 2.5f;
    public float FogEnd { get; set; } = 12.0f;
    public float FogDensity { get; set; } = 0.12f;
    
    // Dark, cold, desaturated atmospheric fog color for horror aesthetic
    public Color FogColor { get; set; } = new Color(28, 35, 38);

    public static FogSettings Default => new();
    
    public static FogSettings Basement => new()
    {
        FogStart = 1.5f,
        FogEnd = 8.0f,
        FogDensity = 0.15f,
        FogColor = new Color(12, 15, 18)
    };
}
