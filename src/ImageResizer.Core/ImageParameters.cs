namespace ImageResizer.Core;

public record class ImageParameters
{
    public string Url { get; init; } = default!;

    public int Size { get; init; }

    public ImageFormat ImageFormat { get; init; }
}
