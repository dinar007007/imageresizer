using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageResizer.Core;
public class ImageResizer : IImageResizer
{
    private readonly IImageGetter _imageGetter;
    private readonly ILogger<ImageGetter> _logger;

    public ImageResizer(
        IImageGetter imageGetter,
        ILogger<ImageGetter> logger)
    {
        _imageGetter = imageGetter ?? throw new ArgumentNullException(nameof(imageGetter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Image> Resize(ImageParameters imageParameters, CancellationToken cancellationToken = default)
    {
        var ImageToResize = await _imageGetter.GetImageAsync(imageParameters.Url);
        var image = SixLabors.ImageSharp.Image.Load(ImageToResize.ImageData);
        var width = image.Width;
        var height = image.Height;
        var maxSize = Math.Max(width, height);
        if (imageParameters.Size <= maxSize)
        {
            var ratio = (double)imageParameters.Size / maxSize;
            image.Mutate(x => x.Resize((int)(width * ratio), (int)(height * ratio)));
        }
        using var imageStream = new MemoryStream();
        var imageEncoder = GetImageEncoder(imageParameters.ImageFormat);
        await image.SaveAsync(imageStream, imageEncoder, cancellationToken);

        var extension = GetExtension(imageParameters.ImageFormat);
        var resizedImage = new Image(imageStream.ToArray(), imageParameters.ImageFormat, ImageToResize.FileName);
        _logger.LogInformation(
            "Image from url {url} resized to format {format} with size {size}",
            imageParameters.Url,
            imageParameters.ImageFormat,
            imageParameters.Size);

        return resizedImage;
    }

    private static string GetExtension(ImageFormat imageFormat)
    {
        return imageFormat.ToString();
    }

    private IImageEncoder GetImageEncoder(ImageFormat imageFormat)
    {
        switch (imageFormat)
        {
            case ImageFormat.jpeg:
                return new JpegEncoder();
            case ImageFormat.png:
                return new PngEncoder();
            case ImageFormat.webp:
                return new WebpEncoder();
            default:
                _logger.LogError("{imageFormat} does not have a suitable encoder", imageFormat);
                throw new ArgumentException($"{imageFormat} does not have a suitable encoder");
        }
    }
}
