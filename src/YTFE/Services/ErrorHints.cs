using YTFE.Localization;

namespace YTFE.Services;

/// <summary>Traduce los errores típicos de yt-dlp a un mensaje comprensible con la acción sugerida.</summary>
public static class ErrorHints
{
    /// <summary>¿El fallo es que no se pudieron leer las cookies del navegador (no instalado, bloqueadas, cifradas)?</summary>
    public static bool IsCookieReadError(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) &&
        new[] { "cookies database", "cookie database", "could not copy", "decrypt", "dpapi", "cookies from browser", "cookies-from-browser" }
            .Any(n => raw.Contains(n, StringComparison.OrdinalIgnoreCase));

    /// <param name="raw">Texto de error de yt-dlp (en inglés).</param>
    /// <param name="cookiesActive">¿Hay un navegador elegido para las cookies?</param>
    public static string Describe(string? raw, bool cookiesActive)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        bool Has(params string[] needles) =>
            needles.Any(n => raw.Contains(n, StringComparison.OrdinalIgnoreCase));

        // El orden importa: de lo más específico a lo más genérico.
        if (cookiesActive && IsCookieReadError(raw)) return Loc.T("ErrCookiesDb");

        if (Has("drm")) return Loc.T("ErrDrm");
        if (Has("unsupported url")) return Loc.T("ErrUnsupported");
        if (Has("geo restrict", "geo-restrict", "not available in your country", "from your location"))
            return Loc.T("ErrGeo");
        if (Has("sign in", "log in", "login", "logged in", "authentication", "private video", "members-only",
                "confirm your age", "age-restricted", "age restricted", "cookies"))
            return Loc.T(cookiesActive ? "ErrLoginWithCookies" : "ErrLogin");
        if (Has("429", "too many requests", "rate limit", "rate-limit")) return Loc.T("ErrRate");
        if (Has("video unavailable", "has been removed", "no longer available", "does not exist", "404",
                "no video could be found"))
            return Loc.T("ErrUnavailable");
        if (Has("unable to download webpage", "getaddrinfo", "timed out", "connection", "network is unreachable"))
            return Loc.T("ErrNetwork");

        return raw;
    }
}
