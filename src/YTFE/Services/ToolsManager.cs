using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using YTFE.Localization;

namespace YTFE.Services;

public sealed record ToolVersions(string YtDlp, string Ffmpeg, string Deno);

/// <summary>
/// Descarga, localiza y actualiza los binarios externos (yt-dlp, ffmpeg, deno) en
/// %LOCALAPPDATA%\YTFE\tools. Así se pueden actualizar sin recompilar la app.
/// </summary>
public sealed class ToolsManager
{
    private const string YtDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
    private const string FfmpegAsset = "ffmpeg-master-latest-win64-gpl.zip";
    private const string FfmpegUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/latest/download/" + FfmpegAsset;
    private const string FfmpegApi = "https://api.github.com/repos/yt-dlp/FFmpeg-Builds/releases/latest";
    private const string DenoUrl = "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip";
    private const string DenoApi = "https://api.github.com/repos/denoland/deno/releases/latest";

    private static readonly TimeSpan AutoCheckInterval = TimeSpan.FromHours(24);
    private static readonly HttpClient Http = CreateClient();

    private readonly string _rootDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YTFE");

    public string ToolsDir => Path.Combine(_rootDir, "tools");
    public string YtDlpPath => Path.Combine(ToolsDir, "yt-dlp.exe");
    public string FfmpegPath => Path.Combine(ToolsDir, "ffmpeg.exe");
    public string DenoPath => Path.Combine(ToolsDir, "deno.exe");
    private string StatePath => Path.Combine(_rootDir, "state.json");

    public bool AllPresent => File.Exists(YtDlpPath) && File.Exists(FfmpegPath) && File.Exists(DenoPath);

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("YTFE/1.0");
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    // ------------------------------------------------------------------ Estado

    private sealed class State
    {
        public DateTime? LastAutoCheckUtc { get; set; }
        public string? FfmpegStamp { get; set; }
    }

    private State LoadState()
    {
        try { return JsonSerializer.Deserialize<State>(File.ReadAllText(StatePath)) ?? new(); }
        catch { return new(); }
    }

    private void SaveState(State s)
    {
        try
        {
            Directory.CreateDirectory(_rootDir);
            File.WriteAllText(StatePath, JsonSerializer.Serialize(s));
        }
        catch { /* el estado es una optimización: si no se puede guardar, no es grave */ }
    }

    /// <summary>¿Han pasado más de 24 h desde la última comprobación automática de yt-dlp?</summary>
    public bool IsAutoCheckDue()
    {
        var last = LoadState().LastAutoCheckUtc;
        return last is null || DateTime.UtcNow - last.Value >= AutoCheckInterval;
    }

    public void MarkAutoChecked()
    {
        var s = LoadState();
        s.LastAutoCheckUtc = DateTime.UtcNow;
        SaveState(s);
    }

    // --------------------------------------------------------------- Instalación

    /// <summary>Descarga las herramientas que falten.</summary>
    public async Task EnsureAsync(IProgress<string>? status = null, CancellationToken ct = default)
    {
        Directory.CreateDirectory(ToolsDir);

        if (!File.Exists(YtDlpPath))
        {
            status?.Report(Loc.T("DlYtDlp"));
            await DownloadFileAsync(YtDlpUrl, YtDlpPath, ct);
        }
        if (!File.Exists(FfmpegPath))
        {
            status?.Report(Loc.T("DlFfmpeg"));
            var stamp = await TryGetFfmpegStampAsync(ct); // antes de descargar: si cambia durante, se actualizará luego
            await DownloadZipEntriesAsync(FfmpegUrl, ["ffmpeg.exe", "ffprobe.exe"], ct);
            RememberFfmpegStamp(stamp);
        }
        if (!File.Exists(DenoPath))
        {
            status?.Report(Loc.T("DlDeno"));
            await DownloadZipEntriesAsync(DenoUrl, ["deno.exe"], ct);
        }
        status?.Report(Loc.T("ToolsReady"));
    }

    // ----------------------------------------------------------------- Versiones

    public async Task<ToolVersions> GetVersionsAsync(CancellationToken ct = default)
    {
        var yt = RunFirstLineAsync(YtDlpPath, ["--version"], ct);
        var ff = RunFirstLineAsync(FfmpegPath, ["-version"], ct);
        var dn = RunFirstLineAsync(DenoPath, ["--version"], ct);
        await Task.WhenAll(yt, ff, dn);

        return new ToolVersions(
            (await yt) ?? "—",
            ParseToken(await ff, "ffmpeg version") ?? "—",
            ParseDenoVersion(await dn)?.ToString() ?? "—");
    }

