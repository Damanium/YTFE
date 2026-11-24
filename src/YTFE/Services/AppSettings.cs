using System.Text.Json;

namespace YTFE.Services;

/// <summary>Preferencias del usuario, guardadas en %LOCALAPPDATA%\YTFE\settings.json.</summary>
public sealed class AppSettings
{
    public string? Language { get; set; }

    /// <summary>"system", "light" o "dark". null = seguir el tema de Windows.</summary>
    public string? Theme { get; set; }

    /// <summary>Navegador del que yt-dlp toma las cookies (chrome, edge, firefox…). null = ninguno.</summary>
    public string? CookiesBrowser { get; set; }

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YTFE", "settings.json");

    public static AppSettings Load()
    {
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new(); }
        catch { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch { /* si no se puede guardar, solo se pierde la preferencia */ }
    }
}
