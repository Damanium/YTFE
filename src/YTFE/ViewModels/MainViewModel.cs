using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using YTFE.Infrastructure;
using YTFE.Localization;
using YTFE.Models;
using YTFE.Services;

namespace YTFE.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly int[] StandardHeights = [4320, 2160, 1440, 1080, 720, 480, 360, 240, 144];

    private readonly AppSettings _settings = AppSettings.Load();
    private readonly ToolsManager _tools = new();
    private readonly YtDlpService _service;
    private readonly SemaphoreSlim _gate = new(2); // descargas simultáneas
    private readonly DownloadStore _store = new();
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private bool _persist = true; // se desactiva al cerrar, para no sobrescribir con el estado "cancelado" del cierre

    private string _url = "";
    private string? _analyzedUrl;
    private bool _isVideo = true;
    private Choice<int?> _selectedResolution;
    private Choice<string> _selectedContainer;
    private Choice<VideoCodec> _selectedCodec;
    private Choice<string> _selectedAudioFormat;
    private Choice<string> _selectedAudioQuality;
    private Choice<string> _selectedLanguage;
    private Choice<string> _selectedCookies;
    private Choice<string> _selectedTheme;
    private bool _playlist;
    private string _cutFrom = "";
    private string _cutTo = "";
    private bool _autoAudio; // el modo audio lo puso la app (sitio de música), no el usuario
    private bool _embedMetadata = true;
    private bool _embedThumbnail;
    private string _outputFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "YTFE");
    private string _statusText = "";
    private string _infoText = "";
    private string _versionsText = "";
    private Func<string>? _statusFactory; // permite volver a traducir el mensaje al cambiar de idioma
    private Func<string>? _infoFactory;
    private bool _toolsReady;
    private bool _busy;
    private bool _analyzing;

    public MainViewModel()
    {
        _service = new YtDlpService(_tools);

        Containers =
        [
            Choice.L("ContMp4", "mp4"),
            Choice.L("ContMkv", "mkv"),
            Choice.Lit("WEBM (VP9/AV1 + Opus)", "webm"),
        ];
        Codecs =
        [
            Choice.L("CodecAuto", VideoCodec.Auto),
            Choice.L("CodecH264", VideoCodec.H264),
            Choice.Lit("VP9", VideoCodec.VP9),
            Choice.L("CodecAv1", VideoCodec.AV1),
        ];
        AudioFormats =
        [
            Choice.Lit("MP3", "mp3"),
            Choice.Lit("OGG (Vorbis)", "vorbis"),
            Choice.L("FmtWav", "wav"),
            Choice.L("FmtFlac", "flac"),
            Choice.Lit("M4A (AAC)", "m4a"),
            Choice.Lit("Opus", "opus"),
        ];
        AudioQualities =
        [
            Choice.L("QualBest", "best"),
            Choice.Lit("320 kbps", "320K"),
            Choice.Lit("256 kbps", "256K"),
            Choice.Lit("192 kbps", "192K"),
            Choice.Lit("128 kbps", "128K"),
            Choice.Lit("96 kbps", "96K"),
        ];
        Languages =
        [
            Choice.Lit("Español", "es"),
            Choice.Lit("English", "en"),
        ];
        Themes =
        [
            Choice.L("ThemeSystem", "system"),
            Choice.L("ThemeLight", "light"),
            Choice.L("ThemeDark", "dark"),
        ];
        // El valor es el que acepta --cookies-from-browser de yt-dlp ("" = no usar cookies).
        CookieBrowsers =
        [
            Choice.L("CookiesNone", ""),
            Choice.Lit("Chrome", "chrome"),
            Choice.Lit("Edge", "edge"),
            Choice.Lit("Firefox", "firefox"),
            Choice.Lit("Brave", "brave"),
            Choice.Lit("Opera", "opera"),
            Choice.Lit("Vivaldi", "vivaldi"),
        ];

        ResetResolutions();
        _selectedResolution = Resolutions[0];
        _selectedContainer = Containers[0];
        _selectedCodec = Codecs[0];
        _selectedAudioFormat = AudioFormats[0];
        _selectedAudioQuality = AudioQualities[0];
        _selectedLanguage = Languages.First(l => l.Value == Loc.Instance.Language);
        _selectedTheme = Themes.FirstOrDefault(t => t.Value == _settings.Theme) ?? Themes[0];
        _selectedCookies = CookieBrowsers.FirstOrDefault(c => c.Value == _settings.CookiesBrowser) ?? CookieBrowsers[0];
        _service.CookiesBrowser = _selectedCookies.Value;

        SetStatus("Starting");

        // Mientras se actualizan binarios (_busy) no se lanzan descargas ni análisis.
        AddCommand = new RelayCommand(_ => Add(), _ => ToolsReady && !_busy && IsValidUrl(Url) && CutIsValid);
        AnalyzeCommand = new RelayCommand(async _ => await AnalyzeAsync(), _ => ToolsReady && !_busy && !_analyzing && IsValidUrl(Url));
        UpdateToolsCommand = new RelayCommand(async _ => await UpdateToolsAsync(), _ => !_busy);
        PauseAllCommand = new RelayCommand(_ => PauseAll(), _ => Downloads.Any(d => d.CanPause));
        ResumeAllCommand = new RelayCommand(_ => ResumeAll(), _ => Downloads.Any(d => d.CanResume));
        ClearFinishedCommand = new RelayCommand(_ => ClearFinished(), _ => Downloads.Any(d => d.CanRemove));

        // Persistencia: se restaura la lista de la sesión anterior y se guarda (con 1 s de retardo
        // para agrupar cambios seguidos) cada vez que cambia la lista o el estado de una descarga.
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); SaveNow(); };
        RestoreDownloads();
        Downloads.CollectionChanged += (_, _) => ScheduleSave();

        Loc.Instance.LanguageChanged += () =>
        {
            if (_statusFactory is not null) StatusText = _statusFactory();
            if (_infoFactory is not null) InfoText = _infoFactory();
        };
    }

    // --- Listas de opciones -------------------------------------------------
    public ObservableCollection<Choice<int?>> Resolutions { get; } = [];
    public IReadOnlyList<Choice<string>> Containers { get; }
    public IReadOnlyList<Choice<VideoCodec>> Codecs { get; }
    public IReadOnlyList<Choice<string>> AudioFormats { get; }
    public IReadOnlyList<Choice<string>> AudioQualities { get; }
    public IReadOnlyList<Choice<string>> Languages { get; }
    public IReadOnlyList<Choice<string>> CookieBrowsers { get; }
    public IReadOnlyList<Choice<string>> Themes { get; }
    public ObservableCollection<DownloadItemViewModel> Downloads { get; } = [];

    public ICommand AddCommand { get; }
    public ICommand AnalyzeCommand { get; }
    public ICommand UpdateToolsCommand { get; }
    public ICommand PauseAllCommand { get; }
    public ICommand ResumeAllCommand { get; }
    public ICommand ClearFinishedCommand { get; }
    public ICommand ShowSitesCommand { get; } = new RelayCommand(_ => Dialogs.ShowSites());
    public ICommand ShowHelpCommand { get; } = new RelayCommand(_ => Dialogs.ShowHelp());

    // --- Estado --------------------------------------------------------------
    public string Url
    {
        get => _url;
        set
        {
            if (!SetProperty(ref _url, value)) return;
            ApplySiteDefaults(value);
            // Si cambia la URL, las resoluciones analizadas dejan de ser válidas.
            if (_analyzedUrl is not null && _analyzedUrl != value.Trim())
            {
                _analyzedUrl = null;
                SetInfo(null);
                ResetResolutions();
            }
        }
    }

    public bool IsVideo
    {
        get => _isVideo;
        set
        {
            if (!SetProperty(ref _isVideo, value)) return;
            _autoAudio = false;
            OnPropertyChanged(nameof(IsAudio));
            OnPropertyChanged(nameof(CanEmbedThumbnail));
        }
    }
    public bool IsAudio { get => !_isVideo; set => IsVideo = !value; }

    public Choice<int?> SelectedResolution
    {
        get => _selectedResolution;
        set { if (value is not null) SetProperty(ref _selectedResolution, value); }
    }

    public Choice<string> SelectedContainer
    {
        get => _selectedContainer;
        set
        {
            if (value is null || !SetProperty(ref _selectedContainer, value)) return;
            OnPropertyChanged(nameof(CanEmbedThumbnail));
        }
    }

    public Choice<VideoCodec> SelectedCodec
    {
        get => _selectedCodec;
        set { if (value is not null) SetProperty(ref _selectedCodec, value); }
    }

    public Choice<string> SelectedAudioFormat
    {
        get => _selectedAudioFormat;
        set
        {
            if (value is null || !SetProperty(ref _selectedAudioFormat, value)) return;
            OnPropertyChanged(nameof(IsAudioQualityEnabled));
            OnPropertyChanged(nameof(CanEmbedThumbnail));
        }
    }

    public Choice<string> SelectedAudioQuality
    {
        get => _selectedAudioQuality;
        set { if (value is not null) SetProperty(ref _selectedAudioQuality, value); }
    }

    public Choice<string> SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null || !SetProperty(ref _selectedLanguage, value)) return;
            Loc.Instance.SetLanguage(value.Value);
            _settings.Language = value.Value;
            _settings.Save();
        }
    }

    public Choice<string> SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (value is null || !SetProperty(ref _selectedTheme, value)) return;
            ThemeManager.Apply(value.Value);
            _settings.Theme = value.Value;
            _settings.Save();
        }
    }

    /// <summary>Navegador del que se toman las cookies para sitios que exigen sesión. Afecta a análisis y descargas nuevas.</summary>
    public Choice<string> SelectedCookies
    {
        get => _selectedCookies;
        set
        {
            if (value is null || !SetProperty(ref _selectedCookies, value)) return;
            _service.CookiesBrowser = value.Value;
            _settings.CookiesBrowser = value.Value.Length > 0 ? value.Value : null;
            _settings.Save();
        }
    }

    /// <summary>WAV y FLAC no tienen bitrate: la calidad es siempre la original.</summary>
    public bool IsAudioQualityEnabled => !DownloadOptions.IsLossless(SelectedAudioFormat.Value);

    public bool CanEmbedThumbnail =>
        DownloadOptions.CanEmbedThumbnailFor(Kind, SelectedContainer.Value, SelectedAudioFormat.Value);

    public bool Playlist { get => _playlist; set => SetProperty(ref _playlist, value); }

    public string CutFrom
    {
        get => _cutFrom;
        set { if (SetProperty(ref _cutFrom, value)) OnCutChanged(); }
    }

    public string CutTo
    {
        get => _cutTo;
        set { if (SetProperty(ref _cutTo, value)) OnCutChanged(); }
    }

    public bool CutIsValid => DownloadOptions.IsValidTime(CutFrom) && DownloadOptions.IsValidTime(CutTo);

    private void OnCutChanged()
    {
        OnPropertyChanged(nameof(CutIsValid));
        CommandManager.InvalidateRequerySuggested();
    }

    // Sitios de música/audio: se preselecciona "Solo audio" y, si el usuario no lo toca, se revierte al cambiar de sitio.
    private static readonly HashSet<string> AudioSites = ["SoundCloud", "Bandcamp", "Mixcloud"];

    private void ApplySiteDefaults(string url)
    {
        var audioSite = AudioSites.Contains(SiteNames.FromUrl(url.Trim()));
        if (audioSite && IsVideo)
        {
            IsVideo = false;
            _autoAudio = true; // tras IsVideo, que lo borra
        }
        else if (!audioSite && _autoAudio)
        {
            IsVideo = true;
        }
    }

    public bool EmbedMetadata { get => _embedMetadata; set => SetProperty(ref _embedMetadata, value); }
    public bool EmbedThumbnail { get => _embedThumbnail; set => SetProperty(ref _embedThumbnail, value); }

    public string OutputFolder { get => _outputFolder; set => SetProperty(ref _outputFolder, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string InfoText { get => _infoText; private set => SetProperty(ref _infoText, value); }
    public bool ToolsReady { get => _toolsReady; private set => SetProperty(ref _toolsReady, value); }
    public string VersionsText { get => _versionsText; private set => SetProperty(ref _versionsText, value); }
    public bool IsBusy => _busy;

    private MediaKind Kind => IsVideo ? MediaKind.Video : MediaKind.Audio;

    // --- Mensajes -------------------------------------------------------------
    /// <summary>Mensaje de la barra de estado a partir de una clave de idioma (se retraduce al cambiar de idioma).</summary>
    private void SetStatus(string key, params object[] args)
    {
        _statusFactory = () => Loc.T(key, args);
        StatusText = _statusFactory();
    }

    /// <summary>Mensaje ya formado (p. ej. la salida de una actualización).</summary>
    private void SetStatusRaw(string text)
    {
        _statusFactory = null;
        StatusText = text;
    }

    private void SetInfo(Func<string>? factory)
    {
        _infoFactory = factory;
        InfoText = factory?.Invoke() ?? "";
    }

    // --- Acciones globales sobre la lista --------------------------------------
    /// <summary>¿Hay descargas en marcha o en cola? (Las que están en pausa no tienen proceso: nada se pierde al salir.)</summary>
    public bool HasActiveDownloads => Downloads.Any(d => d.IsActive);

    /// <summary>Detiene todos los procesos de yt-dlp (al cerrar la aplicación). Los .part se conservan.</summary>
    public void Shutdown()
    {
        // Primero se guarda el estado real (lo que estaba en marcha queda como "en pausa") y luego se
        // detienen los procesos; sin esto, el cierre marcaría todo como "cancelado".
        _saveTimer.Stop();
        SaveNow();
        _persist = false;
        foreach (var d in Downloads.ToList()) d.Shutdown();
    }

    private void RestoreDownloads()
    {
        foreach (var s in _store.Load())
        {
            if (!IsValidUrl(s.Url)) continue; // el archivo lo puede haber tocado alguien: se revalida

            var item = DownloadItemViewModel.Restore(s, _service, _gate, i => Downloads.Remove(i));
            Attach(item);
            Downloads.Add(item);

            // Las que estaban a medias vuelven en pausa; solo arrancan cuando el usuario pulsa "Reanudar".
            if (item.CanResume) _ = item.RunAsync(startPaused: true);
        }
    }

    private void Attach(DownloadItemViewModel item) => item.Changed += ScheduleSave;

    private void ScheduleSave()
    {
        if (!_persist) return;
        _saveTimer.Stop();
        _saveTimer.Start(); // reinicia el retardo: varios cambios seguidos = un solo guardado
    }

    private void SaveNow()
    {
        if (!_persist) return;
        _store.Save(Downloads.Select(d => d.ToSnapshot()).ToList());
    }

    private void PauseAll()
    {
        foreach (var d in Downloads.ToList())
            if (d.CanPause) d.PauseCommand.Execute(null);
    }

    private void ResumeAll()
    {
        foreach (var d in Downloads.ToList())
            if (d.CanResume) d.ResumeCommand.Execute(null);
    }

    /// <summary>Quita de la lista las descargas terminadas (completadas, con error o canceladas). No borra archivos.</summary>
    private void ClearFinished()
    {
        foreach (var d in Downloads.Where(d => d.CanRemove).ToList()) Downloads.Remove(d);
    }

    // --- Acciones ------------------------------------------------------------
    public async Task InitializeAsync()
    {
        SetBusy(true);
        try
        {
            await _tools.EnsureAsync(new Progress<string>(SetStatusRaw));
            ToolsReady = _tools.AllPresent;
        }
        catch (Exception ex)
        {
            SetStatus("ToolsDownloadFailed", ex.Message);
            SetBusy(false);
            return;
        }

        await RefreshVersionsAsync();

        // Comprobación automática de yt-dlp, como máximo una vez cada 24 h.
        // Solo yt-dlp (22 MB): ffmpeg y deno se actualizan a mano con el botón.
        if (_tools.IsAutoCheckDue())
        {
            SetStatus("CheckingYtDlp");
            try
            {
                SetStatusRaw(await _tools.UpdateYtDlpAsync());
                _tools.MarkAutoChecked(); // solo si la comprobación tuvo éxito; si no, se reintenta al próximo arranque
                await RefreshVersionsAsync();
            }
            catch (Exception ex) { SetStatus("YtDlpCheckFailed", ex.Message); }
        }
        else SetStatus("ToolsReady");

        SetBusy(false);
    }

    private async Task UpdateToolsAsync()
    {
        // No se sustituyen binarios mientras haya descargas usándolos.
        if (Downloads.Any(d => d.IsActive))
        {
            SetStatus("WaitDownloads");
            return;
        }

        SetBusy(true);
        try
        {
            SetStatusRaw(await _tools.UpdateAllAsync(new Progress<string>(SetStatusRaw)));
            _tools.MarkAutoChecked();
            ToolsReady = _tools.AllPresent;
            await RefreshVersionsAsync();
        }
        catch (Exception ex) { SetStatus("UpdateFailed", ex.Message); }
        finally { SetBusy(false); }
    }

    private async Task RefreshVersionsAsync()
    {
        var v = await _tools.GetVersionsAsync();
        VersionsText = $"yt-dlp {v.YtDlp} · ffmpeg {v.Ffmpeg} · deno {v.Deno}";
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        OnPropertyChanged(nameof(IsBusy));
        CommandManager.InvalidateRequerySuggested();
    }

    private async Task AnalyzeAsync()
    {
        var url = Url.Trim();
        _analyzing = true;
        SetInfo(() => Loc.T("Analyzing"));
        try
        {
            var info = await _service.GetInfoAsync(url, CancellationToken.None);
            _analyzedUrl = url;

            var previous = SelectedResolution.Value;
            Resolutions.Clear();
            Resolutions.Add(Choice.L<int?>("ResBest", null));
            foreach (var h in info.Heights) Resolutions.Add(Choice.Lit<int?>($"{h}p", h));
            SelectedResolution = Resolutions.FirstOrDefault(r => r.Value == previous) ?? Resolutions[0];

            SetInfo(() =>
            {
                var parts = new List<string> { info.Title };
                if (info.Uploader is not null) parts.Add(info.Uploader);
                if (info.Duration is { } d) parts.Add(d.TotalHours >= 1 ? d.ToString(@"h\:mm\:ss") : d.ToString(@"m\:ss"));
                if (info.Heights.Count > 0) parts.Add(Loc.T("SumUpTo", info.Heights[0]));
                if (info.CookiesSkipped) parts.Add(Loc.T("CookiesSkippedNote"));
                return string.Join(" · ", parts);
            });
        }
        catch (Exception ex)
        {
            SetInfo(() => Loc.T("AnalyzeFailed", ErrorHints.Describe(ex.Message, !string.IsNullOrEmpty(_service.CookiesBrowser))));
        }
        finally { _analyzing = false; }
    }

    private void Add()
    {
        var options = new DownloadOptions(
            Kind,
            SelectedResolution.Value,
            SelectedContainer.Value,
            SelectedCodec.Value,
            SelectedAudioFormat.Value,
            SelectedAudioQuality.Value,
            EmbedMetadata,
            EmbedThumbnail,
            Playlist,
            string.IsNullOrWhiteSpace(CutFrom) ? null : CutFrom.Trim(),
            string.IsNullOrWhiteSpace(CutTo) ? null : CutTo.Trim());

        var item = new DownloadItemViewModel(Url.Trim(), options, OutputFolder, _service, _gate,
            i => Downloads.Remove(i));
        Attach(item);
        Downloads.Insert(0, item);
        Url = "";
        _ = item.RunAsync();
    }

    private void ResetResolutions()
    {
        var previous = _selectedResolution?.Value;
        Resolutions.Clear();
        Resolutions.Add(Choice.L<int?>("ResBest", null));
        foreach (var h in StandardHeights)
            Resolutions.Add(Choice.Lit<int?>(h switch { 4320 => "4320p (8K)", 2160 => "2160p (4K)", 1440 => "1440p (2K)", _ => $"{h}p" }, h));
        if (_selectedResolution is not null)
            SelectedResolution = Resolutions.FirstOrDefault(r => r.Value == previous) ?? Resolutions[0];
    }

    // Solo http/https: evita pasar rutas locales u otros esquemas a yt-dlp.
    private static bool IsValidUrl(string s) =>
        Uri.TryCreate(s?.Trim(), UriKind.Absolute, out var u) &&
        (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}
