using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

public struct FogParticle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Color Color;
    public float Size;
    public float Life;
    public float MaxLife;
    public bool Active;
}

/// <summary>
/// 3D particle system rendering floating mist, drifting snow/dust, muzzle sparks, and combat hit splatters.
/// </summary>
public sealed class ParticleSystem
{
    private readonly FogParticle[] _particles = new FogParticle[256];
    private readonly Random _rand = new(1337);

    public void Update(float dt, Vector3 playerPosition)
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            if (!_particles[i].Active)
            {
                // Respawn ambient mist / dust near player
                if (_rand.Next(0, 100) < 5)
                {
                    SpawnAmbientMist(ref _particles[i], playerPosition);
                }
                continue;
            }

            _particles[i].Position += _particles[i].Velocity * dt;
            _particles[i].Life -= dt;

            // Fade size or despawn
            if (_particles[i].Life <= 0f)
            {
                _particles[i].Active = false;
            }
        }
    }

    public void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
    {
        for (int i = 0; i < 12; i++)
        {
            int idx = FindFreeParticle();
            if (idx == -1) break;

            Vector3 spread = new(
                (float)(_rand.NextDouble() - 0.5) * 0.4f,
                (float)(_rand.NextDouble() - 0.5) * 0.4f,
                (float)(_rand.NextDouble() - 0.5) * 0.4f
            );

            _particles[idx] = new FogParticle
            {
                Position = position,
                Velocity = (direction + spread) * (6f + (float)_rand.NextDouble() * 8f),
                Color = (_rand.Next(0, 2) == 0) ? new Color(255, 220, 110) : new Color(255, 130, 40),
                Size = 0.12f + (float)_rand.NextDouble() * 0.08f,
                Life = 0.08f + (float)_rand.NextDouble() * 0.06f,
                MaxLife = 0.14f,
                Active = true
            };
        }
    }

    public void SpawnBloodImpact(Vector3 position, Vector3 normal)
    {
        for (int i = 0; i < 16; i++)
        {
            int idx = FindFreeParticle();
            if (idx == -1) break;

            Vector3 spread = new(
                (float)(_rand.NextDouble() - 0.5) * 0.8f,
                (float)(_rand.NextDouble() * 0.5f) + 0.2f,
                (float)(_rand.NextDouble() - 0.5) * 0.8f
            );

            _particles[idx] = new FogParticle
            {
                Position = position,
                Velocity = (normal + spread) * (1.5f + (float)_rand.NextDouble() * 3.5f),
                Color = new Color(130 + _rand.Next(-20, 21), 15, 15, 240),
                Size = 0.1f + (float)_rand.NextDouble() * 0.08f,
                Life = 0.35f + (float)_rand.NextDouble() * 0.25f,
                MaxLife = 0.6f,
                Active = true
            };
        }
    }

    private void SpawnAmbientMist(ref FogParticle p, Vector3 center)
    {
        float angle = (float)_rand.NextDouble() * MathF.Tau;
        float dist = 4f + (float)_rand.NextDouble() * 18f;
        float height = 0.2f + (float)_rand.NextDouble() * 3.2f;

        p.Position = new Vector3(
            center.X + MathF.Cos(angle) * dist,
            height,
            center.Z + MathF.Sin(angle) * dist
        );

        // Slow drifting wind velocity
        p.Velocity = new Vector3(
            -0.35f + (float)(_rand.NextDouble() - 0.5) * 0.2f,
            (float)(_rand.NextDouble() - 0.5) * 0.08f,
            0.15f + (float)(_rand.NextDouble() - 0.5) * 0.2f
        );

        int alpha = 40 + _rand.Next(0, 50);
        p.Color = new Color(175, 195, 205, alpha);
        p.Size = 0.25f + (float)_rand.NextDouble() * 0.35f;
        p.Life = 4f + (float)_rand.NextDouble() * 6f;
        p.MaxLife = p.Life;
        p.Active = true;
    }

    private int FindFreeParticle()
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            if (!_particles[i].Active) return i;
        }
        return -1;
    }

    public void Draw(PrimitiveRenderer renderer)
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            if (!_particles[i].Active) continue;
            
            float alpha = Math.Clamp(_particles[i].Life / _particles[i].MaxLife, 0f, 1f);
            var col = new Color(_particles[i].Color, (int)(_particles[i].Color.A * alpha));
            
            renderer.Box(_particles[i].Position, new Vector3(_particles[i].Size), col);
        }
    }
}
