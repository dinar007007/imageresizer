namespace ImageResizer.Core;
public interface IImageResizer
{
    Task<Image> Resize(ImageParameters imageParameters, CancellationToken cancellationToken = default);
}
