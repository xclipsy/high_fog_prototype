namespace HighFog;

public enum ItemType
{
    Handgun,
    HandgunAmmo,
    PoliceKey,
    BasementKey,
    Medkit,
    PoliceReport,
    ProjectHazeDocument
}

public sealed class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ItemType Type { get; }
    public int Quantity { get; set; }
    public bool IsUsable { get; }

    public Item(string id, string name, string description, ItemType type, int quantity = 1, bool isUsable = false)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Quantity = quantity;
        IsUsable = isUsable;
    }

    public static Item CreateHandgun() => new(
        "handgun",
        "OLD SERVICE 9MM",
        "Standard police issue revolver. Heavy steel frame, reliable firing mechanism. Holds 6 rounds.",
        ItemType.Handgun,
        1,
        false
    );

    public static Item CreateAmmo(int count = 12) => new(
        "ammo_9mm",
        $"9MM ROUNDS x{count}",
        "A box of standard 9mm hollow point ammunition.",
        ItemType.HandgunAmmo,
        count,
        false
    );

    public static Item CreatePoliceKey() => new(
        "police_key",
        "POLICE STATION KEY",
        "Brass key found near the town square with an engraved precinct crest.",
        ItemType.PoliceKey,
        1,
        false
    );

    public static Item CreateBasementKey() => new(
        "basement_key",
        "BASEMENT HATCH KEY",
        "Heavy iron key labeled 'SUB-LEVEL STORAGE'.",
        ItemType.BasementKey,
        1,
        false
    );

    public static Item CreateMedkit() => new(
        "medkit",
        "FIRST AID KIT",
        "Sterile gauze, antiseptic, and adrenaline ampoules. Restores 50 HP.",
        ItemType.Medkit,
        1,
        true
    );

    public static Item CreatePoliceReport() => new(
        "police_report",
        "OFFICER'S DISPATCH LOG",
        "Folded logbook entries detailing the sudden descent of the fog and team dispatches.",
        ItemType.PoliceReport,
        1,
        true
    );

    public static Item CreateProjectHazeDoc() => new(
        "project_haze",
        "PROJECT HAZE REPORT",
        "Classified document recovered from the basement safe mentioning deep sub-surface tests.",
        ItemType.ProjectHazeDocument,
        1,
        true
    );
}
