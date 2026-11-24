using System.Text.Json;
using System.Text.Json.Serialization;
using YTFE.Models;

namespace YTFE.Services;

/// <summary>Lo mínimo necesario para volver a mostrar (y reanudar) una descarga en la siguiente sesión.</summary>
public sealed record DownloadSnapshot(
    string Url,
    DownloadOptions Options,
    string OutputFolder,
    string Title,
    DownloadState State,
    double Percent,
    string? VideoId,
    string? FilePath,
    string? Error);

/// <summary>Guarda y carga la lista de descargas en %LOCALAPPDATA%\YTFE\downloads.json.</summary>
public sealed class DownloadStore
{
    private const int MaxItems = 500;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        // Sin escapar tildes y ñ (ó…): el archivo queda legible y no hay riesgo de "?" al editarlo.
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed class FileModel
    {
        public int Version { get; set; } = 1;
        public List<DownloadSnapshot> Items { get; set; } = [];
    }

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YTFE", "downloads.json");

    public List<DownloadSnapshot> Load()
    {
        if (!File.Exists(FilePath)) return [];
        try
        {
            var model = JsonSerializer.Deserialize<FileModel>(File.ReadAllText(FilePath), JsonOptions);
            return model?.Items?.Where(i => i is not null && i.Options is not null).ToList() ?? [];
        }
        catch (Exception)
        {
            // Archivo corrupto: se aparta para no perderlo del todo y se empieza con la lista vacía.
            try { File.Move(FilePath, FilePath + ".bad", overwrite: true); } catch { }
            return [];
        }
    }

    public void Save(IEnumerable<DownloadSnapshot> items)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var model = new FileModel { Items = items.Take(MaxItems).ToList() };

            // Escritura atómica: si la app se cierra a mitad, el archivo anterior sigue intacto.
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(model, JsonOptions));
            File.Move(tmp, FilePath, overwrite: true);
        }
        catch
        {
            // Guardar la lista es secundario: un fallo aquí no debe interrumpir las descargas.
        }
    }
}
