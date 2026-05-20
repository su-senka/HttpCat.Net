# HttpCat.Net

[![NuGet](https://img.shields.io/nuget/v/HttpCat.AspNetCore.svg)](https://www.nuget.org/packages/HttpCat.AspNetCore)
[![Build](https://github.com/su-senka/HttpCat.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/su-senka/HttpCat.Net/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

ASP.NET Core middleware that swaps error responses for embedded HTTP cat images — without touching the status code or breaking your API contracts.

## Why

- 70+ HTTP status codes covered with embedded JPEGs (no external calls, no CDN dependency)
- Response buffering ensures headers stay intact when interception is skipped
- Browser-first by default: only fires when the client sends `Accept: text/html`
- Opt-in header / query-string overrides for debugging non-browser clients
- Drop-in: two lines of code, zero configuration required

## Install

```bash
dotnet add package HttpCat.AspNetCore
```

## Quickstart

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpCats();

var app = builder.Build();
app.UseHttpCats();

app.MapGet("/not-found", () => Results.NotFound());
app.MapGet("/server-error", () => Results.StatusCode(500));

app.Run();
```

Open `/not-found` in a browser — you get a cat. Your API client sending `Accept: application/json` gets the original response unchanged.

## Options

```csharp
builder.Services.AddHttpCats(options =>
{
    options.Enabled = true;                          // master switch
    options.OnlyOnEmptyBody = true;                  // skip when a body already exists
    options.OnlyForHtmlAcceptHeader = true;          // require Accept: text/html
    options.AllowJsonResponses = false;              // skip application/json responses
    options.StatusCodePredicate = code => code >= 400 && code <= 599;
    options.OptInHeaderName = "X-HttpCat";           // send "true" or "1" to force
    options.OptInQueryKey = "httpcat";               // ?httpcat=true to force
});
```

The opt-in header/query bypasses the HTML and JSON guards — useful when inspecting APIs with `curl` or Postman without faking an `Accept` header.

## Extensibility

Replace either seam via DI before calling `AddHttpCats`:

```csharp
// Custom image source (e.g. pull from disk or a remote URL)
services.AddSingleton<IHttpCatImageProvider, MyImageProvider>();

// Custom interception logic
services.AddSingleton<IHttpCatInterceptionPolicy, MyPolicy>();
```

## Sample

A runnable minimal API lives in [`samples/MinimalApi`](samples/MinimalApi):

```bash
cd samples/MinimalApi
dotnet run
```

Then try `/not-found`, `/server-error`, or `/teapot` from a browser. `/json-error` stays as JSON by default.

## Development

```bash
dotnet build
dotnet test
```

Targets `net10.0` and `net8.0`. Licensed under [MIT](LICENSE.txt).
