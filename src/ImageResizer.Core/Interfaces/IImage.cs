namespace ImageResizer.Core;
public interface IImage
{
    byte[] ImageData { get; }

    string FileName { get; }

    string Extension { get; }
}
