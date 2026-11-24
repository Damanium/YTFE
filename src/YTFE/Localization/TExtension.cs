using System.Windows.Data;
using System.Windows.Markup;

namespace YTFE.Localization;

/// <summary>Extensión de XAML: Text="{loc:T Clave}". Se actualiza sola al cambiar de idioma.</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class TExtension : MarkupExtension
{
    public TExtension(string key) => Key = key;

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}
