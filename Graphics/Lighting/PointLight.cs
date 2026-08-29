using Microsoft.Xna.Framework;

namespace HighFog;

public sealed class PointLight
{
    public Vector3 Position { get; set; }
    public Color Color { get; set; }
    public float Radius { get; set; }
    public float Intensity { get; set; } = 1.0f;
    public bool Flicker { get; set; }

    public PointLight(Vector3 position, Color color, float radius, bool flicker = false)
    {
        Position = position;
        Color = color;
        Radius = radius;
        Flicker = flicker;
    }

    public float GetCurrentIntensity(float time)
    {
        if (!Flicker) return Intensity;
        float noise = MathF.Sin(time * 11f) * 0.15f + MathF.Sin(time * 23f) * 0.1f + MathF.Sin(time * 37f) * 0.05f;
        return Math.Clamp(Intensity + noise, 0.4f, 1.3f);
    }
}
