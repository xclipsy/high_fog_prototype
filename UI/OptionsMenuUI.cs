using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Options and Movement Configuration menu allowing real-time adjustment of controls, camera, and visuals.
/// </summary>
public sealed class OptionsMenuUI
{
    private static readonly string[] OptionLabels =
    {
        "MOUSE SENSITIVITY",
        "INVERT Y AXIS",
        "AIMING MODE",
        "RUNNING MODE",
        "MOVEMENT SPEED",
        "FOG DISTANCE",
        "AMBIENT VOLUME",
        "BACK"
    };

    private int _selectedIndex;

    public void Update(InputTracker input, GameConfig config, HighFogGame game, Action onBack)
    {
        if (input.Pressed(Keys.W) || input.Pressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + OptionLabels.Length) % OptionLabels.Length;
            game.Audio.PlayCue("ui_blip");
        }
        else if (input.Pressed(Keys.S) || input.Pressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % OptionLabels.Length;
            game.Audio.PlayCue("ui_blip");
        }

        // Adjust value with A/D, Left/Right, or Enter
        bool left = input.Pressed(Keys.A) || input.Pressed(Keys.Left);
        bool right = input.Pressed(Keys.D) || input.Pressed(Keys.Right);
        bool confirm = input.Pressed(Keys.Enter) || input.Pressed(Keys.E);

        if (left || right || confirm)
        {
            switch (_selectedIndex)
            {
                case 0: // Mouse Sensitivity
                    if (left) config.MouseSensitivity = MathF.Max(0.0015f, config.MouseSensitivity - 0.0005f);
                    if (right || confirm) config.MouseSensitivity = MathF.Min(0.0085f, config.MouseSensitivity + 0.0005f);
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 1: // Invert Y
                    config.InvertY = !config.InvertY;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 2: // Aiming Mode
                    config.ToggleAim = !config.ToggleAim;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 3: // Running Mode
                    config.ToggleRun = !config.ToggleRun;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 4: // Movement Speed
                    config.MovementSpeedMultiplier = (config.MovementSpeedMultiplier >= 1.25f) ? 1.0f : 1.25f;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 5: // Fog Distance
                    if (config.FogDistanceScale >= 1.3f) config.FogDistanceScale = 0.8f;
                    else if (config.FogDistanceScale >= 1.0f) config.FogDistanceScale = 1.3f;
                    else config.FogDistanceScale = 1.0f;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 6: // Ambient Volume
                    if (left) config.MusicVolume = MathF.Max(0.0f, config.MusicVolume - 0.1f);
                    if (right || confirm) config.MusicVolume = (config.MusicVolume >= 1.0f) ? 0.0f : MathF.Min(1.0f, config.MusicVolume + 0.1f);
                    game.Audio.MusicVolume = config.MusicVolume;
                    game.Audio.PlayCue("ui_blip");
                    break;

                case 7: // Back
                    game.Audio.PlayCue("ui_select");
                    onBack();
                    break;
            }
        }

        if (input.Pressed(Keys.Escape))
        {
            game.Audio.PlayCue("ui_select");
            onBack();
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, GameConfig config, int screenWidth, int screenHeight)
    {
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(8, 12, 16, 230), Color.Transparent, 0);

        int menuWidth = 620;
        int menuHeight = 440;
        int menuX = (screenWidth - menuWidth) / 2;
        int menuY = (screenHeight - menuHeight) / 2;

        font.DrawBox(spriteBatch, new Rectangle(menuX, menuY, menuWidth, menuHeight), new Color(16, 22, 26), new Color(75, 95, 105), 3);
        font.DrawBox(spriteBatch, new Rectangle(menuX + 4, menuY + 4, menuWidth - 8, menuHeight - 8), Color.Transparent, new Color(35, 45, 52), 1);

        font.DrawStringCentered(spriteBatch, "--- SETTINGS & CONTROLS ---", new Vector2(screenWidth * 0.5f, menuY + 28), new Color(230, 220, 185), 2.2f);

        int startY = menuY + 74;
        for (int i = 0; i < OptionLabels.Length; i++)
        {
            bool isSelected = (i == _selectedIndex);
            Color labelColor = isSelected ? new Color(255, 235, 120) : new Color(195, 205, 210);
            Color valColor = isSelected ? new Color(125, 235, 175) : new Color(165, 195, 200);

            string valueStr = GetOptionValueString(i, config);

            string prefix = isSelected ? "> " : "  ";
            font.DrawString(spriteBatch, $"{prefix}{OptionLabels[i]}", new Vector2(menuX + 28, startY + i * 38), labelColor, 1.8f);

            if (!string.IsNullOrEmpty(valueStr))
            {
                var valSize = font.MeasureString(valueStr, 1.8f);
                font.DrawString(spriteBatch, valueStr, new Vector2(menuX + menuWidth - valSize.X - 32, startY + i * 38), valColor, 1.8f);
            }
        }

        font.DrawStringCentered(spriteBatch, "[W / S] SELECT   [A / D / ENTER] CHANGE   [ESC] BACK", new Vector2(screenWidth * 0.5f, menuY + menuHeight - 22), new Color(125, 145, 150), 1.4f);
    }

    private static string GetOptionValueString(int index, GameConfig config)
    {
        return index switch
        {
            0 => $"[ {(int)(config.MouseSensitivity * 1000f)} ]",
            1 => config.InvertY ? "< ON >" : "< OFF >",
            2 => config.ToggleAim ? "< TOGGLE >" : "< HOLD >",
            3 => config.ToggleRun ? "< TOGGLE >" : "< HOLD >",
            4 => (config.MovementSpeedMultiplier >= 1.2f) ? "< FAST (1.25x) >" : "< NORMAL (1.0x) >",
            5 => (config.FogDistanceScale >= 1.2f) ? "< CLEAR (1.3x) >" : (config.FogDistanceScale <= 0.9f ? "< DENSE (0.8x) >" : "< NORMAL >"),
            6 => $"< {(int)(config.MusicVolume * 100f)}% >",
            _ => string.Empty
        };
    }
}
