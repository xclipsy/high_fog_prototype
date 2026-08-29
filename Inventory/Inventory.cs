namespace HighFog;

/// <summary>
/// Classic survival horror gridless inventory system.
/// Tracks weapons, keys, documents, medical supplies, and ammunition.
/// </summary>
public sealed class Inventory
{
    public const int MaxSlots = 8;
    private readonly List<Item> _items = new();

    public IReadOnlyList<Item> Items => _items;

    public bool Add(Item item)
    {
        // Stack ammunition
        if (item.Type == ItemType.HandgunAmmo)
        {
            var existingAmmo = _items.FirstOrDefault(i => i.Type == ItemType.HandgunAmmo);
            if (existingAmmo != null)
            {
                existingAmmo.Quantity += item.Quantity;
                return true;
            }
        }

        if (_items.Count >= MaxSlots)
        {
            return false;
        }

        _items.Add(item);
        return true;
    }

    public bool Remove(string id, int count = 1)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item == null) return false;

        if (item.Quantity > count)
        {
            item.Quantity -= count;
            return true;
        }

        _items.Remove(item);
        return true;
    }

    public bool Has(ItemType type) => _items.Any(i => i.Type == type);
    public bool Has(string id) => _items.Any(i => i.Id == id);

    public Item? Get(string id) => _items.FirstOrDefault(i => i.Id == id);
    public Item? Get(ItemType type) => _items.FirstOrDefault(i => i.Type == type);

    public bool UseItem(Item item, Player player, out string message)
    {
        if (item.Type == ItemType.Medkit)
        {
            if (player.Health >= 100f)
            {
                message = "HEALTH IS ALREADY FULL.";
                return false;
            }

            player.Health = MathF.Min(100f, player.Health + 50f);
            Remove(item.Id, 1);
            message = "USED FIRST AID KIT. HEALTH RESTORED.";
            return true;
        }

        if (item.Type == ItemType.PoliceReport || item.Type == ItemType.ProjectHazeDocument)
        {
            message = $"READING {item.Name}...";
            return true;
        }

        message = "CANNOT USE THIS ITEM HERE.";
        return false;
    }
}
