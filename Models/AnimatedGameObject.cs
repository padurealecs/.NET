using Silk.NET.Maths;

namespace TheAdventure.Models;

public class AnimatedGameObject : RenderableGameObject
{
    private readonly int _numberOfFrames;
    private readonly int _numberOfColumns;
    private readonly int _columnWidth;
    private readonly int _rowHeight;
    private readonly int _durationInSeconds;
    private readonly int _timePerFrame;
    private int _timeSinceAnimationStart = 0;
    private int _currentRow = 0;
    private int _currentColumn = 0;

    public AnimatedGameObject(string fileName, int durationInSeconds, int id,
        int numberOfFrames, int numberOfColumns, int numberOfRows, int x, int y)
        : base(fileName, id)
    {
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _durationInSeconds = durationInSeconds;
        _timePerFrame = (durationInSeconds * 1000) / _numberOfFrames;

        _columnWidth = TextureInformation.Width / numberOfColumns;
        _rowHeight = TextureInformation.Height / numberOfRows;

        // center the sprite on the spawn point
        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;
        TextureDestination = new Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        TextureSource = new Rectangle<int>(0, 0, _columnWidth, _rowHeight);
    }

    public override bool Update(int timeSinceLastFrame)
    {
        _timeSinceAnimationStart += timeSinceLastFrame;

        if (_timeSinceAnimationStart > _durationInSeconds * 1000) return false; // animation complete, remove object

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;
        _currentRow = currentFrame / _numberOfColumns;
        _currentColumn = currentFrame % _numberOfColumns;

        TextureSource = new Rectangle<int>(
            _currentColumn * _columnWidth, _currentRow * _rowHeight,
            _columnWidth, _rowHeight);

        return true;
    }
}
