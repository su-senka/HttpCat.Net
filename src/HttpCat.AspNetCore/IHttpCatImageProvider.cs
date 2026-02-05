namespace HttpCat.AspNetCore;

/// <summary>
/// Provides HTTP cat images for status codes.
/// </summary>
public interface IHttpCatImageProvider
{
    /// <summary>
    /// Try to get an image for the given status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="imageBytes">The image bytes if found.</param>
    /// <returns>True if an image exists for this status code; false otherwise.</returns>
    bool TryGetImage(int statusCode, out byte[] imageBytes);
}
