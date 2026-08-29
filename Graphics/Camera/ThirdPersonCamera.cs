using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Third-person camera supporting orbital exploration, obstacle clipping avoidance,
/// and smooth transition to over-the-shoulder combat aiming with configurable controls.
/// </summary>
public sealed class ThirdPersonCamera
{
    public float Yaw { get; set; } = 0.25f;
    public float Pitch { get; set; } = -0.16f;
    public float NormalDistance { get; set; } = 5.2f;
    public float AimDistance { get; set; } = 2.6f;

    public Vector3 Position { get; private set; }
    public Vector3 Forward { get; private set; } = Vector3.Forward;
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private float _currentDistance = 5.2f;
    private Vector3 _currentShoulderOffset = Vector3.Zero;

    public Vector3 FlatForward
    {
        get
        {
            var result = new Vector3(MathF.Sin(Yaw), 0f, -MathF.Cos(Yaw));
            return Vector3.Normalize(result);
        }
    }

    public void ProcessMouseInput(int deltaX, int deltaY, GameConfig config)
    {
        Yaw += deltaX * config.MouseSensitivity;
        float yFactor = config.InvertY ? -1f : 1f;
        Pitch -= deltaY * config.MouseSensitivity * yFactor;
        Pitch = MathHelper.Clamp(Pitch, -0.58f, 0.48f);
    }

    public void Update(Player player, World world, float aspectRatio, float dt, GameConfig config)
    {
        Pitch = MathHelper.Clamp(Pitch, -0.58f, 0.48f);

        Forward = Vector3.Normalize(new Vector3(
            MathF.Sin(Yaw) * MathF.Cos(Pitch),
            MathF.Sin(Pitch),
            -MathF.Cos(Yaw) * MathF.Cos(Pitch)));

        Vector3 right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.Up));

        // Smooth transition to over-the-shoulder offset when aiming
        Vector3 targetShoulderOffset = player.IsAiming ? (right * 0.62f + Vector3.Up * 0.18f) : Vector3.Zero;
        _currentShoulderOffset = Vector3.Lerp(_currentShoulderOffset, targetShoulderOffset, Math.Clamp(dt * 14f, 0f, 1f));

        Vector3 target = player.Position + new Vector3(0f, 1.25f, 0f) + _currentShoulderOffset;

        float targetDist = player.IsAiming ? AimDistance : NormalDistance;
        _currentDistance = MathHelper.Lerp(_currentDistance, targetDist, Math.Clamp(dt * 12f, 0f, 1f));

        float usableDistance = world.GetCameraDistance(target, Forward, _currentDistance);

        Position = target - Forward * usableDistance;
        View = Matrix.CreateLookAt(Position, Position + Forward, Vector3.Up);
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), aspectRatio, 0.08f, 90f);
    }

    public void Reset()
    {
        Yaw = 0f;
        Pitch = -0.15f;
        _currentDistance = NormalDistance;
        _currentShoulderOffset = Vector3.Zero;
    }
}
