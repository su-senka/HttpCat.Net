namespace HttpCat.AspNetCore;

/// <summary>
/// Options controlling when and how responses are replaced with images.
/// </summary>
public class HttpCatOptions
{
    public bool Enabled { get; set; } = true;
    public bool OnlyOnEmptyBody { get; set; } = true;
    public bool OnlyForHtmlAcceptHeader { get; set; } = true;
    public bool AllowJsonResponses { get; set; } = false;
    public Func<int, bool> StatusCodePredicate { get; set; } = code => code >= 400 && code <= 599;
    public string? OptInHeaderName { get; set; }
    public string? OptInQueryKey { get; set; }
}
