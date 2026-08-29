using Microsoft.Xna.Framework.Input;

namespace HighFog;

public sealed class InputTracker
{
    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;

    public KeyboardState Keyboard { get; private set; }
    public MouseState Mouse { get; private set; }

    public void Update()
    {
        _previousKeyboard = Keyboard;
        _previousMouse = Mouse;
        Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
    }

    public bool Pressed(Keys key) => Keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    public bool Down(Keys key) => Keyboard.IsKeyDown(key);
    public bool LeftPressed => Mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton != ButtonState.Pressed;
}
