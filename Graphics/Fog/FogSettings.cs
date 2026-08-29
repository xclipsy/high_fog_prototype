using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Configurable distance fog parameters according to Section 7 of the Master Prompt.
/// </summary>
public sealed class FogSettings
{
    public float FogStart { get; set; } = 4.0f;
    public float FogEnd { get; set; } = 22.0f;
    public float FogDensity { get; set; } = 0.08f;
    
    // Cold, desaturated atmospheric fog color
    public Color FogColor { get; set; } = new Color(42, 54, 58);

    public static FogSettings Default => new();
    
    public static FogSettings Basement => new()
    {
        FogStart = 2.0f,
        FogEnd = 14.0f,
        FogColor = new Color(18, 22, 24)
    };
}
