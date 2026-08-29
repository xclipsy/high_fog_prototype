using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Demo Chapter complete screen celebrating the resolution of the vertical slice investigation.
/// </summary>
public sealed class EndingUI
{
    public void Update(InputTracker input, HighFogGame game)
    {
        if (input.Pressed(Keys.Enter) || input.Pressed(Keys.Space) || input.Pressed(Keys.Escape))
        {
            game.SetScreenState(ScreenState.MainMenu);
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, int screenWidth, int screenHeight)
    {
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(10, 16, 20, 240), Color.Transparent, 0);

        font.DrawStringCentered(spriteBatch, "C H A P T E R   I   C O M P L E T E", new Vector2(screenWidth * 0.5f, screenHeight * 0.22f), new Color(245, 220, 135), 3.0f);
        font.DrawStringCentered(spriteBatch, "THE TRUTH OF GRAYHAVEN", new Vector2(screenWidth * 0.5f, screenHeight * 0.32f), new Color(175, 205, 215), 1.8f);

        string loreSummary = "You uncovered the secret of PROJECT HAZE beneath the old precinct.\n" +
                             "The fog is not weather, and the creatures are not human.\n" +
                             "A deep subterranean rift has opened beneath the valley.\n" +
                             "Clara survived to share the warning.\n\n" +
                             "Thank you for playing the HIGH FOG Demo Slice!";

        var summarySize = font.MeasureString(loreSummary, 1.8f);
        int boxWidth = (int)summarySize.X + 48;
        int boxHeight = (int)summarySize.Y + 40;
        int boxX = (screenWidth - boxWidth) / 2;
        int boxY = (int)(screenHeight * 0.42f);

        font.DrawBox(spriteBatch, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(18, 26, 32), new Color(90, 120, 135), 2);
        font.DrawString(spriteBatch, loreSummary, new Vector2(boxX + 24, boxY + 20), new Color(220, 230, 235), 1.8f);

        font.DrawStringCentered(spriteBatch, "[PRESS ENTER TO RETURN TO MAIN MENU]", new Vector2(screenWidth * 0.5f, screenHeight - 50), new Color(255, 235, 120), 1.8f);
    }
}
