# HttpCat.AspNetCore

ASP.NET Core middleware that swaps error responses for embedded HTTP cat images without changing the status code or breaking API contracts.

## Why
- Ships with embedded JPEGs for common HTTP status codes
- Response buffering keeps headers safe and untouched when not intercepting
- Opt-in hooks via header or query for debugging scenarios
- Works with browsers by default (requires `text/html` Accept unless opt-in is used)

## Install

```bash
dotnet add package HttpCat.AspNetCore
```

## Use

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpCats();

var app = builder.Build();
app.UseHttpCats();

app.MapGet("/notfound", () => Results.NotFound());
app.MapGet("/server-error", () => Results.StatusCode(500));

app.Run();
```

## Options

```csharp
builder.Services.AddHttpCats(options =>
{
    options.OnlyOnEmptyBody = true;                 // skip when a body already exists
    options.OnlyForHtmlAcceptHeader = true;          // require Accept: text/html
    options.AllowJsonResponses = false;              // ignore JSON unless enabled
    options.StatusCodePredicate = code => code >= 400 && code <= 599;
    options.OptInHeaderName = "X-HttpCat";           // set "true" or "1" to force
    options.OptInQueryKey = "httpcat";               // ?httpcat=true to force
});
```

The opt-in header/query bypasses the HTML and JSON checks, which is useful when testing APIs with non-browser clients.

## Sample

Run the minimal API in [samples/MinimalApi](samples/MinimalApi):

```bash
cd samples/MinimalApi
dotnet run
```

Try `/notfound` or `/server-error` from a browser to see cats. `/json-error` stays JSON by default.

## Development

- Build: `dotnet build`
- Tests: `dotnet test`

Targets `net10.0` and `net8.0`. Licensed under MIT.
