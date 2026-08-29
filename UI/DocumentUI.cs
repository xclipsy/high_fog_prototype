using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Fullscreen survival horror document reader for files, police logs, and classified reports.
/// </summary>
public sealed class DocumentUI
{
    public void Draw(SpriteBatch spriteBatch, RetroFont font, string title, string content, int screenWidth, int screenHeight)
    {
        // Darkened background backdrop
        font.DrawBox(spriteBatch, new Rectangle(0, 0, screenWidth, screenHeight), new Color(5, 8, 10, 225), Color.Transparent, 0);

        int docWidth = Math.Min(720, screenWidth - 40);
        int docHeight = Math.Min(560, screenHeight - 60);
        int docX = (screenWidth - docWidth) / 2;
        int docY = (screenHeight - docHeight) / 2;

        // Aged paper / terminal box
        font.DrawBox(spriteBatch, new Rectangle(docX, docY, docWidth, docHeight), new Color(28, 26, 22), new Color(135, 120, 90), 3);
        font.DrawBox(spriteBatch, new Rectangle(docX + 6, docY + 6, docWidth - 12, docHeight - 12), Color.Transparent, new Color(75, 68, 52), 1);

        // Document Title Header
        font.DrawStringCentered(spriteBatch, $"--- {title.ToUpperInvariant()} ---", new Vector2(screenWidth * 0.5f, docY + 30), new Color(235, 205, 120), 2.0f);

        // Content
        string wrappedContent = WrapText(content, 48);
        font.DrawString(spriteBatch, wrappedContent, new Vector2(docX + 32, docY + 70), new Color(225, 218, 195), 1.8f);

        // Close prompt
        string closeText = "[E / ESC] PUT AWAY DOCUMENT";
        font.DrawStringCentered(spriteBatch, closeText, new Vector2(screenWidth * 0.5f, docY + docHeight - 25), new Color(145, 175, 150), 1.6f);
    }

    private static string WrapText(string text, int maxCharsPerLine)
    {
        var lines = text.Split('\n');
        var sb = new System.Text.StringBuilder();

        foreach (var rawLine in lines)
        {
            var words = rawLine.Split(' ');
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
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
