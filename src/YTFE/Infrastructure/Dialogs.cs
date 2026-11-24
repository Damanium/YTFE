using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using YTFE.Localization;
using YTFE.Services;

namespace YTFE.Infrastructure;

public static class Dialogs
{
    /// <summary>Pregunta Sí/No. La respuesta por defecto es "No" para que Enter no confirme por accidente.</summary>
    public static bool Confirm(string message) =>
        MessageBox.Show(message, "YTFE", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
            == MessageBoxResult.Yes;

    public static void Error(string message) =>
        MessageBox.Show(message, "YTFE", MessageBoxButton.OK, MessageBoxImage.Warning);

    private static HelpWindow? _help;

    /// <summary>Abre la ayuda (una sola ventana); si ya está abierta, la trae al frente.</summary>
    public static void ShowHelp(string? topic = null)
    {
        if (_help is not null)
        {
            if (topic is not null) _help.ShowTopic(topic);
            _help.Activate();
            return;
        }
        _help = new HelpWindow(topic) { Owner = Application.Current.MainWindow };
        _help.Closed += (_, _) => _help = null;
        _help.Show();
    }

    private const string FullListUrl = "https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md";

    /// <summary>Ventana con los sitios más conocidos (con su logo) y un enlace a la lista completa de yt-dlp.</summary>
    public static void ShowSites()
    {
        var tiles = new WrapPanel { Margin = new Thickness(0, 6, 0, 6) };
        foreach (var n in SiteNames.Featured)
        {
            var name = new TextBlock
            {
                Text = n, FontSize = 12, TextAlignment = TextAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 8, 0, 0),
            };
            var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            var logo = SiteLogoView.Create(n, 46);
            logo.HorizontalAlignment = HorizontalAlignment.Center;
            content.Children.Add(logo);
            content.Children.Add(name);

            var card = new Border
            {
                Width = 104, Margin = new Thickness(0, 0, 8, 8), Padding = new Thickness(8, 14, 8, 12),
                CornerRadius = new CornerRadius(12), BorderThickness = new Thickness(1), Child = content,
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "CardBorderBrush");
            tiles.Children.Add(card);
        }

        var intro = new TextBlock { Text = Loc.T("SitesIntro"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
        var more = new TextBlock { Text = Loc.T("SitesMore"), TextWrapping = TextWrapping.Wrap, Opacity = 0.7, Margin = new Thickness(0, 4, 0, 4) };
        var noDrm = new TextBlock { Text = Loc.T("SitesNoDrm"), TextWrapping = TextWrapping.Wrap, Opacity = 0.7, Margin = new Thickness(0, 0, 0, 12) };

        var link = new Button { Content = Loc.T("SitesFullList"), Padding = new Thickness(14, 6, 14, 6), HorizontalAlignment = HorizontalAlignment.Left };
        link.Click += (_, _) =>
        {
            try { Process.Start(new ProcessStartInfo(FullListUrl) { UseShellExecute = true }); } catch { }
        };

        var scroller = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = new StackPanel { Children = { tiles, more } } };
        var footer = new StackPanel { Margin = new Thickness(0, 8, 0, 0), Children = { noDrm, link } };

        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(intro, 0); Grid.SetRow(scroller, 1); Grid.SetRow(footer, 2);
        root.Children.Add(intro); root.Children.Add(scroller); root.Children.Add(footer);

        var win = new Window
        {
            Title = Loc.T("SitesTitle"), Content = root, Width = 620, Height = Math.Min(780, SystemParameters.WorkArea.Height - 40), MinWidth = 480, MinHeight = 420,
            Owner = Application.Current.MainWindow, WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false, Icon = Application.Current.MainWindow?.Icon,
        };
        win.SetResourceReference(Control.BackgroundProperty, "AppBgBrush");
        win.ShowDialog();
    }
}
