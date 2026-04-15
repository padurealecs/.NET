using Silk.NET.SDL;

namespace TheAdventure;

public class InputLogic
{
    private readonly Sdl _sdl;
    private readonly GameLogic _gameLogic;
    private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

    public InputLogic(Sdl sdl, GameLogic gameLogic)
    {
        _sdl = sdl;
        _gameLogic = gameLogic;
    }

    public bool ProcessInput()
    {
        ReadOnlySpan<byte> keyboardState;
        unsafe
        {
            keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }
        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];

        int mouseX = 0, mouseY = 0;

        Event ev = new Event();
        while (_sdl.PollEvent(ref ev) != 0)
        {
            if (ev.Type == (uint)EventType.Quit) return true;

            switch (ev.Type)
            {
                case (uint)EventType.Mousebuttondown:
                {
                    mouseX = ev.Motion.X;
                    mouseY = ev.Motion.Y;
                    mouseButtonStates[ev.Button.Button] = 1;
                    break;
                }
                case (uint)EventType.Mousebuttonup:
                    mouseButtonStates[ev.Button.Button] = 0;
                    break;
                case (uint)EventType.Windowevent:
                    switch (ev.Window.Event)
                    {
                        case (byte)WindowEventID.TakeFocus:
                            unsafe
                            {
                                _sdl.SetWindowInputFocus(_sdl.GetWindowFromID(ev.Window.WindowID));
                            }
                            break;
                    }
                    break;
            }
        }

        var currentTime = DateTimeOffset.Now;
        var timeSinceLastFrame = (int)currentTime.Subtract(_lastUpdate).TotalMilliseconds;
        _lastUpdate = currentTime;

        var up = 0.0;
        var down = 0.0;
        var left = 0.0;
        var right = 0.0;

        if (keyboardState[(int)KeyCode.Up] == 1)
        {
            up = 1.0;
        }

        if (keyboardState[(int)KeyCode.Down] == 1)
        {
            down = 1.0;
        }

        if (keyboardState[(int)KeyCode.Left] == 1)
        {
            left = 1.0;
        }

        if (keyboardState[(int)KeyCode.Right] == 1)
        {
            right = 1.0;
        }

        _gameLogic.UpdatePlayerPosition(up, down, left, right, timeSinceLastFrame);

        if (mouseButtonStates[(byte)MouseButton.Primary] == 1)
        {
            var worldCoords = GameRenderer.ToWorldCoordinates(mouseX, mouseY);
            _gameLogic.AddBomb(worldCoords.X, worldCoords.Y);
        }

        return false;
    }
}
