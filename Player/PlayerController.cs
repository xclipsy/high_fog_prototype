using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Responsive third-person survival horror player controller with configurable movement,
/// smooth acceleration/deceleration, multi-axis collision sliding, and precise aiming.
/// </summary>
public sealed class PlayerController
{
    public const float BaseWalkSpeed = 3.25f;
    public const float BaseRunSpeed = 5.75f;
    public const float BaseAimSpeed = 1.7f;
    public const float Acceleration = 22f;
    public const float Deceleration = 28f;
    public const float TurnSpeed = 16f;

    private Vector3 _currentVelocity = Vector3.Zero;
    private bool _runToggled;
    private bool _aimToggled;

    public void Update(float dt, Player player, ThirdPersonCamera camera, World world, InputTracker input, HighFogGame game, GameConfig config)
    {
        player.Handgun.Update(dt);

        if (player.Health <= 0f)
        {
            _currentVelocity = Vector3.Zero;
            return;
        }

        // Aiming Control
        if (config.ToggleAim)
        {
            if (input.Mouse.RightButton == ButtonState.Pressed && input.Down(Keys.Space) || input.Pressed(Keys.Space))
            {
                _aimToggled = !_aimToggled;
            }
            player.IsAiming = player.HasHandgun && _aimToggled;
        }
        else
        {
            player.IsAiming = player.HasHandgun && (input.Mouse.RightButton == ButtonState.Pressed || input.Down(Keys.Space));
        }

        // Running Control
        if (config.ToggleRun)
        {
            if (input.Pressed(Keys.LeftShift) || input.Pressed(Keys.RightShift))
            {
                _runToggled = !_runToggled;
            }
        }
        else
        {
            _runToggled = input.Down(Keys.LeftShift) || input.Down(Keys.RightShift);
        }

        // Movement Direction - Fixed WASD input handling
        Vector3 moveInput = Vector3.Zero;
        
        bool wDown = input.Down(Keys.W);
        bool sDown = input.Down(Keys.S);
        bool aDown = input.Down(Keys.A);
        bool dDown = input.Down(Keys.D);
        
        if (wDown) moveInput += camera.FlatForward;
        if (sDown) moveInput -= camera.FlatForward;

        Vector3 flatRight = Vector3.Normalize(Vector3.Cross(camera.FlatForward, Vector3.Up));
        if (dDown) moveInput += flatRight;
        if (aDown) moveInput -= flatRight;

        bool hasMovement = moveInput.LengthSquared() > 0.001f;
        if (hasMovement)
        {
            moveInput = Vector3.Normalize(moveInput);
        }

        player.IsRunning = hasMovement && _runToggled && !player.IsAiming;

        float targetSpeed = player.IsAiming ? BaseAimSpeed : (player.IsRunning ? BaseRunSpeed : BaseWalkSpeed);
        targetSpeed *= config.MovementSpeedMultiplier;

        Vector3 targetVelocity = hasMovement ? (moveInput * targetSpeed) : Vector3.Zero;

        // Smooth Acceleration & Deceleration
        float accelRate = hasMovement ? Acceleration : Deceleration;
        _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, Math.Clamp(dt * accelRate, 0f, 1f));

        // Facing Rotation
        if (player.IsAiming)
        {
            // Instantly snap / smoothly track camera yaw
            player.Facing = MathHelper.Lerp(player.Facing, camera.Yaw, Math.Clamp(dt * 26f, 0f, 1f));
        }
        else if (hasMovement)
        {
            float targetFacing = MathF.Atan2(moveInput.X, -moveInput.Z);
            // Smooth angular wrap-around interpolation
            float diff = MathHelper.WrapAngle(targetFacing - player.Facing);
            player.Facing += diff * Math.Clamp(dt * TurnSpeed, 0f, 1f);
        }

        // Apply Movement & Collision Resolution
        float currentSpeed = _currentVelocity.Length();
        if (currentSpeed > 0.01f)
        {
            Vector3 delta = _currentVelocity * dt;
            Vector3 targetPos = player.Position + delta;

            bool policeUnlocked = game.State.PoliceStationUnlocked;

            if (!world.IsBlocked(targetPos, player.Radius, policeUnlocked))
            {
                player.Position = targetPos;
            }
            else
            {
                // Multi-axis collision sliding
                Vector3 targetX = player.Position + new Vector3(delta.X, 0, 0);
                if (!world.IsBlocked(targetX, player.Radius, policeUnlocked))
                {
                    player.Position = targetX;
                }
                else
                {
                    _currentVelocity.X = 0f;
                }

                Vector3 targetZ = player.Position + new Vector3(0, 0, delta.Z);
                if (!world.IsBlocked(targetZ, player.Radius, policeUnlocked))
                {
                    player.Position = targetZ;
                }
                else
                {
                    _currentVelocity.Z = 0f;
                }
            }

            // Footstep audio and animation pacing
            float animSpeed = player.IsRunning ? 9.5f : 5.8f;
            player.AnimationTimer += dt * animSpeed;
            player.FootstepTimer += dt * (player.IsRunning ? 1.8f : 1.05f);

            if (player.FootstepTimer >= 0.42f)
            {
                player.FootstepTimer = 0f;
                game.Audio.PlayCue(player.IsRunning ? "footstep_run" : "footstep");
            }
        }
        else
        {
            player.FootstepTimer = 0.2f;
            player.AnimationTimer += dt * 1.5f; // Gentle breathing idle
        }

        // Shooting
        if (player.HasHandgun && (input.LeftPressed || (player.IsAiming && input.Pressed(Keys.F))))
        {
            if (player.Handgun.TryFire())
            {
                game.Audio.PlayCue("gunshot");

                // Muzzle flash particle & light
                Vector3 muzzlePos = player.Position + new Vector3(0, 1.18f, 0) + camera.Forward * 0.65f;
                game.Particles.SpawnMuzzleFlash(muzzlePos, camera.Forward);

                // Raycast hitscan check
                var hitEnemy = game.Enemies.CheckBulletHit(muzzlePos, camera.Forward, 32f, out Vector3 hitPoint);
                if (hitEnemy != null)
                {
                    hitEnemy.TakeDamage(player.Handgun.Damage, camera.Forward, game);
                }
            }
            else if (player.Handgun.Ammo == 0 && !player.Handgun.IsReloading)
            {
                game.Audio.PlayCue("dryfire");
            }
        }

        // Reload
        if (input.Pressed(Keys.R) && player.HasHandgun)
        {
            if (player.Handgun.StartReload())
            {
                game.Audio.PlayCue("reload");
            }
        }

        // Interact
        if (input.Pressed(Keys.E))
        {
            game.Interactions.TriggerInteraction(game);
        }
    }
}
