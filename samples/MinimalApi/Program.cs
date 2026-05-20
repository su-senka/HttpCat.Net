using HttpCat.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpCats(options =>
{
    options.OptInHeaderName = "X-HttpCat";
    options.OptInQueryKey = "httpcat";
});

var app = builder.Build();

app.UseHttpCats();

app.MapGet("/", () => "Hello! Try /not-found, /server-error, or /json-error");
app.MapGet("/not-found", () => Results.NotFound());
app.MapGet("/server-error", () => Results.StatusCode(500));
app.MapGet("/json-error", () => Results.Problem("Something went wrong", statusCode: 500));
app.MapGet("/teapot", () => Results.StatusCode(418));

app.Run();
