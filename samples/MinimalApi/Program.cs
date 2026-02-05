using HttpCat.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpCats();

var app = builder.Build();

app.UseHttpCats();

app.MapGet("/", () => Results.Ok(new { message = "Hello, HttpCat!" }));

app.MapGet("/notfound", () => Results.NotFound());

app.MapGet("/server-error", () => Results.StatusCode(500));

app.MapGet("/json-error", () => Results.BadRequest(new { error = "Ignored by default" }));

app.Run();
