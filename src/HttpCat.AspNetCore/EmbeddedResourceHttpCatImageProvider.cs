namespace HttpCat.AspNetCore;

/// <summary>
/// Provides HTTP cat images from embedded assembly resources.
/// Looks for .jpg files in the HttpCat.AspNetCore assembly under the Assets folder.
/// </summary>
public class EmbeddedResourceHttpCatImageProvider : IHttpCatImageProvider
{
    private readonly Dictionary<int, byte[]> _cache = new();
    private readonly object _cacheLock = new();

    public bool TryGetImage(int statusCode, out byte[] imageBytes)
    {
        // Check cache first.
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(statusCode, out var cached))
            {
                imageBytes = cached;
                return true;
            }
        }

        // Try to load from embedded resources.
        var assembly = typeof(EmbeddedResourceHttpCatImageProvider).Assembly;
        string resourceName = $"HttpCat.AspNetCore.Assets.{statusCode}.jpg";

        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                imageBytes = Array.Empty<byte>();
                return false;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            // Cache it for future requests.
            lock (_cacheLock)
            {
                _cache[statusCode] = bytes;
            }

            imageBytes = bytes;
            return true;
        }
        catch
        {
            imageBytes = Array.Empty<byte>();
            return false;
        }
    }
}
