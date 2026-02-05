using Microsoft.AspNetCore.Http;

namespace HttpCat.AspNetCore;

/// <summary>
/// Decides whether a specific request/response pair should be intercepted.
/// </summary>
public interface IHttpCatInterceptionPolicy
{
    /// <summary>
    /// Determine if this response should be intercepted and replaced with an HttpCat image.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="responseBody">The buffered response body.</param>
    /// <param name="options">The HttpCat options.</param>
    /// <returns>True if the response should be intercepted; false otherwise.</returns>
    bool ShouldIntercept(HttpContext context, byte[] responseBody, HttpCatOptions options);
}
