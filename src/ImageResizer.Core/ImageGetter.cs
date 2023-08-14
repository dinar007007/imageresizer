using System.Net;
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
        _logger.LogInformation("Start getting image", url);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var httpResponseMessage = await _httpClient.SendAsync(request);
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Http code on {url} not equal 200", url);
        }

        var filename = GetFilename(httpResponseMessage.Content.Headers, url);

        var image = new Image(
            await httpResponseMessage.Content.ReadAsByteArrayAsync(),
            ImageFormat.png,
            filename);

        return image;
    }

    private string GetFilename(HttpContentHeaders httpContentHeader, string url)
    {
        if (httpContentHeader.ContentDisposition is not null && httpContentHeader.ContentDisposition.FileName is not null)
        {
            return Path.GetFileNameWithoutExtension(httpContentHeader.ContentDisposition.FileName);
        }
        try
        {
            httpContentHeader.TryGetValues("Content-Disposition", out var contentDisposition);
            var uri = new Uri(url);
            if (contentDisposition is null || !contentDisposition.Any())
            {
                return Path.GetFileNameWithoutExtension(uri.Segments[^1]);
            }
            var fileName = Uri.UnescapeDataString(contentDisposition.First().Split('\'')[^1]).TrimEnd(';');
            if (fileName == "inline")
            {
                return uri.Segments[^1].ToString();
            }
            return Path.GetFileNameWithoutExtension(fileName);

        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get image name", ex);
        }

        throw new InvalidOperationException("");
    }
}
