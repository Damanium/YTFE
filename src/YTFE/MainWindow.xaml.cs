using System.Windows;
using Microsoft.Win32;
using YTFE.Infrastructure;
using YTFE.Localization;
using YTFE.ViewModels;

namespace YTFE;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.InitializeAsync();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Avisar si se perdería una descarga y, al salir, no dejar procesos de yt-dlp huérfanos.
        if (_vm.HasActiveDownloads &&!Dialogs.Confirm(Loc.T("ConfirmExit")))
        {
            e.Cancel = true;
            return;
        }
        _vm.Shutdown();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = Loc.T("BrowseTitle"), InitialDirectory = _vm.OutputFolder };
        if (dlg.ShowDialog(this) == true) _vm.OutputFolder = dlg.FolderName;
    }
}
