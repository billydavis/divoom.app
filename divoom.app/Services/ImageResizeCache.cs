using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace divoom.app.Services;

internal static class ImageResizeCache
{
    public static async Task<byte[]> GetOrCacheRgbAsync(StorageFile source, int targetSize)
    {
        var cachedPath = GetCachedPath(source.Path, targetSize, "rgb");

        if (File.Exists(cachedPath))
            return await File.ReadAllBytesAsync(cachedPath);

        var rgb = await ResizeToRgbAsync(source, targetSize);
        await File.WriteAllBytesAsync(cachedPath, rgb);
        return rgb;
    }

    public static async Task<byte[]> GetOrCacheJpegAsync(StorageFile source, int targetSize)
    {
        var cachedPath = GetCachedPath(source.Path, targetSize, "jpg");

        if (File.Exists(cachedPath))
            return await File.ReadAllBytesAsync(cachedPath);

        var jpeg = await ResizeToJpegAsync(source, targetSize);
        await File.WriteAllBytesAsync(cachedPath, jpeg);
        return jpeg;
    }

    private static async Task<byte[]> ResizeToRgbAsync(StorageFile source, int size)
    {
        using var stream = await source.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(stream);

        var transform = new BitmapTransform
        {
            ScaledWidth = (uint)size,
            ScaledHeight = (uint)size,
            InterpolationMode = BitmapInterpolationMode.Fant
        };

        var pixelData = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Ignore,
            transform,
            ExifOrientationMode.IgnoreExifOrientation,
            ColorManagementMode.DoNotColorManage);

        var bgra = pixelData.DetachPixelData();

        var rgb = new byte[size * size * 3];
        for (int i = 0, j = 0; i < bgra.Length; i += 4, j += 3)
        {
            rgb[j]     = bgra[i + 2]; // R
            rgb[j + 1] = bgra[i + 1]; // G
            rgb[j + 2] = bgra[i];     // B
        }
        return rgb;
    }

    private static async Task<byte[]> ResizeToJpegAsync(StorageFile source, int size)
    {
        using var inStream = await source.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(inStream);

        var transform = new BitmapTransform
        {
            ScaledWidth = (uint)size,
            ScaledHeight = (uint)size,
            InterpolationMode = BitmapInterpolationMode.Fant
        };

        var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            transform,
            ExifOrientationMode.IgnoreExifOrientation,
            ColorManagementMode.DoNotColorManage);

        using var outStream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outStream);
        encoder.SetSoftwareBitmap(softwareBitmap);
        await encoder.FlushAsync();

        var bytes = new byte[outStream.Size];
        outStream.Seek(0);
        await outStream.ReadAsync(bytes.AsBuffer(), (uint)bytes.Length, InputStreamOptions.None);
        return bytes;
    }

    private static string GetCachedPath(string sourcePath, int size, string ext)
    {
        var cacheDir = EnsureCacheFolder();
        var nameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
        var hash = ComputeHash(sourcePath);
        return Path.Combine(cacheDir, $"{nameWithoutExt}_{hash}_{size}x{size}.{ext}");
    }

    private static string EnsureCacheFolder()
    {
        var path = Path.Combine(
            Windows.Storage.UserDataPaths.GetDefault().LocalAppData,
            "DivoomManager", "Cache");
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..8];
    }
}
