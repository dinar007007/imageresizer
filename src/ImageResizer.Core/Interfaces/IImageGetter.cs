namespace ImageResizer.Core;
public interface IImageGetter
{
    Task<Image> GetImageAsync(string url);
}
