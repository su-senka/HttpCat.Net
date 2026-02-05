using Microsoft.AspNetCore.Http;

namespace HttpCat.AspNetCore;

public class DefaultHttpCatInterceptionPolicy : IHttpCatInterceptionPolicy
{
    public bool ShouldIntercept(HttpContext context, byte[] responseBody, HttpCatOptions options)
    {
        if (!options.Enabled)
            return false;

        if (context.Response.HasStarted)
            return false;

        if (context.Request.Method == HttpMethods.Head)
            return false;

        var statusCode = context.Response.StatusCode;
        
        if (statusCode is 204 or 304)
            return false;

        if (!options.StatusCodePredicate(statusCode))
            return false;

        var optInOverride = IsOptInEnabled(context, options);

        if (optInOverride) return true;
        
        if (options.OnlyForHtmlAcceptHeader)
        {
            if (!AcceptsHtml(context.Request))
                return false;
        }

        if (options.OnlyOnEmptyBody)
        {
            if (responseBody.Length > 0)
                return false;
        }

        var contentType = context.Response.ContentType;
        
        if (options.AllowJsonResponses || string.IsNullOrEmpty(contentType)) return true;
        
        return !contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static bool AcceptsHtml(HttpRequest request)
    {
        var accept = request.Headers.Accept.ToString();
        
        return !string.IsNullOrEmpty(accept) && accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOptInEnabled(HttpContext context, HttpCatOptions options)
    {
        // Check header.
        if (!string.IsNullOrEmpty(options.OptInHeaderName))
        {
            var headerValue = context.Request.Headers[options.OptInHeaderName].ToString();
            if (headerValue is "true" or "1")
                return true;
        }

        // Check query parameter.
        if (string.IsNullOrEmpty(options.OptInQueryKey)) return false;
        
        var queryValue = context.Request.Query[options.OptInQueryKey].ToString();
        
        return queryValue is "true" or "1";
    }
}
