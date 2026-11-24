using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using YTFE.Localization;
using YTFE.Services;

namespace YTFE;

/// <summary>Ventana de ayuda: índice de temas con buscador y contenido, en el idioma actual.</summary>
public partial class HelpWindow : Window
{
    private sealed record Entry(string Id, string Title);

    private static readonly Regex InlineRx = new(@"(\*\*.+?\*\*|`.+?`)", RegexOptions.Compiled);
    private static readonly Regex NumberedRx = new(@"^(\d+)\.\s+(.*)$", RegexOptions.Compiled);

    private string? _currentId;

    public HelpWindow(string? topicId = null)
    {
        InitializeComponent();
        _currentId = topicId ?? HelpContent.Topics[0].Id;
        Loc.Instance.LanguageChanged += OnLanguageChanged;
        Closed += (_, _) => Loc.Instance.LanguageChanged -= OnLanguageChanged;
        Loaded += (_, _) => SearchBox.Focus();
        Refresh();
    }

    private void OnLanguageChanged() => Dispatcher.BeginInvoke(Refresh);

    public void ShowTopic(string id)
    {
        _currentId = id;
        SearchBox.Text = "";
        Refresh();
    }

    // --- Índice -------------------------------------------------------------------
    private void Refresh()
    {
        var lang = Loc.Instance.Language;
        Title = Loc.T("HelpTitle");
        SearchHint.Text = Loc.T("HelpSearch");
        NoResults.Text = Loc.T("HelpNoResults");

        var q = SearchBox.Text.Trim();
        var entries = HelpContent.Topics
            .Where(t => q.Length == 0 ||
                        t.Title(lang).Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                        t.Body(lang).Contains(q, StringComparison.CurrentCultureIgnoreCase))
            .Select(t => new Entry(t.Id, t.Title(lang)))
            .ToList();

        TopicList.ItemsSource = entries;
        NoResults.Visibility = entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SearchHint.Visibility = q.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

        var selected = entries.FirstOrDefault(e => e.Id == _currentId) ?? entries.FirstOrDefault();
        TopicList.SelectedItem = selected;
        if (selected is null) ContentHost.Children.Clear(); // sin resultados: no se deja texto antiguo
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ContentHost is not null) Refresh();
    }

    private void TopicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TopicList.SelectedItem is not Entry entry) return;
        _currentId = entry.Id;
        var topic = HelpContent.Topics.First(t => t.Id == entry.Id);
        var lang = Loc.Instance.Language;
        Render(topic.Title(lang), topic.Body(lang));
        Scroller.ScrollToTop();
    }

    // --- Dibujo del contenido ------------------------------------------------------
    private void Render(string title, string body)
    {
        ContentHost.Children.Clear();

        var h1 = new TextBlock { Text = title, FontSize = 26, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) };
        h1.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        ContentHost.Children.Add(h1);

        foreach (var raw in body.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.Length == 0) continue;

            if (line.StartsWith("## "))
            {
                var h2 = new TextBlock { FontSize = 17, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 18, 0, 6) };
                AddInlines(h2, line[3..]);
                ContentHost.Children.Add(h2);
            }
            else if (line.StartsWith("> "))
            {
                var tip = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 13.5, LineHeight = 21 };
                AddInlines(tip, line[2..]);
                var box = new Border
                {
                    Child = tip, Padding = new Thickness(14, 10, 14, 10), Margin = new Thickness(0, 8, 0, 8),
                    CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(3, 0, 0, 0),
                };
                box.SetResourceReference(Border.BackgroundProperty, "ChipBrush");
                box.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                ContentHost.Children.Add(box);
            }
            else if (line.StartsWith("- "))
            {
                ContentHost.Children.Add(ListItem("•", line[2..]));
            }
            else if (NumberedRx.Match(line) is { Success: true } m)
            {
                ContentHost.Children.Add(ListItem(m.Groups[1].Value + ".", m.Groups[2].Value));
            }
            else
            {
                var p = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 14, LineHeight = 22, Margin = new Thickness(0, 4, 0, 4) };
                AddInlines(p, line);
                ContentHost.Children.Add(p);
            }
        }
    }

    private static Grid ListItem(string marker, string text)
    {
        var grid = new Grid { Margin = new Thickness(6, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var mark = new TextBlock { Text = marker, FontWeight = FontWeights.SemiBold, FontSize = 14, LineHeight = 22 };
        mark.SetResourceReference(TextBlock.ForegroundProperty, "AccentBrush");
        var body = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 14, LineHeight = 22 };
        AddInlines(body, text);
        Grid.SetColumn(body, 1);

        grid.Children.Add(mark);
        grid.Children.Add(body);
        return grid;
    }

    /// <summary>Convierte **negrita** y `código` en fragmentos con formato.</summary>
    private static void AddInlines(TextBlock block, string text)
    {
        foreach (var part in InlineRx.Split(text))
        {
            if (part.Length == 0) continue;
            if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
                block.Inlines.Add(new Run(part[2..^2]) { FontWeight = FontWeights.SemiBold });
            else if (part.StartsWith('`') && part.EndsWith('`') && part.Length > 2)
            {
                var code = new Run(part[1..^1]) { FontFamily = new FontFamily("Consolas"), FontSize = block.FontSize - 0.5 };
                code.SetResourceReference(TextElement.ForegroundProperty, "ChipTextBrush");
                block.Inlines.Add(code);
            }
            else block.Inlines.Add(new Run(part));
        }
    }
}
