using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Minimalist N64 / PS1 survival horror in-game HUD.
/// </summary>
public sealed class HUD
{
    public void Draw(SpriteBatch spriteBatch, RetroFont font, HighFogGame game, int screenWidth, int screenHeight)
    {
        var player = game.Player;

        // 1. Health Bar (Top Left)
        int barWidth = 160;
        int barHeight = 16;
        int barX = 24;
        int barY = 24;

        font.DrawBox(spriteBatch, new Rectangle(barX - 4, barY - 4, barWidth + 8, barHeight + 8), new Color(15, 20, 22, 210), new Color(90, 100, 105), 2);
        
        float hpPct = Math.Clamp(player.Health / player.MaxHealth, 0f, 1f);
        Color hpColor = hpPct > 0.6f ? new Color(60, 200, 80) : (hpPct > 0.25f ? new Color(225, 185, 40) : new Color(230, 45, 40));
        
        font.DrawBox(spriteBatch, new Rectangle(barX, barY, (int)(barWidth * hpPct), barHeight), hpColor, Color.Transparent, 0);
        font.DrawString(spriteBatch, "HP", new Vector2(barX + 6, barY + 2), Color.White, 1.5f);

        // 2. Ammo Counter (Bottom Right)
        if (player.HasHandgun)
        {
            string ammoText = player.Handgun.IsReloading 
                ? "RELOADING..." 
                : $"AMMO: {player.Handgun.Ammo} / {player.Handgun.ReserveAmmo}";

            var ammoSize = font.MeasureString(ammoText, 2f);
            var ammoPos = new Vector2(screenWidth - ammoSize.X - 28, screenHeight - ammoSize.Y - 24);
            
            font.DrawBox(spriteBatch, new Rectangle((int)ammoPos.X - 10, (int)ammoPos.Y - 6, (int)ammoSize.X + 20, (int)ammoSize.Y + 12), new Color(15, 20, 22, 200), new Color(90, 100, 105), 2);
            font.DrawString(spriteBatch, ammoText, ammoPos, new Color(235, 225, 195), 2f);
        }

        // 3. Current Objective Banner (Top Center)
        if (!string.IsNullOrEmpty(game.State.Objective))
        {
            string objText = $"OBJECTIVE: {game.State.Objective}";
            var objSize = font.MeasureString(objText, 1.7f);
            var objPos = new Vector2(screenWidth * 0.5f - objSize.X * 0.5f, 24);

            font.DrawBox(spriteBatch, new Rectangle((int)objPos.X - 12, (int)objPos.Y - 4, (int)objSize.X + 24, (int)objSize.Y + 8), new Color(15, 20, 22, 190), new Color(85, 110, 115), 1);
            font.DrawString(spriteBatch, objText, objPos, new Color(185, 220, 230), 1.7f);
        }

        // 4. Interaction Prompt (Center Bottom)
        var interactable = game.Interactions.CurrentTarget;
        if (interactable != null && interactable.IsAvailable)
        {
            string promptText = $"[E] {interactable.GetInteractionText(game)}";
            var promptSize = font.MeasureString(promptText, 2f);
            var promptPos = new Vector2(screenWidth * 0.5f - promptSize.X * 0.5f, screenHeight - 110);

            font.DrawBox(spriteBatch, new Rectangle((int)promptPos.X - 14, (int)promptPos.Y - 6, (int)promptSize.X + 28, (int)promptSize.Y + 12), new Color(12, 16, 18, 220), new Color(220, 190, 80), 2);
            font.DrawString(spriteBatch, promptText, promptPos, new Color(255, 235, 130), 2f);
        }

        // 5. Aim Reticle (Screen Center when Aiming)
        if (player.IsAiming)
        {
            int cx = screenWidth / 2;
            int cy = screenHeight / 2;
            Color reticleColor = new(240, 240, 240, 200);
            
            // Draw four cross lines
            font.DrawBox(spriteBatch, new Rectangle(cx - 14, cy - 1, 8, 2), reticleColor, Color.Transparent, 0);
            font.DrawBox(spriteBatch, new Rectangle(cx + 6, cy - 1, 8, 2), reticleColor, Color.Transparent, 0);
            font.DrawBox(spriteBatch, new Rectangle(cx - 1, cy - 14, 2, 8), reticleColor, Color.Transparent, 0);
            font.DrawBox(spriteBatch, new Rectangle(cx - 1, cy + 6, 2, 8), reticleColor, Color.Transparent, 0);
            font.DrawBox(spriteBatch, new Rectangle(cx - 1, cy - 1, 2, 2), new Color(255, 60, 40), Color.Transparent, 0);
        }

        // 6. Toast Notification Banner
        if (game.ToastTimer > 0f && !string.IsNullOrEmpty(game.ToastMessage))
        {
            float alpha = Math.Clamp(game.ToastTimer / 0.5f, 0f, 1f);
            var toastSize = font.MeasureString(game.ToastMessage, 1.8f);
            var toastPos = new Vector2(screenWidth * 0.5f - toastSize.X * 0.5f, 72);

            font.DrawBox(spriteBatch, new Rectangle((int)toastPos.X - 12, (int)toastPos.Y - 4, (int)toastSize.X + 24, (int)toastSize.Y + 8), new Color(18, 24, 28, (int)(220 * alpha)), new Color(120, 150, 160, (int)(255 * alpha)), 2);
            font.DrawString(spriteBatch, game.ToastMessage, toastPos, new Color(255, 255, 220, (int)(255 * alpha)), 1.8f);
        }
    }
}
