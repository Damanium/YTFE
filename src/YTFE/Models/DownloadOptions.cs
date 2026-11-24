using YTFE.Infrastructure;
using YTFE.Localization;

namespace YTFE.Models;

public enum MediaKind { Video, Audio }

public enum VideoCodec { Auto, H264, VP9, AV1 }

/// <summary>
/// Elemento de un ComboBox: texto visible + valor. Si el texto viene de una clave de idioma,
/// se actualiza solo cuando el usuario cambia de idioma.
/// </summary>
public sealed class Choice<T> : ObservableObject
{
    private readonly string? _key;
    private readonly string? _literal;

    internal Choice(string? key, string? literal, T value)
    {
        _key = key;
        _literal = literal;
        Value = value;
        if (key is not null) Loc.Instance.LanguageChanged += () => OnPropertyChanged(nameof(Label));
    }

    public T Value { get; }
    public string Label => _key is null ? _literal! : Loc.T(_key);
    public override string ToString() => Label;
}

public static class Choice
{
    /// <summary>Texto traducible (clave de idioma).</summary>
    public static Choice<T> L<T>(string key, T value) => new(key, null, value);

    /// <summary>Texto fijo que no se traduce (p. ej. "MP3", "1080p").</summary>
    public static Choice<T> Lit<T>(string text, T value) => new(null, text, value);
}

public sealed record DownloadProgress(
    string Status,
    double Percent,
    double? SpeedBytesPerSec,
    int? EtaSeconds);

public sealed record VideoInfo(
    string Title,
    string? Uploader,
    TimeSpan? Duration,
    IReadOnlyList<int> Heights,
    bool CookiesSkipped = false);

/// <summary>Todo lo que el usuario elige para una descarga. Se traduce a argumentos de yt-dlp.</summary>
public sealed record DownloadOptions(
    MediaKind Kind,
    int? MaxHeight,            // null = la mejor disponible
    string Container,          // mp4 | mkv | webm
    VideoCodec Codec,
    string AudioFormat,        // mp3 | vorbis | wav | flac | m4a | opus
    string AudioQuality,       // "best" o bitrate, p. ej. "320K"
    bool EmbedMetadata,
    bool EmbedThumbnail,
    bool Playlist = false,     // true = si la URL es una lista/álbum/set, descargarla entera
    string? CutFrom = null,    // recorte opcional (H:MM:SS, M:SS o segundos); null = desde el principio
    string? CutTo = null)      // null = hasta el final
{
    private static readonly System.Text.RegularExpressions.Regex TimeRx = new(@"^\d{1,5}(:\d{2}){0,2}$");

    /// <summary>Vacío o un tiempo válido (S, M:SS o H:MM:SS).</summary>
    public static bool IsValidTime(string? t) => string.IsNullOrWhiteSpace(t) || TimeRx.IsMatch(t.Trim());

    public bool HasCut => !string.IsNullOrWhiteSpace(CutFrom) || !string.IsNullOrWhiteSpace(CutTo);

    public static bool IsLossless(string audioFormat) => audioFormat is "wav" or "flac";

    public static bool CanEmbedThumbnailFor(MediaKind kind, string container, string audioFormat) =>
        kind == MediaKind.Video ? container != "webm" : audioFormat != "wav";

    public IReadOnlyList<string> ToArgs()
    {
        var a = new List<string>();

        if (Kind == MediaKind.Video)
        {
            var h = MaxHeight is int m ? $"[height<={m}]" : "";
            if (Container == "webm")
                a.AddRange(["-f", $"bv*{h}[ext=webm]+ba[ext=webm]/b{h}[ext=webm]/bv*{h}+ba/b{h}"]);
            else
                a.AddRange(["-f", $"bv*{h}+ba/b{h}"]);

            var codec = Codec switch
            {
                VideoCodec.H264 => "h264",
                VideoCodec.VP9 => "vp9",
                VideoCodec.AV1 => "av01",
                _ => null,
            };
            // -S solo prioriza (no excluye): si no existe ese códec, se usa el siguiente mejor.
            if (codec is not null) a.AddRange(["-S", $"vcodec:{codec}"]);

            a.AddRange(["--merge-output-format", Container]);
        }
        else
        {
            a.AddRange(["-f", "ba/b", "-x", "--audio-format", AudioFormat]);

            if (!IsLossless(AudioFormat))
            {
                if (AudioQuality == "best")
                {
                    // VBR máximo (V0) solo tiene sentido en MP3; en el resto, yt-dlp elige.
                    if (AudioFormat == "mp3") a.AddRange(["--audio-quality", "0"]);
                }
                else a.AddRange(["--audio-quality", AudioQuality]);
            }
        }

        if (HasCut && IsValidTime(CutFrom) && IsValidTime(CutTo))
        {
            var from = string.IsNullOrWhiteSpace(CutFrom) ? "0" : CutFrom.Trim();
            var to = string.IsNullOrWhiteSpace(CutTo) ? "inf" : CutTo.Trim();
            a.AddRange(["--download-sections", $"*{from}-{to}"]);
        }

        if (EmbedMetadata) a.Add("--embed-metadata");
        if (EmbedThumbnail && CanEmbedThumbnailFor(Kind, Container, AudioFormat))
            a.AddRange(["--embed-thumbnail", "--convert-thumbnails", "jpg"]);

        return a;
    }

    /// <summary>Texto corto para mostrar en la lista de descargas.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string Summary
    {
        get => BaseSummary + (Playlist ? " · " + Loc.T("SumPlaylist") : "") + (HasCut ? $" · {CutFrom?.Trim()}–{CutTo?.Trim()}" : "");
    }

    private string BaseSummary
    {
        get
        {
            if (Kind == MediaKind.Video)
            {
                var res = MaxHeight is int m ? Loc.T("SumUpTo", m) : Loc.T("SumBest");
                var codec = Codec == VideoCodec.Auto ? "" : $" · {Codec}";
                return $"{Container.ToUpperInvariant()} · {res}{codec}";
            }
            var fmt = AudioFormat == "vorbis" ? "OGG" : AudioFormat.ToUpperInvariant();
            var q = IsLossless(AudioFormat) ? Loc.T("SumLossless")
                  : AudioQuality == "best" ? Loc.T("SumBest")
                  : AudioQuality.Replace("K", " kbps");
            return $"{fmt} · {q}";
        }
    }
}
