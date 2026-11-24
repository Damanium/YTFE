using System.Windows;
using Microsoft.Win32;

#pragma warning disable WPF0001 // ThemeMode (tema Fluent de WPF) está marcado como experimental
namespace YTFE.Infrastructure;

/// <summary>Cambia entre tema claro, oscuro o el del sistema, en caliente.</summary>
public static class ThemeManager
{
    private static ResourceDictionary? _colors;
    private static string _mode = "system";
    private static bool _watching;

    /// <param name="mode">"light", "dark" o "system" (cualquier otro valor = system).</param>
    public static void Apply(string? mode)
    {
        _mode = mode is "light" or "dark" ? mode : "system";
        var app = Application.Current;

        // Fluent (WPF) pinta los controles estándar y la barra de título; nuestros colores van aparte.
        app.ThemeMode = _mode switch
        {
            "dark" => ThemeMode.Dark,
            "light" => ThemeMode.Light,
            _ => ThemeMode.System,
        };

        var dark = _mode == "dark" || (_mode == "system" && SystemUsesDarkTheme());
        var dicts = app.Resources.MergedDictionaries;
        if (_colors is not null) dicts.Remove(_colors);
        _colors = new ResourceDictionary
        {
            Source = new Uri(dark ? "Themes/Dark.xaml" : "Themes/Light.xaml", UriKind.Relative),
        };
        dicts.Add(_colors);

        if (!_watching)
        {
            _watching = true;
            // Si el usuario cambia el tema de Windows mientras la app está en "Sistema", se sigue.
            SystemEvents.UserPreferenceChanged += (_, e) =>
            {
                if (_mode == "system" && e.Category == UserPreferenceCategory.General)
                    app.Dispatcher.BeginInvoke(() => Apply("system"));
            };
        }
    }

    private static bool SystemUsesDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }
        catch { return false; }
    }
}
