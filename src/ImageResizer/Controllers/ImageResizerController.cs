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

    [HttpGet("optimize")]
    [Produces("image/jpeg", "image/webp", "text/plain")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] string src,
        [FromQuery] ImageFormat imageFormat = ImageFormat.png,
        [FromQuery] int size = 500)
    {
        if (size <= 0)
        {
            ModelState.AddModelError(nameof(size), "Incorrect size");
        }
        var uri = new Uri(src);
        var splitHostName = uri.Host.Split('.');
        var t = _configuration.GetValue<string>("AllowedDomain");
        if (splitHostName.Length <= 1 ||
            (splitHostName[^2] + "." + splitHostName[^1] != _configuration.GetValue<string>("AllowedDomain")))
        {
            ModelState.AddModelError(nameof(src), "Incorrect url");
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
        };
        var resizedImage = await _imageResizer.Resize(imageParameters);

        return File(resizedImage.ImageData, resizedImage.ImageFormat.ToMimeType(), resizedImage.FileName);
    }
}