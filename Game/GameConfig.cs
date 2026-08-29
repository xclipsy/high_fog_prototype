namespace HighFog;

/// <summary>
/// Configurable movement, camera, audio, and visual settings.
/// </summary>
public sealed class GameConfig
{
    public float MouseSensitivity { get; set; } = 0.0035f;
    public bool InvertY { get; set; } = false;
    public bool ToggleAim { get; set; } = false;
    public bool ToggleRun { get; set; } = false;
    public float MovementSpeedMultiplier { get; set; } = 1.0f;
    public float FogDistanceScale { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.8f;

    public static GameConfig Default { get; } = new();
}
