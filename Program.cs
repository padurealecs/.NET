using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer |
                                     Sdl.InitGamecontroller | Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        var gameWindow = new GameWindow(sdl);
        var gameLogic = new GameLogic();
        var gameRenderer = new GameRenderer(sdl, gameWindow, gameLogic);
        var inputLogic = new InputLogic(sdl, gameLogic);

        gameLogic.InitializeGame(gameRenderer);

        bool quit = false;
        while (!quit)
        {
            quit = inputLogic.ProcessInput();
            if (quit) break;
            gameLogic.ProcessFrame();

            gameRenderer.Render();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.016666666666666666));
        }

        gameWindow.Destroy();
        sdl.Quit();
    }
}
