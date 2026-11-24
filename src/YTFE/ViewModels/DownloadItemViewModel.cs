using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic.FileIO;
using YTFE.Infrastructure;
using YTFE.Localization;
using YTFE.Models;
using YTFE.Services;

namespace YTFE.ViewModels;

public sealed class DownloadItemViewModel : ObservableObject
{
    private readonly YtDlpService _service;
    private readonly DownloadOptions _options;
    private readonly string _outputFolder;
    private readonly SemaphoreSlim _gate;
    private readonly Action<DownloadItemViewModel> _removeFromList;

    private CancellationTokenSource _cts = new();    // cancelar la descarga entera
    private CancellationTokenSource? _attempt;       // detener solo el intento actual (pausa)
    private TaskCompletionSource<bool>? _resume;     // true = reanudar, false = cancelada mientras estaba en pausa
    private bool _pauseRequested;

    private string _title;
    private DownloadState _state = DownloadState.Queued;
    private double _percent;
    private double? _speed;
    private int? _eta;
    private string? _error;
    private string? _videoId;
    private string? _filePath;
    private bool _cookiesSkipped; // se descargó sin cookies porque no se pudieron leer

    public DownloadItemViewModel(string url, DownloadOptions options, string outputFolder,
        YtDlpService service, SemaphoreSlim gate, Action<DownloadItemViewModel> removeFromList)
    {
        Url = url;
        _title = url;
        _options = options;
        _outputFolder = outputFolder;
        _service = service;
        _gate = gate;
        _removeFromList = removeFromList;

        PauseCommand = new RelayCommand(_ => Pause(), _ => CanPause);
        ResumeCommand = new RelayCommand(_ => Resume(), _ => CanResume);
        CancelCommand = new RelayCommand(_ => Cancel(), _ => CanCancel);
        RetryCommand = new RelayCommand(_ => Retry(), _ => CanRetry);
        OpenFileCommand = new RelayCommand(_ => OpenFile(), _ => CanOpen);
        ShowInFolderCommand = new RelayCommand(_ => ShowInFolder());
        CopyLinkCommand = new RelayCommand(_ => CopyLink());
        OpenInBrowserCommand = new RelayCommand(_ => OpenInBrowser());
        RemoveCommand = new RelayCommand(_ => _removeFromList(this), _ => CanRemove);
        DeleteFilesCommand = new RelayCommand(_ => DeleteFiles(), _ => CanDeleteFiles);

        // Estado, detalle y resumen dependen del idioma: se recalculan al cambiarlo.
        Loc.Instance.LanguageChanged += () =>
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Detail));
            OnPropertyChanged(nameof(Summary));
        };
    }

    /// <summary>Se dispara cuando cambia algo que merece guardarse (estado, título…). No se dispara por cada tick de progreso.</summary>
    public event Action? Changed;

    /// <summary>Recrea una descarga guardada en la sesión anterior.</summary>
    public static DownloadItemViewModel Restore(DownloadSnapshot s, YtDlpService service, SemaphoreSlim gate,
        Action<DownloadItemViewModel> removeFromList)
    {
        var item = new DownloadItemViewModel(s.Url, s.Options, s.OutputFolder, service, gate, removeFromList)
        {
            _title = string.IsNullOrWhiteSpace(s.Title) ? s.Url : s.Title,
            _percent = Math.Clamp(s.Percent, 0, 100),
            _videoId = IsSafeVideoId(s.VideoId) ? s.VideoId : null,
            _error = s.Error,
            // Un estado "en marcha" guardado no tiene proceso detrás: se normaliza a pausa.
            _state = s.State is DownloadState.Queued or DownloadState.Downloading or DownloadState.Processing
                ? DownloadState.Paused : s.State,
            // Solo se acepta una ruta dentro de la carpeta de descargas: así un archivo de datos alterado
            // no puede hacer que "Abrir" o "Eliminar" apunten a otro lugar del disco.
            _filePath = IsInsideFolder(s.FilePath, s.OutputFolder) ? s.FilePath : null,
        };
        return item;
    }

    public DownloadSnapshot ToSnapshot() => new(
        Url, _options, _outputFolder, _title,
        // Lo que estaba en marcha al guardar se restaura en pausa: nada arranca solo al abrir la app.
        IsUnfinished ? DownloadState.Paused : _state,
        _percent, _videoId, _filePath, _error);

    private static bool IsSafeVideoId(string? id) =>
        !string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"^[\w-]{1,64}$");

    private static bool IsInsideFolder(string? file, string folder)
    {
        if (string.IsNullOrEmpty(file)) return false;
        try
        {
            var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.GetFullPath(file).StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    // --- Datos -----------------------------------------------------------------
    public string Url { get; }
    public string Site => SiteNames.FromUrl(Url);
    public string Summary => _options.Summary;
    public string Title
    {
        get => _title;
        private set { if (SetProperty(ref _title, value)) Changed?.Invoke(); }
    }
    public double Percent { get => _percent; private set => SetProperty(ref _percent, value); }
    public string Status => Loc.T("State_" + _state);

    public string Detail
    {
        get
        {
            if (_state == DownloadState.Completed && _cookiesSkipped) return Loc.T("CookiesSkippedNote");
            if (_state == DownloadState.Error) return ErrorHints.Describe(_error, !string.IsNullOrEmpty(_service.CookiesBrowser));
            if (_state is not (DownloadState.Downloading or DownloadState.Processing or DownloadState.Paused)) return "";

            var c = Loc.Instance.Culture;
            var parts = new List<string> { string.Format(c, "{0:0.0}%", _percent) };
            if (_state != DownloadState.Paused)
            {
                if (_speed is { } s) parts.Add(string.Format(c, "{0:0.00} MB/s", s / 1048576));
                if (_eta is { } e) parts.Add($"{Loc.T("Eta")} {TimeSpan.FromSeconds(e):mm\\:ss}");
            }
            return string.Join(" · ", parts);
        }
    }

    /// <summary>Esperando turno o con un proceso de yt-dlp en marcha (usa los binarios).</summary>
    public bool IsError => _state == DownloadState.Error;
    public bool IsCompleted => _state == DownloadState.Completed;

    public bool IsActive => _state is DownloadState.Queued or DownloadState.Downloading or DownloadState.Processing;

    /// <summary>Todavía no ha terminado (puede estar en pausa).</summary>
    public bool IsUnfinished => IsActive || _state == DownloadState.Paused;

    // --- Qué acciones están disponibles en cada estado ---------------------------
    // Pausar no se permite durante "Procesando": interrumpir ffmpeg a medias dejaría el archivo corrupto.
    public bool CanPause => _state == DownloadState.Downloading && !_pauseRequested;
    public bool CanResume => _state == DownloadState.Paused;
    public bool CanCancel => IsUnfinished;
    public bool CanRetry => _state is DownloadState.Error or DownloadState.Cancelled;
    public bool CanOpen => _state == DownloadState.Completed && _filePath is not null && File.Exists(_filePath);
    public bool CanRemove => _state is DownloadState.Completed or DownloadState.Error or DownloadState.Cancelled;
    public bool CanDeleteFiles => _state is DownloadState.Completed or DownloadState.Paused
                                          or DownloadState.Error or DownloadState.Cancelled;

    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand ShowInFolderCommand { get; }
    public ICommand CopyLinkCommand { get; }
    public ICommand OpenInBrowserCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand DeleteFilesCommand { get; }

    // --- Ciclo de vida -----------------------------------------------------------
    /// <summary>
    /// Bucle de la descarga. Una pausa detiene el proceso de yt-dlp (conservando los .part) y espera;
    /// al reanudar se lanza de nuevo y yt-dlp continúa desde donde quedó.
    /// </summary>
    public async Task RunAsync(bool startPaused = false)
    {
        var ct = _cts.Token;

        // Descarga restaurada de la sesión anterior: espera en pausa hasta que el usuario reanude.
        if (startPaused && !await WaitForResumeAsync(ct)) { await FinishCancelledAsync(); return; }

        while (true)
        {
            SetState(DownloadState.Queued);
            try { await _gate.WaitAsync(ct); }
            catch (OperationCanceledException) { await FinishCancelledAsync(); return; }

            YtDlpService.DownloadResult result;
            using var attempt = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _attempt = attempt;
            try
            {
                SetState(DownloadState.Downloading);
                var progress = new Progress<DownloadProgress>(OnProgress); // se captura el contexto de la UI
                var dispatcher = Application.Current.Dispatcher;
                result = await _service.DownloadAsync(
                    Url, _options, _outputFolder, progress,
                    t => dispatcher.BeginInvoke(() => Title = t),
                    // Llega desde el hilo que lee la salida de yt-dlp: se pasa al hilo de la interfaz.
                    id => dispatcher.BeginInvoke(() => { _videoId = id; Changed?.Invoke(); }),
                    attempt.Token);
            }
            finally
            {
                _attempt = null;
                _gate.Release(); // una descarga en pausa no ocupa un turno
            }

            _cookiesSkipped = result.CookiesSkipped;
            if (result.Success)
            {
                _filePath = result.FilePath;
                Percent = 100;
                Finish(DownloadState.Completed);
                return;
            }

            if (ct.IsCancellationRequested) { await FinishCancelledAsync(); return; }

            if (_pauseRequested)
            {
                _pauseRequested = false;
                if (!await WaitForResumeAsync(ct)) { await FinishCancelledAsync(); return; }
                continue; // reanudada: vuelve a pedir turno y relanza yt-dlp
            }

            Finish(DownloadState.Error, result.Error);
            return;
        }
    }

    /// <summary>Queda en pausa hasta que se reanude (true) o se cancele (false).</summary>
    private async Task<bool> WaitForResumeAsync(CancellationToken ct)
    {
        var resume = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _resume = resume;
        using (ct.Register(() => resume.TrySetResult(false)))
        {
            SetState(DownloadState.Paused);
            return await resume.Task;
        }
    }

    private void Pause()
    {
        if (!CanPause) return;
        _pauseRequested = true;
        RaiseStateChanged();   // desactiva "Pausar" al instante
        _attempt?.Cancel();    // el servicio mata el proceso; los .part se conservan
    }

    private void Resume() => _resume?.TrySetResult(true);

    /// <summary>Por debajo de este avance, cancelar no pide confirmación (se pierde muy poco).</summary>
    private const double ConfirmCancelAbovePercent = 5;

    /// <summary>
    /// Cancelar = abandonar: detiene el proceso y descarta lo descargado (los .part y temporales de ESTE vídeo).
    /// Para conservar lo descargado y seguir luego está Pausar.
    /// </summary>
    private void Cancel()
    {
        if (!CanCancel) return;

        if (_percent > ConfirmCancelAbovePercent &&
            !Dialogs.Confirm(Loc.T("ConfirmCancel", Math.Round(_percent))))
            return;

        _discardOnCancel = true;
        _cts.Cancel(); // el bucle de RunAsync termina en FinishCancelledAsync, ya sin proceso que bloquee archivos
    }

    private bool _discardOnCancel;

    private async Task FinishCancelledAsync()
    {
        if (_discardOnCancel)
        {
            _discardOnCancel = false;
            var failed = await DeleteFilesWithRetryAsync(FindPartialFiles());
            Percent = 0;
            _speed = null;
            _eta = null;
            if (failed is not null) Dialogs.Error(Loc.T("DeleteFailed", failed));
        }
        Finish(DownloadState.Cancelled);
    }

    /// <summary>
    /// Borra los archivos dados. Reintenta unas veces porque Windows (o el antivirus) puede tardar
    /// un instante en soltar un archivo tras matar el proceso. Devuelve el primer error, o null si todo fue bien.
    /// </summary>
    private static async Task<string?> DeleteFilesWithRetryAsync(IEnumerable<string> paths)
    {
        string? firstError = null;
        foreach (var path in paths)
        {
            for (var attempt = 1; ; attempt++)
            {
                try { File.Delete(path); break; }
                catch (Exception ex) when (attempt < 4 && ex is IOException or UnauthorizedAccessException)
                {
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    firstError ??= $"{Path.GetFileName(path)}: {ex.Message}";
                    break;
                }
            }
        }
        return firstError;
    }

    private void Retry()
    {
        if (!CanRetry) return;
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _pauseRequested = false;
        _error = null;
        _ = RunAsync(); // yt-dlp reutiliza los .part que existan
    }

    private void OnProgress(DownloadProgress p)
    {
        // Un mensaje de progreso tardío no debe pisar el estado de pausa o de fin.
        if (_state is not (DownloadState.Downloading or DownloadState.Processing)) return;

        Percent = p.Percent;
        _speed = p.SpeedBytesPerSec;
        _eta = p.EtaSeconds;
        SetState(p.Status == "finished" ? DownloadState.Processing : DownloadState.Downloading);
    }

    private void SetState(DownloadState state)
    {
        var changed = _state != state;
        _state = state;
        RaiseStateChanged();
        if (changed) Changed?.Invoke(); // el progreso (Percent) no dispara esto: solo los cambios de estado
    }

    private void Finish(DownloadState state, string? error = null)
    {
        _error = error;
        SetState(state);
    }

    private void RaiseStateChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Detail));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(IsError));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsUnfinished));
        OnPropertyChanged(nameof(CanPause));
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanRetry));
        OnPropertyChanged(nameof(CanOpen));
        OnPropertyChanged(nameof(CanRemove));
        OnPropertyChanged(nameof(CanDeleteFiles));
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>Detiene el proceso sin más (al cerrar la aplicación).</summary>
    public void Shutdown() => _cts.Cancel();

    // --- Acciones sobre archivos y enlace ------------------------------------------
    private void OpenFile()
    {
        if (_filePath is null) return;
        try { Process.Start(new ProcessStartInfo(_filePath) { UseShellExecute = true }); }
        catch (Exception ex) { Dialogs.Error(ex.Message); }
    }

    private void ShowInFolder()
    {
        try
        {
            if (_filePath is not null && File.Exists(_filePath))
                Process.Start("explorer.exe", $"/select,\"{_filePath}\"");
            else if (Directory.Exists(_outputFolder))
                Process.Start(new ProcessStartInfo(_outputFolder) { UseShellExecute = true });
        }
        catch (Exception ex) { Dialogs.Error(ex.Message); }
    }

    private void CopyLink()
    {
        try { Clipboard.SetText(Url); }
        catch { /* el portapapeles puede estar bloqueado por otra aplicación; no es grave */ }
    }

    private void OpenInBrowser()
    {
        // La URL ya se validó como http/https al añadirla.
        try { Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true }); }
        catch (Exception ex) { Dialogs.Error(ex.Message); }
    }

    /// <summary>
    /// Completada: envía el archivo final a la Papelera (se puede recuperar).
    /// Sin completar: borra solo los archivos parciales de ESTE vídeo (.part, .ytdl, temporales).
    /// </summary>
    private void DeleteFiles()
    {
        if (!CanDeleteFiles) return;

        var completed = _state == DownloadState.Completed && _filePath is not null;
        var targets = completed
            ? (File.Exists(_filePath!) ? [_filePath!] : new List<string>())
            : FindPartialFiles();

        var detail = targets.Count > 0
            ? string.Join("\n", targets.Take(5).Select(Path.GetFileName)) + (targets.Count > 5 ? "\n…" : "")
            : Title;
        if (!Dialogs.Confirm(Loc.T(completed ? "ConfirmDeleteFile" : "ConfirmDeletePartial", detail))) return;

        if (_state == DownloadState.Paused) _cts.Cancel(); // libera el bucle que espera la reanudación (sin pedir otra confirmación)

        try
        {
            foreach (var path in targets)
            {
                if (completed)
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                else
                    File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Dialogs.Error(Loc.T("DeleteFailed", ex.Message));
            return;
        }

        _removeFromList(this);
    }

    /// <summary>Archivos temporales de yt-dlp que pertenecen a este vídeo (por su id en el nombre).</summary>
    private List<string> FindPartialFiles()
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(_videoId) || !Directory.Exists(_outputFolder)) return result;

        var marker = $"[{_videoId}]";
        foreach (var path in Directory.EnumerateFiles(_outputFolder))
        {
            var name = Path.GetFileName(path);
            var i = name.LastIndexOf(marker, StringComparison.Ordinal);
            if (i < 0) continue;

            // Lo que va tras "[id]": el archivo final solo tiene ".ext"; los temporales tienen más.
            var rest = name[(i + marker.Length)..];
            if (rest.Contains(".part", StringComparison.OrdinalIgnoreCase) ||
                rest.EndsWith(".ytdl", StringComparison.OrdinalIgnoreCase) ||
                rest.StartsWith(".temp.", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(rest, @"^\.f[\w-]+\.\w+$"))
                result.Add(path);
        }
        return result;
    }
}
