using System.Text.Json;
using Microsoft.Xna.Framework;

namespace HighFog;

public sealed class SaveData
{
    public float PlayerX { get; set; }
    public float PlayerY { get; set; }
    public float PlayerZ { get; set; }
    public float PlayerFacing { get; set; }
    public float PlayerHealth { get; set; }
    public bool HasHandgun { get; set; }
    public int HandgunAmmo { get; set; }
    public int HandgunReserveAmmo { get; set; }
    public List<SavedItem> Inventory { get; set; } = new();

    // Narrative flags
    public bool MetClara { get; set; }
    public bool PoliceStationUnlocked { get; set; }
    public bool FoundBasement { get; set; }
    public bool FoundGun { get; set; }
    public bool FirstWalkerDefeated { get; set; }
    public bool ReadProjectHaze { get; set; }
    public bool SawFogSilhouette { get; set; }
    public string Objective { get; set; } = string.Empty;
}

public sealed class SavedItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Quantity { get; set; }
    public bool IsUsable { get; set; }
}

public static class SaveManager
{
    private static readonly string SaveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

    public static bool HasSaveFile => File.Exists(SaveFilePath);

    public static bool SaveGame(HighFogGame game)
    {
        try
        {
            var data = new SaveData
            {
                PlayerX = game.Player.Position.X,
                PlayerY = game.Player.Position.Y,
                PlayerZ = game.Player.Position.Z,
                PlayerFacing = game.Player.Facing,
                PlayerHealth = game.Player.Health,
                HasHandgun = game.Player.HasHandgun,
                HandgunAmmo = game.Player.Handgun.Ammo,
                HandgunReserveAmmo = game.Player.Handgun.ReserveAmmo,
                MetClara = game.State.MetClara,
                PoliceStationUnlocked = game.State.PoliceStationUnlocked,
                FoundBasement = game.State.FoundBasement,
                FoundGun = game.State.FoundGun,
                FirstWalkerDefeated = game.State.FirstWalkerDefeated,
                ReadProjectHaze = game.State.ReadProjectHaze,
                SawFogSilhouette = game.State.SawFogSilhouette,
                Objective = game.State.Objective
            };

            foreach (var item in game.Inventory.Items)
            {
                data.Inventory.Add(new SavedItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Type = (int)item.Type,
                    Quantity = item.Quantity,
                    IsUsable = item.IsUsable
                });
            }

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool LoadGame(HighFogGame game)
    {
        try
        {
            if (!File.Exists(SaveFilePath)) return false;

            string json = File.ReadAllText(SaveFilePath);
            var data = JsonSerializer.Deserialize<SaveData>(json);
            if (data == null) return false;

            game.Player.Position = new Vector3(data.PlayerX, data.PlayerY, data.PlayerZ);
            game.Player.Facing = data.PlayerFacing;
            game.Player.Health = data.PlayerHealth;
            game.Player.HasHandgun = data.HasHandgun;
            game.Player.Handgun.Ammo = data.HandgunAmmo;
            game.Player.Handgun.ReserveAmmo = data.HandgunReserveAmmo;

            game.State.MetClara = data.MetClara;
            game.State.PoliceStationUnlocked = data.PoliceStationUnlocked;
            game.State.FoundBasement = data.FoundBasement;
            game.State.FoundGun = data.FoundGun;
            game.State.FirstWalkerDefeated = data.FirstWalkerDefeated;
            game.State.ReadProjectHaze = data.ReadProjectHaze;
            game.State.SawFogSilhouette = data.SawFogSilhouette;
            game.State.Objective = data.Objective;

            // Reconstruct inventory
            while (game.Inventory.Items.Count > 0)
            {
                game.Inventory.Remove(game.Inventory.Items[0].Id, 999);
            }

            foreach (var sItem in data.Inventory)
            {
                game.Inventory.Add(new Item(sItem.Id, sItem.Name, sItem.Description, (ItemType)sItem.Type, sItem.Quantity, sItem.IsUsable));
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
