using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Atmospheric main menu screen with title banner, fog visuals, settings, and game launch triggers.
/// </summary>
public sealed class MainMenuUI
{
    private static readonly string[] MenuItems =
    {
        "NEW GAME",
        "LOAD GAME",
        "SETTINGS",
        "CONTROLS",
        "QUIT"
    };

    private int _selectedIndex;
    private bool _showingControls;
    private bool _showingOptions;
    private readonly OptionsMenuUI _optionsUI = new();
    private float _titleTimer;

    public void Update(float dt, InputTracker input, HighFogGame game)
    {
        _titleTimer += dt;

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
            _selectedIndex = (_selectedIndex - 1 + MenuItems.Length) % MenuItems.Length;
            game.Audio.PlayCue("ui_blip");
        }
        else if (input.Pressed(Keys.S) || input.Pressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % MenuItems.Length;
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
            case 0: // New Game
                game.StartNewGame();
                break;
            case 1: // Load Game
                if (SaveManager.LoadGame(game))
                {
                    game.ResumeGame();
                }
                else
                {
                    game.ShowToast("NO SAVE FILE FOUND.");
                }
                break;
            case 2: // Settings
                _showingOptions = true;
                break;
            case 3: // Controls
                _showingControls = true;
                break;
            case 4: // Quit
                game.Exit();
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, GameConfig config, int screenWidth, int screenHeight)
    {
        // Dark atmospheric background
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(10, 14, 18), Color.Transparent, 0);

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

        // Title Header with subtle eerie pulse
        float glow = 0.85f + MathF.Sin(_titleTimer * 2f) * 0.15f;
        Color titleColor = new((int)(240 * glow), (int)(220 * glow), (int)(180 * glow));

        font.DrawStringCentered(spriteBatch, "H I G H   F O G", new Vector2(screenWidth * 0.5f, screenHeight * 0.24f), titleColor, 4.0f);
        font.DrawStringCentered(spriteBatch, "GRAYHAVEN SURVIVAL INCIDENT", new Vector2(screenWidth * 0.5f, screenHeight * 0.34f), new Color(145, 175, 185), 1.8f);

        // Menu Items
        int startY = (int)(screenHeight * 0.48f);
        for (int i = 0; i < MenuItems.Length; i++)
        {
            bool isSelected = (i == _selectedIndex);
            Color itemColor = isSelected ? new Color(255, 235, 120) : new Color(180, 195, 200);
            string text = isSelected ? $">  {MenuItems[i]}  <" : MenuItems[i];

            font.DrawStringCentered(spriteBatch, text, new Vector2(screenWidth * 0.5f, startY + i * 40), itemColor, 2.0f);
        }

        // Footer
        font.DrawStringCentered(spriteBatch, "MONOGAME N64-STYLE 3D SURVIVAL HORROR", new Vector2(screenWidth * 0.5f, screenHeight - 36), new Color(95, 115, 120), 1.4f);
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
