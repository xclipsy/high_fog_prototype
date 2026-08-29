using Microsoft.Xna.Framework;

namespace HighFog;

public interface IInteractable
{
    Vector3 Position { get; }
    bool IsAvailable { get; }
    string GetInteractionText(HighFogGame game);
    void Interact(HighFogGame game);
}
