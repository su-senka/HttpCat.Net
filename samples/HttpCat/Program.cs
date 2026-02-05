using HttpCat.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// register HttpCat services and options if needed
builder.Services.AddHttpCats();

var app = builder.Build();

// enable middleware early in the pipeline
app.UseHttpCats();

// sample endpoints to test behavior
app.MapGet("/", () => "OK");

// 2xx Success responses
app.MapGet("/created", () => Results.StatusCode(201));
app.MapGet("/accepted", () => Results.StatusCode(202));
app.MapGet("/no-content", Results.NoContent);

// 3xx Redirection
app.MapGet("/moved-permanently", () => Results.Redirect("/", permanent: true));
app.MapGet("/found", () => Results.StatusCode(302));
app.MapGet("/not-modified", () => Results.StatusCode(304));
app.MapGet("/temporary-redirect", () => Results.StatusCode(307));

// 4xx Client errors
app.MapGet("/bad-request", () => Results.BadRequest());
app.MapGet("/unauthorized", Results.Unauthorized);
app.MapGet("/payment-required", () => Results.StatusCode(402));
app.MapGet("/forbidden", () => Results.StatusCode(403));
app.MapGet("/notfound", () => Results.NotFound());
app.MapGet("/method-not-allowed", () => Results.StatusCode(405));
app.MapGet("/not-acceptable", () => Results.StatusCode(406));
app.MapGet("/request-timeout", () => Results.StatusCode(408));
app.MapGet("/conflict", () => Results.StatusCode(409));
app.MapGet("/gone", () => Results.StatusCode(410));
app.MapGet("/im-a-teapot", () => Results.StatusCode(418));
app.MapGet("/unprocessable-entity", () => Results.StatusCode(422));
app.MapGet("/locked", () => Results.StatusCode(423));
app.MapGet("/too-many-requests", () => Results.StatusCode(429));
app.MapGet("/unavailable-for-legal-reasons", () => Results.StatusCode(451));

// 5xx Server errors
app.MapGet("/server-error", () => Results.StatusCode(500));
app.MapGet("/not-implemented", () => Results.StatusCode(501));
app.MapGet("/bad-gateway", () => Results.StatusCode(502));
app.MapGet("/service-unavailable", () => Results.StatusCode(503));
app.MapGet("/gateway-timeout", () => Results.StatusCode(504));
app.MapGet("/http-version-not-supported", () => Results.StatusCode(505));

app.Run();
