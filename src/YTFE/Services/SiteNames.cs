namespace YTFE.Services;

/// <summary>Nombre legible del sitio a partir de la URL (p. ej. "youtu.be" → "YouTube").</summary>
public static class SiteNames
{
    // Dominio base (sin "www.", "m.", etc.) → nombre. Los subdominios se resuelven subiendo por el host.
    private static readonly Dictionary<string, string> Known = new(StringComparer.OrdinalIgnoreCase)
    {
        ["youtube.com"] = "YouTube", ["youtu.be"] = "YouTube", ["youtube-nocookie.com"] = "YouTube",
        ["vimeo.com"] = "Vimeo",
        ["dailymotion.com"] = "Dailymotion", ["dai.ly"] = "Dailymotion",
        ["twitch.tv"] = "Twitch",
        ["tiktok.com"] = "TikTok",
        ["instagram.com"] = "Instagram",
        ["facebook.com"] = "Facebook", ["fb.watch"] = "Facebook",
        ["twitter.com"] = "X", ["x.com"] = "X",
        ["reddit.com"] = "Reddit", ["redd.it"] = "Reddit",
        ["soundcloud.com"] = "SoundCloud",
        ["bandcamp.com"] = "Bandcamp",
        ["mixcloud.com"] = "Mixcloud",
        ["bilibili.com"] = "Bilibili", ["b23.tv"] = "Bilibili",
        ["vk.com"] = "VK",
        ["linkedin.com"] = "LinkedIn",
        ["pinterest.com"] = "Pinterest",
        ["tumblr.com"] = "Tumblr",
        ["imgur.com"] = "Imgur",
        ["streamable.com"] = "Streamable",
        ["archive.org"] = "Archive.org",
        ["ted.com"] = "TED",
        ["rtve.es"] = "RTVE",
        ["bbc.co.uk"] = "BBC", ["bbc.com"] = "BBC",
    };

    /// <summary>Nombres distintos de los sitios conocidos, ordenados.</summary>
    public static IReadOnlyList<string> Featured { get; } =
        Known.Values.Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

    public static string FromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "";
        var host = uri.Host.ToLowerInvariant();

        // Prueba "a.b.c.com", luego "b.c.com", luego "c.com".
        for (var h = host; h.Contains('.'); h = h[(h.IndexOf('.') + 1)..])
            if (Known.TryGetValue(h, out var name)) return name;

        // Desconocido: el propio dominio sin "www." (p. ej. "ejemplo.com").
        return host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;
    }
}
