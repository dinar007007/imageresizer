using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace ImageResizer.Core;
public class ImageGetter : IImageGetter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageGetter> _logger;

    public ImageGetter(
        HttpClient httpClient,
        ILogger<ImageGetter> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Image> GetImageAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var httpResponseMessage = await _httpClient.SendAsync(request);
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Http code on {url} not equal 200", url);
        }

        var (filename, imageFormat) = GetFilename(httpResponseMessage.Content.Headers, url);

        var image = new Image(
            await httpResponseMessage.Content.ReadAsByteArrayAsync(),
            imageFormat,
            filename);

        return image;
    }

    private (string, ImageFormat) GetFilename(HttpContentHeaders httpContentHeader, string url)
    {
        if (httpContentHeader.ContentDisposition is not null && httpContentHeader.ContentDisposition.FileName is not null)
        {
            return (Path.GetFileNameWithoutExtension(httpContentHeader.ContentDisposition.FileName),
                    Utils.GetImageFormat(httpContentHeader.ContentDisposition.FileName));
        }
        try
        {
            httpContentHeader.TryGetValues("Content-Disposition", out var contentDisposition);
            if (contentDisposition is null || !contentDisposition.Any())
            {
                var uri = new Uri(url);
                return (Path.GetFileNameWithoutExtension(uri.Segments[^1]),
                    Utils.GetImageFormat(uri.Segments[^1]));
            }
            var fileName = Uri.UnescapeDataString(contentDisposition.First().Split('\'')[^1]).TrimEnd(';');
            return (Path.GetFileNameWithoutExtension(fileName),
                    Utils.GetImageFormat(fileName));

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get image name", ex);
        }

        throw new InvalidOperationException("");
    }
}
