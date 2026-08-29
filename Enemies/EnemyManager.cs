using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Manages enemy life-cycles, combat hit detection, and spatial positioning.
/// </summary>
public sealed class EnemyManager
{
    private readonly List<Enemy> _enemies = new();
    public IReadOnlyList<Enemy> Enemies => _enemies;

    public void InitializeTownEnemies()
    {
        _enemies.Clear();
        // 1. Suspense silhouette Walker in the foggy northern road (Section 47)
        _enemies.Add(new FogWalker(new Vector3(6f, 0f, -28f), isSilhouetteOnly: true));

        // 2. Suspense Walker near old abandoned house to the west
        _enemies.Add(new FogWalker(new Vector3(-25f, 0f, -12f), isSilhouetteOnly: false));
    }

    public void TriggerBasementAmbush()
    {
        // Spawns the threatening Fog Walker in the basement corridor upon finding the gun (Section 48 & 49)
        var basementWalker = new FogWalker(new Vector3(45f, 0f, 0f))
        {
            Facing = -MathF.PI * 0.5f,
            State = EnemyState.Chase
        };
        _enemies.Add(basementWalker);
    }

    public void Update(float dt, Player player, World world, HighFogGame game)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            _enemies[i].Update(dt, player, world, game);
        }
    }

    public Enemy? CheckBulletHit(Vector3 origin, Vector3 direction, float maxRange, out Vector3 hitPoint)
    {
        hitPoint = origin + direction * maxRange;
        Enemy? closestEnemy = null;
        float closestDistance = maxRange;

        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead) continue;

            // Approximate enemy with a vertical capsule / cylinder (radius 0.6m, height 1.8m)
            Vector3 center = enemy.Position + new Vector3(0, 0.9f, 0);
            
            // Ray to sphere / cylinder intersection check
            Vector3 m = origin - center;
            float b = Vector3.Dot(m, direction);
            float c = Vector3.Dot(m, m) - 0.7f * 0.7f;

            if (c > 0f && b > 0f) continue;
            float discr = b * b - c;
            if (discr < 0f) continue;

            float t = -b - MathF.Sqrt(discr);
            if (t < 0f) t = 0f;

            if (t < closestDistance)
            {
                closestDistance = t;
                closestEnemy = enemy;
                hitPoint = origin + direction * t;
            }
        }

        return closestEnemy;
    }
}
