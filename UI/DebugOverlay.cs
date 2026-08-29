using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HighFog;

/// <summary>
/// Technical debug overlay toggled with F3.
/// </summary>
public sealed class DebugOverlay
{
    public bool IsVisible { get; set; }
    private float _fpsTimer;
    private int _frameCount;
    private int _currentFps;

    public void Update(float dt)
    {
        _frameCount++;
        _fpsTimer += dt;
        if (_fpsTimer >= 1.0f)
        {
            _currentFps = _frameCount;
            _frameCount = 0;
            _fpsTimer = 0f;
        }
    }

    public void Draw(SpriteBatch spriteBatch, RetroFont font, HighFogGame game, int screenWidth, int screenHeight)
    {
        if (!IsVisible) return;

        var player = game.Player;
        string zone = game.World.IsInBasement(player.Position) ? "POLICE STATION BASEMENT" : "GRAYHAVEN TOWN SQUARE";
        int activeEnemies = game.Enemies.Enemies.Count(e => !e.IsDead);

        string debugInfo = $"[DEBUG MODE - F3 TO CLOSE]\n" +
                           $"FPS: {_currentFps}\n" +
                           $"POS: X={player.Position.X:F1} Y={player.Position.Y:F1} Z={player.Position.Z:F1}\n" +
                           $"ZONE: {zone}\n" +
                           $"HEALTH: {player.Health:F0} / {player.MaxHealth:F0}\n" +
                           $"WEAPON: {(player.HasHandgun ? $"9MM ({player.Handgun.Ammo}/{player.Handgun.ReserveAmmo})" : "UNARMED")}\n" +
                           $"ACTIVE ENEMIES: {activeEnemies}\n" +
                           $"FOG START/END: {game.CurrentFog.FogStart:F1}m / {game.CurrentFog.FogEnd:F1}m\n" +
                           $"OBJ: {game.State.Objective}";

        var size = font.MeasureString(debugInfo, 1.4f);
        font.DrawBox(spriteBatch, new Rectangle(16, 70, (int)size.X + 16, (int)size.Y + 16), new Color(10, 14, 18, 210), new Color(90, 180, 110), 1);
        font.DrawString(spriteBatch, debugInfo, new Vector2(24, 78), new Color(140, 245, 160), 1.4f);
    }
}
