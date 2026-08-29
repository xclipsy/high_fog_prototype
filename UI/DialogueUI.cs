using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Dialogue box UI rendered at the bottom of the screen with character name, typewriter text, and continue prompts.
/// </summary>
public sealed class DialogueUI
{
    private float _typewriterTimer;
    private int _visibleCharacters;

    public void ResetTypewriter()
    {
        _typewriterTimer = 0f;
        _visibleCharacters = 0;
    }

    public void Update(float dt, DialogueSequence dialogue, HighFogGame game)
    {
        if (!dialogue.IsActive || dialogue.CurrentLine == null) return;

        _typewriterTimer += dt;
        int targetChars = (int)(_typewriterTimer * 38f); // 38 characters per second
        
        if (targetChars > _visibleCharacters && _visibleCharacters < dialogue.CurrentLine.Text.Length)
        {
            _visibleCharacters = Math.Min(targetChars, dialogue.CurrentLine.Text.Length);
            if (_visibleCharacters % 3 == 0)
            {
                game.Audio.PlayCue("ui_blip");
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, DialogueSequence dialogue, TextureGenerator textures, int screenWidth, int screenHeight)
    {
        if (!dialogue.IsActive || dialogue.CurrentLine == null) return;

        var line = dialogue.CurrentLine;
        int boxWidth = Math.Min(780, screenWidth - 48);
        int boxHeight = 140;
        int boxX = (screenWidth - boxWidth) / 2;
        int boxY = screenHeight - boxHeight - 28;

        // Dark retro box with double border
        font.DrawBox(spriteBatch, new Rectangle(boxX, boxY, boxWidth, boxHeight), new Color(14, 18, 22, 240), new Color(75, 95, 105), 3);
        font.DrawBox(spriteBatch, new Rectangle(boxX + 4, boxY + 4, boxWidth - 8, boxHeight - 8), Color.Transparent, new Color(42, 54, 62), 1);

        // NPC Portrait on left corner (N64-style 24x24 scaled to 96x96)
        if (!string.IsNullOrEmpty(dialogue.CurrentPortraitKey))
        {
            Texture2D portrait = textures.Get(dialogue.CurrentPortraitKey);
            int portraitSize = 96;
            int portraitX = 24;
            int portraitY = screenHeight - portraitSize - 40;
            
            // Draw portrait with border
            font.DrawBox(spriteBatch, new Rectangle(portraitX - 4, portraitY - 4, portraitSize + 8, portraitSize + 8), 
                new Color(14, 18, 22, 240), new Color(75, 95, 105), 2);
            spriteBatch.Draw(portrait, new Rectangle(portraitX, portraitY, portraitSize, portraitSize), Color.White);
        }

        // Speaker Header
        string speakerText = $"[{line.Speaker}]";
        Color speakerColor = line.Speaker == "CLARA" ? new Color(245, 185, 95) : new Color(210, 225, 230);
        font.DrawString(spriteBatch, speakerText, new Vector2(boxX + 20, boxY + 16), speakerColor, 2.0f);

        // Dialogue Body Text with word wrap
        string fullText = line.Text;
        string displayedText = fullText.Substring(0, Math.Min(_visibleCharacters, fullText.Length));
        string wrappedText = WrapText(displayedText, 52);

        font.DrawString(spriteBatch, wrappedText, new Vector2(boxX + 24, boxY + 46), new Color(230, 235, 235), 1.8f);

        // Continue prompt
        string continueText = "[E] CONTINUE";
        var contSize = font.MeasureString(continueText, 1.6f);
        font.DrawString(spriteBatch, continueText, new Vector2(boxX + boxWidth - contSize.X - 20, boxY + boxHeight - contSize.Y - 12), new Color(175, 205, 120), 1.6f);
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
