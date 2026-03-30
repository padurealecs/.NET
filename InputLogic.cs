using Silk.NET.SDL;

namespace TheAdventure;

public class InputLogic
{
    private readonly Sdl _sdl;
    private readonly GameLogic _gameLogic;

    public InputLogic(Sdl sdl, GameLogic gameLogic)
    {
        _sdl = sdl;
        _gameLogic = gameLogic;
    }

    public bool ProcessInput()
    {
        ReadOnlySpan<byte> _keyboardState;
        unsafe
        {
            _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
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

        if (mouseButtonStates[(byte)MouseButton.Primary] == 1)
        {
            _gameLogic.AddBomb(mouseX, mouseY);
        }

        return false;
    }
}
