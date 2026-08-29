using Microsoft.Xna.Framework;

namespace HighFog;

public sealed class Player
{
    public Vector3 Position { get; set; }
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Facing { get; set; }
    public float Radius => 0.48f;
    public bool HasHandgun { get; set; }
    public Handgun Handgun { get; } = new();
    public bool IsAiming { get; set; }
    public bool IsRunning { get; set; }
    public float AnimationTimer { get; set; }
    public float FootstepTimer { get; set; }

    public Player(Vector3 position)
    {
        Position = position;
    }

    public void Damage(float amount)
    {
        Health = MathF.Max(0f, Health - amount);
    }

    public void Reset(Vector3 position)
    {
        Position = position;
        Health = 100f;
        HasHandgun = false;
        Handgun.Ammo = 0;
        Handgun.ReserveAmmo = 0;
        IsAiming = false;
        IsRunning = false;
        Facing = 0f;
    }
}
