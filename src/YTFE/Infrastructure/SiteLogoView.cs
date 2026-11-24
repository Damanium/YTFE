using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using YTFE.Services;

namespace YTFE.Infrastructure;

/// <summary>Dibuja el logo de un sitio como un azulejo de color de marca con el símbolo en blanco.</summary>
public static class SiteLogoView
{
    private static readonly Brush Unknown = Freeze(new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)));
    private static readonly Brush Edge = Freeze(new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)));

    private static Brush Freeze(Brush b) { b.Freeze(); return b; }

    public static FrameworkElement Create(string site, double size)
    {
        var logo = SiteLogos.TryGet(site);
        var tile = new Border
        {
            Width = size, Height = size, CornerRadius = new CornerRadius(size * 0.26),
            Background = logo?.Brush ?? Unknown, BorderBrush = Edge, BorderThickness = new Thickness(1),
            ToolTip = site,
        };

        if (logo?.Geometry is { } geometry)
        {
            tile.Child = new System.Windows.Shapes.Path
            {
                Data = geometry, Fill = Brushes.White, Stretch = Stretch.Uniform,
                Margin = new Thickness(size * 0.22),
            };
        }
        else
        {
            var letter = site.Length > 0 ? char.ToUpperInvariant(site[0]).ToString() : "?";
            tile.Child = new TextBlock
            {
                Text = letter, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = size * 0.5,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            };
        }
        return tile;
    }
}

/// <summary>Convierte el nombre de un sitio en su logo (el parámetro es el tamaño en píxeles).</summary>
public sealed class SiteLogoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        SiteLogoView.Create(value as string ?? "",
            double.TryParse(parameter as string, NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : 20);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