    private static string? ParseToken(string? line, string prefix)
    {
        if (line is null || !line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        var token = line[prefix.Length..].Trim().Split(' ')[0];
        return token.Length > 22 ? token[..22] + "…" : token;
    }

    private static Version? ParseDenoVersion(string? line)
    {
        // "deno 2.9.8 (stable, release, x86_64-pc-windows-msvc)"
        if (line is null) return null;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && Version.TryParse(parts[1], out var v) ? v : null;
    }

    private static async Task<string?> RunFirstLineAsync(string exe, string[] args, CancellationToken ct)
    {
        if (!File.Exists(exe)) return null;
        try
        {
            var psi = new ProcessStartInfo(exe)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            using var p = Process.Start(psi);
            if (p is null) return null;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            var stdout = p.StandardOutput.ReadToEndAsync(timeout.Token);
            _ = p.StandardError.ReadToEndAsync(timeout.Token);
            try { await p.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                try { p.Kill(true); } catch { }
                return null;
            }
            return (await stdout).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }
        catch { return null; }
    }

    // ------------------------------------------------------------ Actualizaciones

    /// <summary>Actualiza yt-dlp.exe a la última versión (yt-dlp -U).</summary>
    public async Task<string> UpdateYtDlpAsync(CancellationToken ct = default)
    {
        if (!File.Exists(YtDlpPath))
        {
            Directory.CreateDirectory(ToolsDir);
            await DownloadFileAsync(YtDlpUrl, YtDlpPath, ct);
            return Loc.T("YtDlpInstalled");
        }

        var psi = new ProcessStartInfo(YtDlpPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("-U");
        using var p = Process.Start(psi) ?? throw new InvalidOperationException(Loc.T("CouldNotLaunch"));
        var stdout = p.StandardOutput.ReadToEndAsync(ct);
        var stderr = p.StandardError.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);
        var text = ((await stdout) + (await stderr)).Trim();
        if (p.ExitCode != 0) throw new InvalidOperationException(text.Split('\n').Last().Trim());
        return text.Length == 0 ? Loc.T("YtDlpUnchanged") : text.Split('\n').Last().Trim();
    }

    /// <summary>Actualiza deno solo si hay una versión más nueva en GitHub.</summary>
    public async Task<string> UpdateDenoAsync(CancellationToken ct = default)
    {
        var installed = ParseDenoVersion(await RunFirstLineAsync(DenoPath, ["--version"], ct));

        using var doc = JsonDocument.Parse(await Http.GetStringAsync(DenoApi, ct));
        var tag = doc.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v');
        if (!Version.TryParse(tag, out var latest))
            throw new InvalidOperationException(Loc.T("DenoVersionUnknown"));

        if (installed is not null && installed >= latest) return Loc.T("DenoUpToDate", installed);

        Directory.CreateDirectory(ToolsDir);
        await DownloadZipEntriesAsync(DenoUrl, ["deno.exe"], ct);
        return Loc.T("DenoUpdated", latest);
    }

    /// <summary>Actualiza ffmpeg solo si el build publicado es más reciente que el instalado.</summary>
    public async Task<string> UpdateFfmpegAsync(CancellationToken ct = default)
    {
        var latest = await TryGetFfmpegStampAsync(ct)
            ?? throw new InvalidOperationException(Loc.T("FfmpegVersionUnknown"));

        var state = LoadState();
        if (state.FfmpegStamp is null)
        {
            // Instalación anterior a este control: se asume actual y se toma como referencia.
            state.FfmpegStamp = latest;
            SaveState(state);
            return Loc.T("FfmpegUpToDate");
        }
        if (state.FfmpegStamp == latest) return Loc.T("FfmpegUpToDate");

        Directory.CreateDirectory(ToolsDir);
        await DownloadZipEntriesAsync(FfmpegUrl, ["ffmpeg.exe", "ffprobe.exe"], ct);
        RememberFfmpegStamp(latest);
        return Loc.T("FfmpegUpdated");
    }

    /// <summary>Actualiza las tres herramientas. Un fallo en una no impide actualizar las demás.</summary>
    public async Task<string> UpdateAllAsync(IProgress<string>? status = null, CancellationToken ct = default)
    {
        var results = new List<string>();
        async Task Step(string label, Func<Task<string>> action)
        {
            status?.Report(Loc.T("UpdatingTool", label));
            try { results.Add(await action()); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { results.Add(Loc.T("ToolStepError", label, ex.Message)); }
        }

        await Step("yt-dlp", () => UpdateYtDlpAsync(ct));
        await Step("deno", () => UpdateDenoAsync(ct));
        await Step("ffmpeg", () => UpdateFfmpegAsync(ct));
        return string.Join(" · ", results);
    }

    private async Task<string?> TryGetFfmpegStampAsync(CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(FfmpegApi, ct));
            foreach (var a in doc.RootElement.GetProperty("assets").EnumerateArray())
                if (a.GetProperty("name").GetString() == FfmpegAsset)
                    return a.GetProperty("updated_at").GetString();
        }
        catch (OperationCanceledException) { throw; }
        catch { /* sin red o límite de la API de GitHub */ }
        return null;
    }

    private void RememberFfmpegStamp(string? stamp)
    {
        if (stamp is null) return;
        var s = LoadState();
        s.FfmpegStamp = stamp;
        SaveState(s);
    }

    // ------------------------------------------------------------------ Descargas

    private static async Task DownloadFileAsync(string url, string dest, CancellationToken ct)
    {
        var tmp = dest + ".part";
        try
        {
            using var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            await using (var fs = File.Create(tmp))
                await resp.Content.CopyToAsync(fs, ct);
            File.Move(tmp, dest, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    private async Task DownloadZipEntriesAsync(string url, string[] fileNames, CancellationToken ct)
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"ytfe_{Guid.NewGuid():N}.zip");
        try
        {
            await DownloadFileAsync(url, zipPath, ct);
            using var zip = ZipFile.OpenRead(zipPath);
            foreach (var name in fileNames)
            {
                var entry = zip.Entries.FirstOrDefault(e =>
                    string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
                if (entry is null) continue;

                // Solo el nombre del archivo (evita rutas maliciosas dentro del zip) y a un temporal
                // primero, para no dejar un .exe a medias si falla la extracción.
                var dest = Path.Combine(ToolsDir, name);
                var tmp = dest + ".new";
                entry.ExtractToFile(tmp, overwrite: true);
                File.Move(tmp, dest, overwrite: true);
            }
        }
        finally
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
        }
    }
}
