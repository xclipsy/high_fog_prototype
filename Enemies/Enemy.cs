using Microsoft.Xna.Framework;

namespace HighFog;

public abstract class Enemy
{
    public Vector3 Position { get; set; }
    public float Facing { get; set; }
    public float Health { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Radius { get; set; } = 0.55f;
    public EnemyState State { get; set; } = EnemyState.Idle;
    public bool IsDead => State == EnemyState.Dead;
    public float StateTimer { get; set; }
    public float AttackCooldown { get; set; }
    public float StaggerTimer { get; set; }
    public float AnimationTime { get; set; }

    public abstract void Update(float dt, Player player, World world, HighFogGame game);
    public abstract void TakeDamage(float amount, Vector3 hitDirection, HighFogGame game);
}
