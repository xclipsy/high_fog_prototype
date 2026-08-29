using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Survival horror inventory interface showing slots, item inspection details, and use actions.
/// </summary>
public sealed class InventoryUI
{
    private int _selectedIndex;

    public void Update(InputTracker input, Inventory inventory, Player player, HighFogGame game)
    {
        if (inventory.Items.Count == 0) return;

        if (input.Pressed(Keys.W) || input.Pressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + inventory.Items.Count) % inventory.Items.Count;
            game.Audio.PlayCue("ui_blip");
        }
        else if (input.Pressed(Keys.S) || input.Pressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % inventory.Items.Count;
            game.Audio.PlayCue("ui_blip");
        }

        if (input.Pressed(Keys.Enter) || input.Pressed(Keys.E))
        {
            if (_selectedIndex >= 0 && _selectedIndex < inventory.Items.Count)
            {
                var item = inventory.Items[_selectedIndex];
                if (item.IsUsable)
                {
                    if (inventory.UseItem(item, player, out string msg))
                    {
                        game.Audio.PlayCue(item.Type == ItemType.Medkit ? "item_pickup" : "page_turn");
                        game.ShowToast(msg);
                        if (item.Type == ItemType.PoliceReport)
                        {
                            game.OpenDocument("OFFICER'S LOG", "NOVEMBER 14 - 22:15\nThick fog rolled in off the mountains. Radio tower went dark.\n\n23:02\nEmergency calls flooding dispatch. Reports of tall humanoid shapes in the mist. Officers Miller and Vance dispatched to investigate the old factory gate.\n\n23:40\nOfficers did not return. Radio emitting harmonic screech. Barricading the precinct front doors.");
                        }
                        else if (item.Type == ItemType.ProjectHazeDocument)
                        {
                            game.OpenDocument("PROJECT HAZE - TOP SECRET", "CLASSIFIED FACILITY REPORT\nSUBJECT: SUBTERRANEAN DRILLING ANOMALY\n\nPhase 4 deep-core resonance drilling beneath the old factory has breached an anomalous geode cavity at depth 820m.\n\nThe released particulate (designated HAZE) exhibits biological catalyst properties and temporal distortion.\n\nDO NOT DISPATCH UNPROTECTED RESCUE TEAMS. THE ANOMALY REACTS TO AUDITORY VIBRATION.");
                        }
                    }
                    else
                    {
                        game.Audio.PlayCue("dryfire");
                        game.ShowToast(msg);
                    }
                }
                else
                {
                    game.Audio.PlayCue("dryfire");
                    game.ShowToast("CANNOT USE THIS DIRECTLY.");
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, Inventory inventory, Player player, int screenWidth, int screenHeight)
    {
        // Dark overlay
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(6, 10, 12, 230), Color.Transparent, 0);

        int menuWidth = Math.Min(740, screenWidth - 40);
        int menuHeight = Math.Min(520, screenHeight - 60);
        int menuX = (screenWidth - menuWidth) / 2;
        int menuY = (screenHeight - menuHeight) / 2;

        font.DrawBox(spriteBatch, new Rectangle(menuX, menuY, menuWidth, menuHeight), new Color(16, 22, 25), new Color(75, 95, 105), 3);
        font.DrawBox(spriteBatch, new Rectangle(menuX + 4, menuY + 4, menuWidth - 8, menuHeight - 8), Color.Transparent, new Color(38, 48, 54), 1);

        // Header
        font.DrawStringCentered(spriteBatch, "--- INVENTORY ---", new Vector2(screenWidth * 0.5f, menuY + 26), new Color(230, 215, 175), 2.2f);

        // Left Column: Items List
        int listX = menuX + 24;
        int listY = menuY + 68;
        int listWidth = 320;
        int listHeight = menuHeight - 110;

        font.DrawBox(spriteBatch, new Rectangle(listX, listY, listWidth, listHeight), new Color(10, 14, 16), new Color(50, 65, 72), 1);

        if (inventory.Items.Count == 0)
        {
            font.DrawString(spriteBatch, "INVENTORY IS EMPTY", new Vector2(listX + 16, listY + 20), new Color(130, 140, 145), 1.6f);
        }
        else
        {
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                var item = inventory.Items[i];
                bool isSelected = (i == _selectedIndex);
                Color itemColor = isSelected ? new Color(255, 235, 120) : new Color(205, 215, 220);
                string text = isSelected ? $"> {item.Name}" : $"  {item.Name}";

                if (isSelected)
                {
                    font.DrawBox(spriteBatch, new Rectangle(listX + 4, listY + 12 + i * 36, listWidth - 8, 30), new Color(38, 52, 60), new Color(120, 155, 165), 1);
                }

                font.DrawString(spriteBatch, text, new Vector2(listX + 8, listY + 16 + i * 36), itemColor, 1.6f);
            }
        }

        // Right Column: Item Inspection Details
        int detailX = listX + listWidth + 20;
        int detailY = listY;
        int detailWidth = menuWidth - listWidth - 68;
        int detailHeight = listHeight;

        font.DrawBox(spriteBatch, new Rectangle(detailX, detailY, detailWidth, detailHeight), new Color(12, 16, 18), new Color(50, 65, 72), 1);

        if (inventory.Items.Count > 0 && _selectedIndex >= 0 && _selectedIndex < inventory.Items.Count)
        {
            var selectedItem = inventory.Items[_selectedIndex];
            font.DrawString(spriteBatch, selectedItem.Name, new Vector2(detailX + 16, detailY + 18), new Color(245, 220, 130), 2.0f);
            
            string qtyStr = $"QUANTITY: {selectedItem.Quantity}";
            font.DrawString(spriteBatch, qtyStr, new Vector2(detailX + 16, detailY + 52), new Color(160, 185, 195), 1.6f);

            string wrappedDesc = WrapText(selectedItem.Description, 26);
            font.DrawString(spriteBatch, wrappedDesc, new Vector2(detailX + 16, detailY + 90), new Color(215, 220, 220), 1.6f);

            if (selectedItem.IsUsable)
            {
                font.DrawString(spriteBatch, "[ENTER] USE ITEM", new Vector2(detailX + 16, detailY + detailHeight - 34), new Color(120, 215, 140), 1.6f);
            }
        }

        // Footer prompt
        font.DrawStringCentered(spriteBatch, "[W / S] SELECT   [I / TAB / ESC] CLOSE", new Vector2(screenWidth * 0.5f, menuY + menuHeight - 20), new Color(140, 160, 165), 1.5f);
    }

    private static string WrapText(string text, int maxCharsPerLine)
    {
        var words = text.Split(' ');
        var sb = new System.Text.StringBuilder();
        int lineLen = 0;

        foreach (var word in words)
        {
            if (lineLen + word.Length + 1 > maxCharsPerLine)
            {
                sb.AppendLine();
                lineLen = 0;
            }
            if (lineLen > 0)
            {
                sb.Append(' ');
                lineLen++;
            }
            sb.Append(word);
            lineLen += word.Length;
        }

        return sb.ToString();
    }
}
