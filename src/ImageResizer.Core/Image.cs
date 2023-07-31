namespace ImageResizer.Core;

public record class Image(byte[] ImageData, ImageFormat ImageFormat, string FileName);