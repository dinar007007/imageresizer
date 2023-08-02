using ImageResizer.Core;
using Microsoft.AspNetCore.Mvc;

namespace ImageResizer.Controllers;

[ApiController]
[Route("")]
public class ImageResizerController : ControllerBase
{
    private readonly IImageResizer _imageResizer;
    private readonly IConfiguration _configuration;

    public ImageResizerController(
        IImageResizer imageResizer,
        IConfiguration configuration)
    {
        _imageResizer = imageResizer ?? throw new ArgumentNullException(nameof(imageResizer));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Конвертирование изображение и уменьшение размера по самой большой стороне
    /// </summary>
    /// <param name="src">Ссылка на изображение, спец символы должны быть кодированы в UTF-8 (например методом encodeURIComponent в js)</param>
    /// <param name="imageFormat">Формат изображения, по умолчанию jpg</param>
    /// <param name="size">Размер по самой большой стороне</param>
    /// <param name="quality">Качество изображения, от 1 до 100 для форматов jpg и webp, по умолчанию 75</param>
    /// <returns></returns>
    [HttpGet("optimize")]
    [Produces("image/jpeg", "image/webp", "text/plain")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] string src,
        [FromQuery] ImageFormat imageFormat = ImageFormat.png,
        [FromQuery] int size = 500,
        [FromQuery] int quality = 75)
    {
        if (size <= 0)
        {
            ModelState.AddModelError(nameof(size), "Incorrect size");
        }
        var uri = new Uri(src);
        var splitHostName = uri.Host.Split('.');
        if (splitHostName.Length <= 1 ||
            (splitHostName[^2] + "." + splitHostName[^1] != _configuration.GetValue<string>("AllowedDomain")))
        {
            ModelState.AddModelError(nameof(src), "Incorrect url");
        }
        if (quality < 1 || quality > 100)
        {
            ModelState.AddModelError(nameof(quality), "Incorrect quality value");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var imageParameters = new ImageParameters
        {
            Url = src,
            Size = size,
            ImageFormat = imageFormat,
            Quality = quality,
        };
        var resizedImage = await _imageResizer.Resize(imageParameters);

        return File(resizedImage.ImageData, resizedImage.ImageFormat.ToMimeType(), resizedImage.FileName);
    }
}