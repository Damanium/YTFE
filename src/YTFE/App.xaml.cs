using System.Windows;
using YTFE.Localization;
using YTFE.Services;

namespace YTFE;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // El idioma debe fijarse antes de crear la ventana principal.
        var settings = AppSettings.Load();
        Loc.Instance.SetLanguage(settings.Language ?? Loc.DetectSystemLanguage());
        Infrastructure.ThemeManager.Apply(settings.Theme);
        base.OnStartup(e);
    }
}
