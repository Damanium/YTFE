using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using YTFE.Localization;
using YTFE.Models;

namespace YTFE.Services;

public sealed class YtDlpService
{
    private const string ProgressPrefix = "YTFE|";
    private const string TitlePrefix = "YTFE_TITLE|";
    private const string IdPrefix = "YTFE_ID|";
    private const string FilePrefix = "YTFE_FILE|";

    private readonly ToolsManager _tools;

    public YtDlpService(ToolsManager tools) => _tools = tools;

    /// <summary>Navegador del que se toman las cookies (valor de --cookies-from-browser), o null.</summary>
    public string? CookiesBrowser { get; set; }

    public sealed record DownloadResult(bool Success, string? FilePath, string? Error, bool CookiesSkipped = false);

    private bool CookiesActive => !string.IsNullOrEmpty(CookiesBrowser);

    /// <summary>
    /// Descarga con las cookies elegidas. Si el fallo es que no se pudieron leer, reintenta sin cookies
    /// (los vídeos públicos no las necesitan) y lo indica en el resultado.
    /// </summary>
    public async Task<DownloadResult> DownloadAsync(
        string url,
        DownloadOptions options,
        string outputFolder,
        IProgress<DownloadProgress> progress,
        Action<string> onTitle,
        Action<string> onId,
        CancellationToken ct)
    {
        var result = await DownloadCoreAsync(url, options, outputFolder, progress, onTitle, onId, ct, CookiesActive);
        if (!result.Success && CookiesActive && !ct.IsCancellationRequested && ErrorHints.IsCookieReadError(result.Error))
        {
            result = await DownloadCoreAsync(url, options, outputFolder, progress, onTitle, onId, ct, useCookies: false);
            return result with { CookiesSkipped = true };
        }
        return result;
    }

