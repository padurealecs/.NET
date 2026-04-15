using Silk.NET.SDL;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;

namespace TheAdventure;

public partial class GameRenderer
{
    private readonly Sdl _sdl;
    private readonly IntPtr _renderer;
    private readonly GameLogic _gameLogic;
    private readonly GameCamera _camera = new();
    private DateTimeOffset _lastFrameRenderedAt = DateTimeOffset.MinValue;

    private static GameRenderer? _instance;

    public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic)
    {
        _sdl = sdl;
        _renderer = gameWindow.CreateRenderer();
        _gameLogic = gameLogic;
        _instance = this;

        _camera.X = 0;
        _camera.Y = 0;
        var windowSize = gameWindow.Size;
        _camera.Width = windowSize.Width;
        _camera.Height = windowSize.Height;
    }

    public void RenderGameObject(RenderableGameObject gameObject)
    {
        unsafe
        {
            var renderer = (Renderer*)_renderer;
            if (gameObject.TextureId > -1 &&
                _texturePointers.TryGetValue(gameObject.TextureId, out var texturePointer))
            {
                var textureDest = _camera.ToScreenCoordinates(gameObject.TextureDestination);
                _sdl.RenderCopyEx(renderer, (Texture*)texturePointer,
                    gameObject.TextureSource, textureDest,
                    0, new Silk.NET.SDL.Point(0, 0), RendererFlip.None);
            }
        }
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst)
    {
        unsafe
        {
            if (_texturePointers.TryGetValue(textureId, out var texture))
            {
                var translatedDst = _camera.ToScreenCoordinates(dst);
                _sdl.RenderCopy((Renderer*)_renderer, (Texture*)texture, in src, in translatedDst);
            }
        }
    }

    public void Render()
    {
        var timeSinceLastFrame = 0;
        var now = DateTimeOffset.UtcNow;
        if (_lastFrameRenderedAt > DateTimeOffset.MinValue)
        {
            timeSinceLastFrame = (int)now.Subtract(_lastFrameRenderedAt).TotalMilliseconds;
        }

        var playerPos = _gameLogic.GetPlayerPosition();
        _camera.X = playerPos.X;
        _camera.Y = playerPos.Y;

        unsafe
        {
            var renderer = (Renderer*)_renderer;
            _sdl.SetRenderDrawColor(renderer, 255, 255, 255, 255);
            _sdl.RenderClear(renderer);
        }

        _gameLogic.RenderTerrain(this);
        _gameLogic.RenderAllObjects(timeSinceLastFrame, this);
        _lastFrameRenderedAt = now;

        unsafe
        {
            _sdl.RenderPresent((Renderer*)_renderer);
        }
    }
}

public partial class GameRenderer
{
    private readonly Dictionary<int, IntPtr> _texturePointers = new();
    private readonly Dictionary<int, TextureData> _textureInformation = new();
    private int _index = 0;

    public static int LoadTexture(string fileName, out TextureData textureData)
    {
        using var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var image = Image.Load<Rgba32>(fStream);
        textureData = new TextureData() { Width = image.Width, Height = image.Height };

        var pixelDataSize = textureData.Width * textureData.Height * 4;
        var imageRawData = new byte[pixelDataSize];
        image.CopyPixelDataTo(imageRawData.AsSpan());

        IntPtr imageTexture;
        unsafe
        {
            fixed (byte* data = imageRawData)
            {
                var imageSurface = _instance!._sdl.CreateRGBSurfaceWithFormatFrom(data,
                    textureData.Width, textureData.Height,
                    8, textureData.Width * 4, (uint)PixelFormatEnum.Rgba32);
                imageTexture = (IntPtr)_instance._sdl.CreateTextureFromSurface(
                    (Renderer*)_instance._renderer, imageSurface);
                _instance._sdl.FreeSurface(imageSurface);
            }
        }

        _instance!._texturePointers[_instance._index] = imageTexture;
        _instance._textureInformation[_instance._index] = textureData;
        return _instance._index++;
    }

    public static (int X, int Y) ToWorldCoordinates(int x, int y)
    {
        if (_instance == null)
        {
            throw new InvalidOperationException("GameRenderer instance is not initialized.");
        }

        var worldCoords = _instance._camera.ToWorldCoordinates(new Vector2D<int>(x, y));
        return (worldCoords.X, worldCoords.Y);
    }
}
