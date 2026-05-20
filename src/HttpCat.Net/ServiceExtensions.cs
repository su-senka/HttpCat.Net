using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HttpCat.Net;

/// <summary>
/// Extension methods for registering the HttpCat.Net middleware.
/// </summary>
public static class HttpCatServiceCollectionExtensions
{
    /// <summary>
    /// Add HttpCat.Net middleware services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure HttpCatOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpCats(
        this IServiceCollection services,
        Action<HttpCatOptions>? configure = null)
    {
        var options = new HttpCatOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IHttpCatInterceptionPolicy, DefaultHttpCatInterceptionPolicy>();
        services.AddSingleton<IHttpCatImageProvider, EmbeddedResourceHttpCatImageProvider>();

        return services;
    }
}

/// <summary>
/// Extension methods for using the HttpCat.Net middleware.
/// </summary>
public static class HttpCatApplicationBuilderExtensions
{
    /// <summary>
    /// Use the HttpCat.Net middleware in the request pipeline.
    /// Should be placed early in the pipeline to intercept responses before other middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseHttpCats(this IApplicationBuilder app)
    {
        app.UseMiddleware<HttpCatMiddleware>();
        return app;
    }
}
