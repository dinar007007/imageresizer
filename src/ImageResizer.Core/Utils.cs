namespace ImageResizer.Core;
public static class Utils
{
    public static string ToMimeType(this ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.jpeg => System.Net.Mime.MediaTypeNames.Image.Jpeg,
            ImageFormat.png => "image/png",
            ImageFormat.webp => "image/webp",
            _ => throw new ArgumentException($"{imageFormat} don't have mime type"),
        };
    }

    public static ImageFormat GetImageFormat(string filename)
    {
        var extension = Path.GetExtension(filename);
        if (extension == null)
        {
            throw new ArgumentException($"Incorrect filename {filename}");
        }

        return extension switch
        {
            ".jpeg" or ".jpg" => ImageFormat.jpeg,
            ".png" => ImageFormat.png,
            ".webp" => ImageFormat.webp,
            _ => throw new ArgumentException($"Incorrect extension {extension}"),
        };
    }
}
