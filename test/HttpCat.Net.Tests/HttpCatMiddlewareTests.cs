using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpCat.Net.Tests;

public class EmbeddedResourceHttpCatImageProviderTests
{
    [Fact]
    public void TryGetImage_Returns404Image()
    {
        // Arrange
        var provider = new EmbeddedResourceHttpCatImageProvider();

        // Act
        var result = provider.TryGetImage(404, out var imageBytes);

        // Assert
        Assert.True(result);
        Assert.NotEmpty(imageBytes);
        Assert.True(imageBytes.Length > 0);
    }

    [Fact]
    public void TryGetImage_Returns500Image()
    {
        // Arrange
        var provider = new EmbeddedResourceHttpCatImageProvider();

        // Act
        var result = provider.TryGetImage(500, out var imageBytes);

        // Assert
        Assert.True(result);
        Assert.NotEmpty(imageBytes);
    }

    [Fact]
    public void TryGetImage_Returns403Image()
    {
        // Arrange
        var provider = new EmbeddedResourceHttpCatImageProvider();

        // Act
        var result = provider.TryGetImage(403, out var imageBytes);

        // Assert
        Assert.True(result);
        Assert.NotEmpty(imageBytes);
    }

    [Fact]
    public void TryGetImage_ReturnsFalseForMissingImage()
    {
        // Arrange
        var provider = new EmbeddedResourceHttpCatImageProvider();

        // Act
        var result = provider.TryGetImage(600, out var imageBytes);

        // Assert
        Assert.False(result);
        Assert.Empty(imageBytes);
    }

    [Fact]
    public void TryGetImage_CachesResults()
    {
        // Arrange
        var provider = new EmbeddedResourceHttpCatImageProvider();

        // Act
        var result1 = provider.TryGetImage(404, out var imageBytes1);
        var result2 = provider.TryGetImage(404, out var imageBytes2);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.Equal(imageBytes1, imageBytes2);
    }
}

public class HttpCatMiddlewareIntegrationTests
{
    [Fact]
    public async Task Middleware_InterceptsStatusCodeSetByEndpoint_503()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Accept"] = "text/html";
        
        var options = new HttpCatOptions();
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                // Simulate endpoint setting status code AFTER middleware buffering starts
                ctx.Response.StatusCode = 503;
                await Task.CompletedTask;
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("image/jpeg", context.Response.ContentType);
        Assert.True(context.Response.ContentLength > 0);
    }

    [Fact]
    public async Task Middleware_InterceptsStatusCodeSetByEndpoint_404()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Accept"] = "text/html";
        
        var options = new HttpCatOptions();
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = 404;
                await Task.CompletedTask;
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal("image/jpeg", context.Response.ContentType);
    }

    [Fact]
    public async Task Middleware_PassesThrough_WhenStatusIs200()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Accept"] = "text/html";
        
        var options = new HttpCatOptions();
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync("OK");
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotEqual("image/jpeg", context.Response.ContentType);
    }

    [Fact]
    public async Task Middleware_PassesThrough_WhenNoHtmlAccept()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Accept"] = "application/json";
        
        var options = new HttpCatOptions();
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = 500;
                await Task.CompletedTask;
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        Assert.NotEqual("image/jpeg", context.Response.ContentType);
    }

    [Fact]
    public async Task Middleware_PassesThrough_HeadRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Method = "HEAD";
        context.Request.Headers["Accept"] = "text/html";
        
        var options = new HttpCatOptions();
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = 404;
                await Task.CompletedTask;
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
        Assert.NotEqual("image/jpeg", context.Response.ContentType);
    }

    [Fact]
    public async Task Middleware_RespectsOptInHeader_WhenAcceptIsNotHtml()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Accept"] = "application/json";
        context.Request.Headers["X-HttpCat.Net"] = "true";
        
        var options = new HttpCatOptions { OptInHeaderName = "X-HttpCat.Net" };
        var policy = new DefaultHttpCatInterceptionPolicy();
        var provider = new EmbeddedResourceHttpCatImageProvider();
        var middleware = new HttpCatMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = 404;
                await Task.CompletedTask;
            },
            policy,
            provider,
            options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal("image/jpeg", context.Response.ContentType);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
