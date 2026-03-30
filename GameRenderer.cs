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
    private DateTimeOffset _lastFrameRenderedAt = DateTimeOffset.MinValue;

    private static GameRenderer? _instance;

    public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic)
    {
        _sdl = sdl;
        _renderer = gameWindow.CreateRenderer();
        _gameLogic = gameLogic;
        _instance = this;
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst)
    {
        unsafe
        {
            if (_texturePointers.TryGetValue(textureId, out var texture))
            {
                _sdl.RenderCopy((Renderer*)_renderer, (Texture*)texture, in src, in dst);
            }
        }
    }

    public void RenderGameObject(RenderableGameObject renderableGameObject)
    {
        unsafe
        {
            var renderer = (Renderer*)_renderer;
            if (renderableGameObject.TextureId > -1 &&
                _texturePointers.TryGetValue(renderableGameObject.TextureId, out var texturePointer))
            {
                _sdl.RenderCopyEx(renderer, (Texture*)texturePointer,
                    renderableGameObject.TextureSource, renderableGameObject.TextureDestination,
                    0, new Silk.NET.SDL.Point(0, 0), RendererFlip.None);
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
}
