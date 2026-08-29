using Microsoft.Xna.Framework;

namespace HighFog;

public class NPC : IInteractable
{
    public string Id { get; }
    public string Name { get; set; }
    public Personality Personality { get; set; }
    public Vector3 Position { get; set; }
    public float Facing { get; set; }
    public float Radius => 0.55f;
    public bool IsAvailable { get; set; } = true;
    public Color ClothingColor { get; set; }
    public Color AccentColor { get; set; }

    public NPC(string id, string name, Personality personality, Vector3 position, Color clothingColor, Color accentColor)
    {
        Id = id;
        Name = name;
        Personality = personality;
        Position = position;
        ClothingColor = clothingColor;
        AccentColor = accentColor;
    }

    public virtual string GetInteractionText(HighFogGame game) => $"TALK TO {Name.ToUpperInvariant()}";

    public virtual void Interact(HighFogGame game)
    {
    }
}
