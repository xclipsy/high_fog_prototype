using Microsoft.Xna.Framework;

namespace HighFog;

public sealed class CustomInteractable : IInteractable
{
    public Vector3 Position { get; }
    public bool IsAvailable => true;
    private readonly Func<string> _getText;
    private readonly Action _onInteract;

    public CustomInteractable(Vector3 position, Func<string> getText, Action onInteract)
    {
        Position = position;
        _getText = getText;
        _onInteract = onInteract;
    }

    public string GetInteractionText(HighFogGame game) => _getText();
    public void Interact(HighFogGame game) => _onInteract();
}

public sealed class ItemPickupInteractable : IInteractable
{
    public Vector3 Position { get; }
    public Item Item { get; }
    public bool IsAvailable { get; private set; } = true;
    public string CustomPickupMessage { get; }

    public ItemPickupInteractable(Vector3 position, Item item, string customPickupMessage = "")
    {
        Position = position;
        Item = item;
        CustomPickupMessage = customPickupMessage;
    }

    public string GetInteractionText(HighFogGame game) => $"PICK UP {Item.Name}";

    public void Interact(HighFogGame game)
    {
        if (!IsAvailable) return;

        if (game.Inventory.Add(Item))
        {
            IsAvailable = false;
            game.Audio.PlayCue("item_pickup");
            
            if (Item.Type == ItemType.Handgun)
            {
                game.Player.HasHandgun = true;
                game.Player.Handgun.Ammo = 6;
                game.Player.Handgun.ReserveAmmo = 12;
                game.State.FoundGun = true;
                game.State.Objective = "SURVIVE THE THREAT IN THE POLICE BASEMENT.";
                game.ShowToast("ACQUIRED OLD SERVICE 9MM (AMMO: 6 / 12)");
                // Spawn/wake up the Fog Walker in the basement corridor!
                game.Enemies.TriggerBasementAmbush();
            }
            else if (Item.Type == ItemType.HandgunAmmo)
            {
                game.Player.Handgun.ReserveAmmo += Item.Quantity;
                game.ShowToast($"ACQUIRED {Item.Quantity} 9MM ROUNDS");
            }
            else
            {
                game.ShowToast(string.IsNullOrEmpty(CustomPickupMessage) ? $"ACQUIRED {Item.Name}" : CustomPickupMessage);
            }
        }
        else
        {
            game.ShowToast("INVENTORY IS FULL!");
        }
    }
}

public sealed class DoorInteractable : IInteractable
{
    public Vector3 Position { get; }
    public string DoorName { get; }
    public bool IsLocked { get; set; }
    public ItemType RequiredKey { get; }
    public bool IsAvailable => true;
    public Action<HighFogGame>? OnOpen { get; }

    public DoorInteractable(Vector3 position, string doorName, bool isLocked, ItemType requiredKey, Action<HighFogGame>? onOpen = null)
    {
        Position = position;
        DoorName = doorName;
        IsLocked = isLocked;
        RequiredKey = requiredKey;
        OnOpen = onOpen;
    }

    public string GetInteractionText(HighFogGame game)
    {
        if (IsLocked)
        {
            return game.Inventory.Has(RequiredKey) ? $"UNLOCK {DoorName.ToUpperInvariant()}" : $"LOCKED ({DoorName.ToUpperInvariant()})";
        }
        return $"ENTER {DoorName.ToUpperInvariant()}";
    }

    public void Interact(HighFogGame game)
    {
        if (IsLocked)
        {
            if (game.Inventory.Has(RequiredKey))
            {
                IsLocked = false;
                game.Audio.PlayCue("door");
                game.ShowToast($"UNLOCKED {DoorName.ToUpperInvariant()}");
                OnOpen?.Invoke(game);
            }
            else
            {
                game.Audio.PlayCue("dryfire");
                game.ShowToast($"THE {DoorName.ToUpperInvariant()} IS LOCKED TIGHT.");
            }
            return;
        }

        game.Audio.PlayCue("door");
        OnOpen?.Invoke(game);
    }
}

public sealed class DocumentInteractable : IInteractable
{
    public Vector3 Position { get; }
    public string Title { get; }
    public string Content { get; }
    public string FlagToSet { get; }
    public bool IsAvailable => true;

    public DocumentInteractable(Vector3 position, string title, string content, string flagToSet = "")
    {
        Position = position;
        Title = title;
        Content = content;
        FlagToSet = flagToSet;
    }

    public string GetInteractionText(HighFogGame game) => $"READ {Title.ToUpperInvariant()}";

    public void Interact(HighFogGame game)
    {
        game.Audio.PlayCue("page_turn");
        game.OpenDocument(Title, Content);

        if (FlagToSet == "ProjectHaze")
        {
            game.State.ReadProjectHaze = true;
            game.State.Objective = "RETURN TO CLARA WITH THE PROJECT HAZE DISCOVERY.";
        }
    }
}
