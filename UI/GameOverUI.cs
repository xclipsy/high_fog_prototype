using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Game Over screen shown upon player death in the fog.
/// </summary>
public sealed class GameOverUI
{
    private static readonly string[] Options =
    {
        "RESTART CHECKPOINT",
        "LOAD LAST SAVE",
        "MAIN MENU"
    };

    private int _selectedIndex;

    public void Update(InputTracker input, HighFogGame game)
    {
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
            switch (_selectedIndex)
            {
                case 0:
                    game.RestartCheckpoint();
                    break;
                case 1:
                    if (SaveManager.LoadGame(game))
                    {
                        game.ResumeGame();
                    }
                    else
                    {
                        game.RestartCheckpoint();
                    }
                    break;
                case 2:
                    game.SetScreenState(ScreenState.MainMenu);
                    break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, int screenWidth, int screenHeight)
    {
        // Dark crimson tinted death screen
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(25, 6, 8, 235), Color.Transparent, 0);

        font.DrawStringCentered(spriteBatch, "Y O U   D I E D", new Vector2(screenWidth * 0.5f, screenHeight * 0.32f), new Color(225, 45, 45), 3.5f);
        font.DrawStringCentered(spriteBatch, "THE FOG CONSUMED ANOTHER SOUL", new Vector2(screenWidth * 0.5f, screenHeight * 0.42f), new Color(175, 140, 140), 1.8f);

        int startY = (int)(screenHeight * 0.58f);
        for (int i = 0; i < Options.Length; i++)
        {
            bool isSelected = (i == _selectedIndex);
            Color optColor = isSelected ? new Color(255, 235, 120) : new Color(195, 185, 185);
            string text = isSelected ? $">  {Options[i]}  <" : Options[i];

            font.DrawStringCentered(spriteBatch, text, new Vector2(screenWidth * 0.5f, startY + i * 40), optColor, 1.9f);
        }
    }
}
