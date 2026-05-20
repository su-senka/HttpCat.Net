using System.Collections.Concurrent;

namespace HttpCat.Net;

/// <summary>
/// Provides HTTP cat images from embedded assembly resources.
/// </summary>
public class EmbeddedResourceHttpCatImageProvider : IHttpCatImageProvider
{
    private readonly ConcurrentDictionary<int, byte[]> _cache = new();

    public bool TryGetImage(int statusCode, out byte[] imageBytes)
    {
        if (_cache.TryGetValue(statusCode, out var cached))
        {
            imageBytes = cached;
            return true;
        }

        var assembly = typeof(EmbeddedResourceHttpCatImageProvider).Assembly;
        var resourceName = $"HttpCat.Net.Assets.{statusCode}.jpg";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            imageBytes = [];
            return false;
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();

        _cache[statusCode] = bytes;
        imageBytes = bytes;
        return true;
    }
}
