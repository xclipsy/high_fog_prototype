using Microsoft.Xna.Framework;

namespace HighFog;

public readonly record struct WorldBounds(Vector3 Center, Vector2 Size)
{
    public bool Overlaps(Vector3 point, float radius)
    {
        var half = Size * 0.5f;
        return point.X + radius > Center.X - half.X && point.X - radius < Center.X + half.X &&
               point.Z + radius > Center.Z - half.Y && point.Z - radius < Center.Z + half.Y;
    }
}
