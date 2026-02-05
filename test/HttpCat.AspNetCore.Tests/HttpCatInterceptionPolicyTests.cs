using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpCat.AspNetCore.Tests;

public class HttpCatInterceptionPolicyTests
{
    [Fact]
    public void ShouldNotIntercept_WhenDisabled()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions { Enabled = false };
        var httpContext = CreateHttpContext();
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_HeadRequest()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions { Enabled = true };
        var httpContext = CreateHttpContext(method: "HEAD");
        httpContext.Response.StatusCode = 404;
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_Status204()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions { Enabled = true };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 204;
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_Status304()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions { Enabled = true };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 304;
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_StatusCodeNotInPredicate()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions { Enabled = true };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 200; // Success, not in default 4xx-5xx range
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_NoHtmlAcceptHeader()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            OnlyForHtmlAcceptHeader = true
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 404;
        httpContext.Request.Headers["Accept"] = "application/json";
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIntercept_HtmlAcceptHeader()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            OnlyForHtmlAcceptHeader = true
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 404;
        httpContext.Request.Headers["Accept"] = "text/html";
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldNotIntercept_NonEmptyBodyWhenRequired()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            OnlyOnEmptyBody = true
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 404;
        httpContext.Request.Headers["Accept"] = "text/html";
        var responseBody = new byte[] { 1, 2, 3 };

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldNotIntercept_JsonContentTypeByDefault()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            AllowJsonResponses = false
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 400;
        httpContext.Response.ContentType = "application/json";
        httpContext.Request.Headers["Accept"] = "text/html";
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIntercept_JsonContentTypeWhenAllowed()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            AllowJsonResponses = true
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 400;
        httpContext.Response.ContentType = "application/json";
        httpContext.Request.Headers["Accept"] = "text/html";
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldIntercept_WithOptInHeader()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            OptInHeaderName = "X-HttpCat",
            OnlyForHtmlAcceptHeader = true
        };
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = 404;
        httpContext.Request.Headers["X-HttpCat"] = "true";
        // Note: No Accept header, but opt-in should bypass this check
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldIntercept_WithOptInQuery()
    {
        // Arrange
        var policy = new DefaultHttpCatInterceptionPolicy();
        var options = new HttpCatOptions
        {
            Enabled = true,
            OptInQueryKey = "httpcat",
            OnlyForHtmlAcceptHeader = true
        };
        var httpContext = CreateHttpContext(queryString: "?httpcat=true");
        httpContext.Response.StatusCode = 404;
        var responseBody = Array.Empty<byte>();

        // Act
        var result = policy.ShouldIntercept(httpContext, responseBody, options);

        // Assert
        Assert.True(result);
    }

    private HttpContext CreateHttpContext(string method = "GET", string queryString = "")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = "/test";
        if (!string.IsNullOrEmpty(queryString))
        {
            httpContext.Request.QueryString = new QueryString(queryString);
        }

        return httpContext;
    }
}
