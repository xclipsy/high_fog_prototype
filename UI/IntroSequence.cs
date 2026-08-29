using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HighFog;

/// <summary>
/// Atmospheric introduction sequence fading from darkness with retro typewriter text before gameplay begins.
/// </summary>
public sealed class IntroSequence
{
    private float _timer;
    private const float TotalDuration = 6.5f;

    public void Reset()
    {
        _timer = 0f;
    }

    public void Update(float dt, InputTracker input, HighFogGame game)
    {
        _timer += dt;

        // Skip intro with Enter / Space / E
        if (input.Pressed(Keys.Enter) || input.Pressed(Keys.Space) || input.Pressed(Keys.E))
        {
            _timer = TotalDuration;
        }

        if (_timer >= TotalDuration)
        {
            game.SetScreenState(ScreenState.Playing);
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, int screenWidth, int screenHeight)
    {
        float alpha = 1.0f;
        if (_timer > 4.5f)
        {
            alpha = 1.0f - (_timer - 4.5f) / 2.0f;
            alpha = Math.Clamp(alpha, 0f, 1f);
        }

        // Black backdrop fading into the game world
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(0, 0, 0, (int)(255 * alpha)), Color.Transparent, 0);

        if (_timer >= 0.8f && _timer < 5.5f)
        {
            float textFade = 1.0f;
            if (_timer < 1.6f) textFade = (_timer - 0.8f) / 0.8f;
            else if (_timer > 4.2f) textFade = 1.0f - (_timer - 4.2f) / 1.3f;
            textFade = Math.Clamp(textFade, 0f, 1f);

            Color textColor = new(235, 235, 235, (int)(255 * textFade));
            Color subtitleColor = new(160, 185, 195, (int)(255 * textFade));

            font.DrawStringCentered(spriteBatch, "G R A Y H A V E N", new Vector2(screenWidth * 0.5f, screenHeight * 0.4f), textColor, 3.2f);
            font.DrawStringCentered(spriteBatch, "NOVEMBER 14   11:43 PM", new Vector2(screenWidth * 0.5f, screenHeight * 0.49f), subtitleColor, 1.8f);
            font.DrawStringCentered(spriteBatch, "COMMUNICATIONS WITH THE TOWN HAVE CEASED.", new Vector2(screenWidth * 0.5f, screenHeight * 0.56f), subtitleColor, 1.6f);
        }

        if (_timer < 5.0f)
        {
            font.DrawStringCentered(spriteBatch, "[SPACE / ENTER] SKIP", new Vector2(screenWidth * 0.5f, screenHeight - 35), new Color(100, 120, 125, 160), 1.4f);
        }
    }
}
