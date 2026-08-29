using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Fog Walker: Distorted humanoid enemy born from the anomalous subterranean phenomenon.
/// Implements N64 survival horror behavior: stealth stalking, fog silhouetting, shambling chase, stagger, and claw swipe attack.
/// </summary>
public sealed class FogWalker : Enemy
{
    private readonly Vector3 _homePosition;
    private float _soundTimer;
    public bool IsSilhouetteOnly { get; set; }

    public FogWalker(Vector3 position, bool isSilhouetteOnly = false)
    {
        Position = position;
        _homePosition = position;
        IsSilhouetteOnly = isSilhouetteOnly;
        Health = 100f;
        MaxHealth = 100f;
        State = isSilhouetteOnly ? EnemyState.Suspense : EnemyState.Idle;
    }

    public override void Update(float dt, Player player, World world, HighFogGame game)
    {
        AnimationTime += dt * 3.5f;
        _soundTimer -= dt;
        AttackCooldown = MathF.Max(0f, AttackCooldown - dt);

        if (State == EnemyState.Dead)
        {
            return;
        }

        if (State == EnemyState.Stagger)
        {
            StaggerTimer -= dt;
            if (StaggerTimer <= 0f)
            {
                State = EnemyState.Chase;
            }
            return;
        }

        float distToPlayer = Vector3.Distance(Position, player.Position);

        // Ambient growl timer when near player
        if (distToPlayer < 14f && _soundTimer <= 0f && State != EnemyState.Suspense)
        {
            game.Audio.PlayCue("monster_growl");
            _soundTimer = 5f + (float)Random.Shared.NextDouble() * 4f;
        }

        // Suspense Encounter Mode (Section 47: First distant sighting that fades away)
        if (IsSilhouetteOnly)
        {
            // If player approaches within 12m, it vanishes into the fog
            if (distToPlayer < 12f)
            {
                game.Audio.PlayCue("monster_scream");
                game.State.SawFogSilhouette = true;
                State = EnemyState.Dead; // Despawn cleanly
            }
            return;
        }

        switch (State)
        {
            case EnemyState.Idle:
                // Check player detection range (sight ~10m or closer if player is running)
                float detectRange = 11f;
                if (distToPlayer < detectRange)
                {
                    State = EnemyState.DetectPlayer;
                    StateTimer = 0.5f;
                    game.Audio.PlayCue("monster_scream");
                }
                break;

            case EnemyState.DetectPlayer:
                StateTimer -= dt;
                // Face player
                TurnTowards(player.Position);
                if (StateTimer <= 0f)
                {
                    State = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                TurnTowards(player.Position);

                if (distToPlayer <= 1.45f)
                {
                    // Attack range
                    if (AttackCooldown <= 0f)
                    {
                        State = EnemyState.Attack;
                        StateTimer = 0.75f;
                    }
                }
                else
                {
                    // Shambling chase movement (2.35 m/s)
                    float speed = 2.35f;
                    Vector3 moveDir = new(MathF.Sin(Facing), 0f, -MathF.Cos(Facing));
                    Vector3 nextPos = Position + moveDir * speed * dt;

                    // World boundary / obstacle collision check
                    if (!world.IsBlocked(nextPos, Radius, game.State.PoliceStationUnlocked))
                    {
                        Position = nextPos;
                    }
                    else
                    {
                        // Try sliding along X or Z
                        Vector3 nextPosX = Position + new Vector3(moveDir.X * speed * dt, 0, 0);
                        if (!world.IsBlocked(nextPosX, Radius, game.State.PoliceStationUnlocked))
                            Position = nextPosX;
                        else
                        {
                            Vector3 nextPosZ = Position + new Vector3(0, 0, moveDir.Z * speed * dt);
                            if (!world.IsBlocked(nextPosZ, Radius, game.State.PoliceStationUnlocked))
                                Position = nextPosZ;
                        }
                    }
                }
                break;

            case EnemyState.Attack:
                StateTimer -= dt;
                TurnTowards(player.Position);

                // Swipe hit timing halfway through attack animation
                if (StateTimer <= 0.35f && StateTimer + dt > 0.35f)
                {
                    if (distToPlayer <= 1.8f)
                    {
                        game.Player.Damage(25f);
                        game.Audio.PlayCue("player_hurt");
                        game.Particles.SpawnBloodImpact(player.Position + new Vector3(0, 1.0f, 0), Vector3.Up);
                    }
                }

                if (StateTimer <= 0f)
                {
                    AttackCooldown = 1.35f;
                    State = EnemyState.Chase;
                }
                break;
        }
    }

    public override void TakeDamage(float amount, Vector3 hitDirection, HighFogGame game)
    {
        if (State == EnemyState.Dead) return;

        Health = MathF.Max(0f, Health - amount);
        game.Audio.PlayCue("monster_hit");
        game.Particles.SpawnBloodImpact(Position + new Vector3(0, 1.2f, 0), -hitDirection);

        if (Health <= 0f)
        {
            State = EnemyState.Dead;
            game.Audio.PlayCue("monster_scream");
            game.State.FirstWalkerDefeated = true;
            game.ShowToast("THE CREATURE HAS COLLAPSED.");
            
            // If killed in basement, update narrative objective
            if (game.World.IsInBasement(Position))
            {
                game.State.Objective = "SEARCH THE BASEMENT AND LOCATE CLUES.";
            }
        }
        else
        {
            // Stagger backward
            State = EnemyState.Stagger;
            StaggerTimer = 0.42f;
            Vector3 push = hitDirection * 0.45f;
            push.Y = 0f;
            if (!game.World.IsBlocked(Position + push, Radius, game.State.PoliceStationUnlocked))
            {
                Position += push;
            }
        }
    }

    private void TurnTowards(Vector3 target)
    {
        Vector3 dir = target - Position;
        Facing = MathF.Atan2(dir.X, -dir.Z);
    }
}
