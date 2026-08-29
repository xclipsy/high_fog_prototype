using Microsoft.Xna.Framework;

namespace HighFog;

/// <summary>
/// Scans the environment for nearby interactables, manages HUD prompt hints, and handles execution on [E].
/// </summary>
public sealed class InteractionManager
{
    private readonly List<IInteractable> _interactables = new();
    public IInteractable? CurrentTarget { get; private set; }

    public IReadOnlyList<IInteractable> All => _interactables;

    public void Register(IInteractable interactable)
    {
        _interactables.Add(interactable);
    }

    public void Clear() => _interactables.Clear();

    public void Update(Player player, HighFogGame game)
    {
        CurrentTarget = null;
        float bestDistance = 2.4f;

        foreach (var item in _interactables)
        {
            if (!item.IsAvailable) continue;

            float dist = Vector3.Distance(player.Position, item.Position);
            if (dist < bestDistance)
            {
                // Verify facing angle roughly towards object
                Vector3 toObj = Vector3.Normalize(item.Position - player.Position);
                Vector3 playerDir = new(MathF.Sin(player.Facing), 0f, -MathF.Cos(player.Facing));
                float dot = Vector3.Dot(toObj, playerDir);

                if (dot > -0.2f || dist < 1.4f)
                {
                    bestDistance = dist;
                    CurrentTarget = item;
                }
            }
        }
    }

    public void TriggerInteraction(HighFogGame game)
    {
        if (CurrentTarget != null && CurrentTarget.IsAvailable)
        {
            CurrentTarget.Interact(game);
        }
    }
}
