using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Classic survival horror pause menu with resume, inventory access, saving, settings, and controls.
/// </summary>
public sealed class PauseMenu
{
    private static readonly string[] Options =
    {
        "RESUME",
        "INVENTORY",
        "SAVE GAME",
        "LOAD GAME",
        "SETTINGS",
        "CONTROLS",
        "MAIN MENU"
    };

    private int _selectedIndex;
    private bool _showingControls;
    private bool _showingOptions;
    private readonly OptionsMenuUI _optionsUI = new();

    public void Reset()
    {
        _selectedIndex = 0;
        _showingControls = false;
        _showingOptions = false;
    }

    public void Update(InputTracker input, HighFogGame game)
    {
        if (_showingOptions)
        {
            _optionsUI.Update(input, game.Config, game, () => _showingOptions = false);
            return;
        }

        if (_showingControls)
        {
            if (input.Pressed(Keys.Enter) || input.Pressed(Keys.Escape) || input.Pressed(Keys.E))
            {
                _showingControls = false;
                game.Audio.PlayCue("ui_blip");
            }
            return;
        }

        if (input.Pressed(Keys.W) || input.Pressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + Options.Length) % Options.Length;
            game.Audio.PlayCue("ui_blip");
        }
        else if (input.Pressed(Keys.S) || input.Pressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % Options.Length;
            game.Audio.PlayCue("ui_blip");
        }

        if (input.Pressed(Keys.Enter) || input.Pressed(Keys.E))
        {
            game.Audio.PlayCue("ui_select");
            ExecuteOption(_selectedIndex, game);
        }
    }

    private void ExecuteOption(int index, HighFogGame game)
    {
        switch (index)
        {
            case 0: // Resume
                game.ResumeGame();
                break;
            case 1: // Inventory
                game.SetScreenState(ScreenState.Inventory);
                break;
            case 2: // Save Game
                if (SaveManager.SaveGame(game))
                {
                    game.ShowToast("GAME SAVED SUCCESSFULLY.");
                }
                else
                {
                    game.ShowToast("FAILED TO SAVE GAME.");
                }
                break;
            case 3: // Load Game
                if (SaveManager.LoadGame(game))
                {
                    game.ShowToast("GAME LOADED.");
                    game.ResumeGame();
                }
                else
                {
                    game.ShowToast("NO SAVE FILE FOUND.");
                }
                break;
            case 4: // Settings
                _showingOptions = true;
                break;
            case 5: // Controls
                _showingControls = true;
                break;
            case 6: // Main Menu
                game.SetScreenState(ScreenState.MainMenu);
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, GameConfig config, int screenWidth, int screenHeight)
    {
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(8, 12, 14, 215), Color.Transparent, 0);

        if (_showingOptions)
        {
            _optionsUI.Draw(spriteBatch, font, config, screenWidth, screenHeight);
            return;
        }

        if (_showingControls)
        {
            DrawControls(spriteBatch, font, screenWidth, screenHeight);
            return;
        }

        int menuWidth = 440;
        int menuHeight = 390;
        int menuX = (screenWidth - menuWidth) / 2;
        int menuY = (screenHeight - menuHeight) / 2;

        font.DrawBox(spriteBatch, new Rectangle(menuX, menuY, menuWidth, menuHeight), new Color(16, 22, 26), new Color(75, 95, 105), 3);
        font.DrawBox(spriteBatch, new Rectangle(menuX + 4, menuY + 4, menuWidth - 8, menuHeight - 8), Color.Transparent, new Color(35, 45, 52), 1);

        font.DrawStringCentered(spriteBatch, "HIGH FOG", new Vector2(screenWidth * 0.5f, menuY + 34), new Color(230, 220, 185), 2.5f);
        font.DrawStringCentered(spriteBatch, "- PAUSED -", new Vector2(screenWidth * 0.5f, menuY + 68), new Color(145, 175, 185), 1.6f);

        int startY = menuY + 104;
        for (int i = 0; i < Options.Length; i++)
        {
            bool isSelected = (i == _selectedIndex);
            Color optColor = isSelected ? new Color(255, 235, 120) : new Color(195, 205, 210);
            string text = isSelected ? $"> {Options[i]} <" : Options[i];

            font.DrawStringCentered(spriteBatch, text, new Vector2(screenWidth * 0.5f, startY + i * 34), optColor, 1.8f);
        }

        font.DrawStringCentered(spriteBatch, "[W / S] SELECT   [ENTER] CONFIRM   [ESC] RESUME", new Vector2(screenWidth * 0.5f, menuY + menuHeight - 22), new Color(125, 145, 150), 1.4f);
    }

    private void DrawControls(SpriteBatch spriteBatch, RetroFont font, int screenWidth, int screenHeight)
    {
        int boxWidth = 560;
        int boxHeight = 420;
        int boxX = (screenWidth - boxWidth) / 2;
        int boxY = (screenHeight - boxHeight) / 2;

        font.DrawBox(spriteBatch, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(16, 22, 26), new Color(75, 95, 105), 3);
        font.DrawStringCentered(spriteBatch, "CONTROLS", new Vector2(screenWidth * 0.5f, boxY + 28), new Color(230, 220, 185), 2.2f);

        string[] controls =
        {
            "W / A / S / D     - MOVE CHARACTER",
            "LEFT SHIFT       - RUN",
            "MOUSE LOOK       - ORBIT CAMERA",
            "RIGHT MOUSE      - AIM WEAPON",
            "LEFT MOUSE       - FIRE HANDGUN",
            "R                - RELOAD",
            "E                - INTERACT / TALK / PICKUP",
            "I / TAB          - INVENTORY",
            "ESC              - PAUSE MENU",
            "F3               - TOGGLE DEBUG OVERLAY"
        };

        for (int i = 0; i < controls.Length; i++)
        {
            font.DrawString(spriteBatch, controls[i], new Vector2(boxX + 32, boxY + 70 + i * 28), new Color(205, 215, 220), 1.6f);
        }

        font.DrawStringCentered(spriteBatch, "[ENTER / ESC] BACK", new Vector2(screenWidth * 0.5f, boxY + boxHeight - 24), new Color(145, 185, 150), 1.6f);
    }
}
