using System.Globalization;
using YTFE.Infrastructure;

namespace YTFE.Localization;

/// <summary>
/// Servicio de idioma. Se enlaza desde XAML con {loc:T Clave} y desde código con Loc.T("Clave").
/// Al cambiar de idioma notifica "Item[]", y todos los textos enlazados se refrescan al instante.
/// </summary>
public sealed class Loc : ObservableObject
{
    public static Loc Instance { get; } = new();

    public static readonly string[] Supported = ["es", "en"];

    private string _language = "es";
    private IReadOnlyDictionary<string, string> _table = Strings.Spanish;

    public string Language => _language;
    public CultureInfo Culture { get; private set; } = CultureInfo.GetCultureInfo("es-ES");

    /// <summary>Se dispara tras cambiar de idioma (para recalcular textos construidos en código).</summary>
    public event Action? LanguageChanged;

    /// <summary>Indexador usado por los enlaces de XAML.</summary>
    public string this[string key] => Get(key);

    public string Get(string key) =>
        _table.TryGetValue(key, out var v) ? v
        : Strings.Spanish.TryGetValue(key, out var fallback) ? fallback
        : $"[{key}]"; // clave sin traducir: se nota a simple vista en vez de quedar vacío

    public void SetLanguage(string code)
    {
        code = Supported.Contains(code) ? code : "en";
        if (code == _language && ReferenceEquals(_table, code == "es" ? Strings.Spanish : Strings.English)) return;

        _language = code;
        _table = code == "es" ? Strings.Spanish : Strings.English;
        Culture = CultureInfo.GetCultureInfo(code == "es" ? "es-ES" : "en-US");

        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(Language));
        LanguageChanged?.Invoke();
    }

    /// <summary>Idioma inicial por defecto: español si el Windows está en español, inglés en caso contrario.</summary>
    public static string DetectSystemLanguage() =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es" ? "es" : "en";

    public static string T(string key) => Instance.Get(key);

    public static string T(string key, params object[] args) =>
        string.Format(Instance.Culture, Instance.Get(key), args);
}
