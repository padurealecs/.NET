using Silk.NET.SDL;

namespace TheAdventure;

public class InputLogic
{
    private readonly Sdl _sdl;

    public InputLogic(Sdl sdl)
    {
        _sdl = sdl;
    }

    public bool ProcessInput()
    {
        ReadOnlySpan<byte> _keyboardState;
        unsafe
        {
            _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }
        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];

        Event ev = new Event();
        while (_sdl.PollEvent(ref ev) != 0)
        {
            if (ev.Type == (uint)EventType.Quit) return true;

            switch (ev.Type)
            {
                case (uint)EventType.Mousebuttondown:
                    mouseButtonStates[ev.Button.Button] = 1;
                    break;
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

        return false;
    }
}
