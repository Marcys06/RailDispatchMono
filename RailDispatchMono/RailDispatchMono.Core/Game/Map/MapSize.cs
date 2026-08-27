using System;
namespace RailDispatchMono.Core.Game.Map;

public readonly record struct MapSize
{
    public const int MaxWidth = 16384;
    public const int MaxHeight = 16384;

    public int Width { get; }
    public int Height { get; }

    public MapSize(int width, int height)
    {
        if (width <= 0 || width > MaxWidth)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0 || height > MaxHeight)
            throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
    }
}




