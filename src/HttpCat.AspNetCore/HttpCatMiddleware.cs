using Microsoft.AspNetCore.Http;

namespace HttpCat.AspNetCore;

public class HttpCatMiddleware(
    RequestDelegate next,
    IHttpCatInterceptionPolicy policy,
    IHttpCatImageProvider imageProvider,
    HttpCatOptions options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!options.Enabled || context.Request.Method == HttpMethods.Head)
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await next(context);

            if (context.Response.HasStarted)
            {
                context.Response.Body = originalBody;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBody);
                return;
            }

            var statusCode = context.Response.StatusCode;
            if (statusCode == 204 || statusCode == 304 || !options.StatusCodePredicate(statusCode))
            {
                context.Response.Body = originalBody;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBody);
                return;
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = memoryStream.ToArray();

            if (policy.ShouldIntercept(context, responseBody, options))
            {
                if (imageProvider.TryGetImage(context.Response.StatusCode, out byte[] imageBytes))
                {
                    context.Response.Body = originalBody;

                    context.Response.ContentType = "image/jpeg";
                    context.Response.ContentLength = imageBytes.Length;

                    await context.Response.Body.WriteAsync(imageBytes);
                    return;
                }
            }

            context.Response.Body = originalBody;
            if (responseBody.Length > 0)
            {
                await context.Response.Body.WriteAsync(responseBody);
            }
        }
        catch
        {
            // Restore original body and rethrow.
            context.Response.Body = originalBody;
            throw;
        }
    }
}