    private async Task<DownloadResult> DownloadCoreAsync(
        string url,
        DownloadOptions options,
        string outputFolder,
        IProgress<DownloadProgress> progress,
        Action<string> onTitle,
        Action<string> onId,
        CancellationToken ct,
        bool useCookies)
    {
        Directory.CreateDirectory(outputFolder);

        var psi = NewStartInfo(useCookies);

        // ArgumentList escapa cada argumento: la URL nunca se interpreta como shell ni como opción.
        foreach (var a in new[]
        {
            "--newline", "--progress", options.Playlist ? "--yes-playlist" : "--no-playlist", "--no-colors", "--no-warnings",
            "-P", outputFolder,
            "-o", "%(title).150B [%(id)s].%(ext)s",
            "--progress-template",
            "download:" + ProgressPrefix +
                "%(progress.status)s|%(progress.downloaded_bytes)s|%(progress.total_bytes)s|" +
                "%(progress.total_bytes_estimate)s|%(progress.speed)s|%(progress.eta)s",
            "--print", "before_dl:" + TitlePrefix + "%(title)s",
            "--print", "before_dl:" + IdPrefix + "%(id)s",
            "--print", "after_move:" + FilePrefix + "%(filepath)s",
        }) psi.ArgumentList.Add(a);

        foreach (var a in options.ToArgs()) psi.ArgumentList.Add(a);
        psi.ArgumentList.Add("--");
        psi.ArgumentList.Add(url);

        string? filePath = null;
        var errors = new List<string>();

        using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, e) =>
        {
            var line = e.Data;
            if (string.IsNullOrEmpty(line)) return;

            if (line.StartsWith(ProgressPrefix, StringComparison.Ordinal))
            {
                var p = ParseProgress(line[ProgressPrefix.Length..]);
                if (p is not null) progress.Report(p);
            }
            else if (line.StartsWith(TitlePrefix, StringComparison.Ordinal))
                onTitle(line[TitlePrefix.Length..]);
            else if (line.StartsWith(IdPrefix, StringComparison.Ordinal))
                onId(line[IdPrefix.Length..]);
            else if (line.StartsWith(FilePrefix, StringComparison.Ordinal))
                filePath = line[FilePrefix.Length..];
        };
        proc.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data)) errors.Add(e.Data.Trim());
        };

        try
        {
            if (!proc.Start()) return new(false, null, Loc.T("CouldNotStart"));
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            using var reg = ct.Register(() =>
            {
                try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { /* ya terminó */ }
            });

            await proc.WaitForExitAsync(CancellationToken.None);
            proc.WaitForExit(); // vacía los buffers de salida asíncronos

            if (ct.IsCancellationRequested) return new(false, null, Loc.T("Cancelled"));
            if (proc.ExitCode == 0) return new(true, filePath, null);

            var err = errors.LastOrDefault(l => l.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                      ?? errors.LastOrDefault()
                      ?? Loc.T("ExitCode", proc.ExitCode);
            return new(false, null, err);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new(false, null, ex.Message);
        }
    }

    /// <summary>Consulta los metadatos de la URL (título, duración y resoluciones disponibles).</summary>
    public async Task<VideoInfo> GetInfoAsync(string url, CancellationToken ct)
    {
        try { return await GetInfoCoreAsync(url, ct, CookiesActive); }
        catch (InvalidOperationException ex) when (CookiesActive && ErrorHints.IsCookieReadError(ex.Message))
        {
            return (await GetInfoCoreAsync(url, ct, useCookies: false)) with { CookiesSkipped = true };
        }
    }

    private async Task<VideoInfo> GetInfoCoreAsync(string url, CancellationToken ct, bool useCookies)
    {
        var psi = NewStartInfo(useCookies);
        foreach (var a in new[] { "-J", "--no-playlist", "--no-warnings", "--no-colors", "--", url })
            psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException(Loc.T("CouldNotStart"));
        using var reg = ct.Register(() =>
        {
            try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { }
        });

        var stdoutTask = proc.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderrTask = proc.StandardError.ReadToEndAsync(CancellationToken.None);
        await proc.WaitForExitAsync(CancellationToken.None);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        ct.ThrowIfCancellationRequested();
        if (proc.ExitCode != 0)
        {
            var err = stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault(l => l.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                ?? Loc.T("ExitCode", proc.ExitCode);
            throw new InvalidOperationException(err);
        }

        using var doc = JsonDocument.Parse(stdout);
        var root = doc.RootElement;

        string? Str(string name) =>
            root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        TimeSpan? duration = root.TryGetProperty("duration", out var d) && d.ValueKind == JsonValueKind.Number
            ? TimeSpan.FromSeconds(d.GetDouble()) : null;

        var heights = new SortedSet<int>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        if (root.TryGetProperty("formats", out var formats) && formats.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in formats.EnumerateArray())
            {
                if (f.TryGetProperty("height", out var h) && h.ValueKind == JsonValueKind.Number &&
                    f.TryGetProperty("vcodec", out var vc) && vc.GetString() is { } v && v != "none")
                    heights.Add(h.GetInt32());
            }
        }

        return new VideoInfo(Str("title") ?? url, Str("uploader") ?? Str("channel"), duration, heights.ToList());
    }

    private ProcessStartInfo NewStartInfo(bool useCookies = true)
    {
        var psi = new ProcessStartInfo(_tools.YtDlpPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _tools.ToolsDir,
        };
        psi.Environment["PYTHONIOENCODING"] = "utf-8";
        psi.Environment["PYTHONUTF8"] = "1";
        // Sin esto, el .exe de yt-dlp ignora PYTHONIOENCODING y devuelve "?" / "�" en lugar de
        // los acentos y la ñ (títulos, rutas y errores). Es lo que garantiza el UTF-8 en la salida.
        psi.ArgumentList.Add("--encoding");
        psi.ArgumentList.Add("utf-8");
        psi.ArgumentList.Add("--ffmpeg-location");
        psi.ArgumentList.Add(_tools.ToolsDir);
        psi.ArgumentList.Add("--js-runtimes");
        psi.ArgumentList.Add($"deno:{_tools.DenoPath}");
        if (useCookies && CookiesBrowser is { Length: > 0 } browser)
        {
            psi.ArgumentList.Add("--cookies-from-browser");
            psi.ArgumentList.Add(browser);
        }
        return psi;
    }

    private static DownloadProgress? ParseProgress(string s)
    {
        // status|downloaded|total|total_estimate|speed|eta   (valores no disponibles = "NA")
        var f = s.Split('|');
        if (f.Length < 6) return null;

        double? Num(string v) =>
            double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

        var downloaded = Num(f[1]);
        var total = Num(f[2]) ?? Num(f[3]);
        var percent = downloaded is not null && total is > 0 ? downloaded.Value / total.Value * 100 : 0;
        var eta = Num(f[5]);

        return new DownloadProgress(f[0], Math.Clamp(percent, 0, 100), Num(f[4]), eta is null ? null : (int)eta.Value);
    }
}
