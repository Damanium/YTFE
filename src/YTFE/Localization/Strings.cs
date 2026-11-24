namespace YTFE.Localization;

/// <summary>Todos los textos de la interfaz, en español e inglés. Una clave = una entrada en cada idioma.</summary>
internal static class Strings
{
    public static readonly IReadOnlyDictionary<string, string> Spanish;
    public static readonly IReadOnlyDictionary<string, string> English;

    static Strings()
    {
        var es = new Dictionary<string, string>();
        var en = new Dictionary<string, string>();
        void Add(string key, string spanish, string english) { es[key] = spanish; en[key] = english; }

        // ---- Ventana principal -------------------------------------------------
        Add("Analyze", "Analizar", "Analyze");
        Add("Download", "Descargar", "Download");
        Add("OptionsHeader", "Opciones de descarga", "Download options");
        Add("Video", "Vídeo", "Video");
        Add("AudioOnly", "Solo audio", "Audio only");
        Add("Resolution", "Resolución", "Resolution");
        Add("Container", "Contenedor", "Container");
        Add("VideoCodec", "Códec de vídeo", "Video codec");
        Add("AudioFormat", "Formato de audio", "Audio format");
        Add("AudioQuality", "Calidad (bitrate)", "Quality (bitrate)");
        Add("LosslessNote", "WAV y FLAC conservan la calidad original, sin bitrate.",
                            "WAV and FLAC keep the original quality, with no bitrate.");
        Add("EmbedMetadata", "Incrustar metadatos (título, artista…)", "Embed metadata (title, artist…)");
        Add("EmbedThumbnail", "Incrustar miniatura / carátula", "Embed thumbnail / cover art");
        Add("SaveTo", "Guardar en:", "Save to:");
        Add("Browse", "Examinar…", "Browse…");
        Add("BrowseTitle", "Carpeta de descargas", "Download folder");
        Add("Cancel", "Cancelar", "Cancel");

        // ---- Acciones sobre una descarga -------------------------------------
        Add("Pause", "Pausar", "Pause");
        Add("Resume", "Reanudar", "Resume");
        Add("Retry", "Reintentar", "Retry");
        Add("OpenFile", "Abrir", "Open");
        Add("More", "Más acciones", "More actions");
        Add("ShowInFolder", "Mostrar en carpeta", "Show in folder");
        Add("CopyLink", "Copiar enlace", "Copy link");
        Add("OpenInBrowser", "Abrir en el navegador", "Open in browser");
        Add("RemoveFromList", "Quitar de la lista", "Remove from list");
        Add("DeleteFiles", "Eliminar archivos…", "Delete files…");
        Add("PauseAll", "Pausar todo", "Pause all");
        Add("ResumeAll", "Reanudar todo", "Resume all");
        Add("ClearFinished", "Limpiar finalizadas", "Clear finished");
        Add("ConfirmDeleteFile", "¿Enviar a la papelera este archivo?\n\n{0}", "Move this file to the Recycle Bin?\n\n{0}");
        Add("ConfirmDeletePartial", "¿Eliminar los archivos parciales de esta descarga?\n\n{0}", "Delete the partial files of this download?\n\n{0}");
        Add("ConfirmCancel", "Si cancelas, se descartará lo ya descargado ({0} %) y, si la reintentas, empezará desde cero.\n\nSi solo quieres parar y seguir más tarde, usa «Pausar».\n\n¿Cancelar la descarga?",
                             "If you cancel, what has already been downloaded ({0}%) will be discarded and, if you retry, it will start from scratch.\n\nIf you only want to stop and continue later, use \"Pause\".\n\nCancel the download?");
        Add("DeleteFailed", "No se pudo eliminar: {0}", "Could not delete: {0}");
        Add("ConfirmExit", "Hay descargas en curso. Si cierras la aplicación se detendrán y quedarán en pausa; podrás reanudarlas al volver a abrirla.\n\n¿Quieres salir?",
                           "There are downloads in progress. If you close the app they will be stopped and left paused; you can resume them when you reopen it.\n\nDo you want to exit?");
        Add("UpdateTools", "Actualizar herramientas", "Update tools");
        Add("Language", "Idioma", "Language");
        Add("HelpButton", "Ayuda", "Help");
        Add("HelpTitle", "Ayuda de YTFE", "YTFE Help");
        Add("HelpSearch", "Buscar en la ayuda…", "Search the help…");
        Add("HelpNoResults", "No hay temas que coincidan con tu búsqueda.", "No topics match your search.");
        Add("Tagline", "Descargador de vídeo y audio", "Video & audio downloader");
        Add("Theme", "Tema", "Theme");
        Add("ThemeSystem", "Sistema", "System");
        Add("ThemeLight", "Claro", "Light");
        Add("ThemeDark", "Oscuro", "Dark");
        Add("UrlHint", "Pega aquí el enlace del vídeo…", "Paste the video link here…");
        Add("EmptyHint", "Aún no hay descargas.\nPega un enlace arriba y pulsa «Descargar».", "No downloads yet.\nPaste a link above and press \"Download\".");
        Add("CookiesLabel", "Cookies:", "Cookies:");
        Add("CookiesNone", "Ninguno", "None");
        Add("CookiesTip", "Usa tu sesión del navegador para sitios que piden iniciar sesión (Instagram, X, Facebook, vídeos privados o con restricción de edad…).\nFirefox es el más fiable; con Chrome, Edge o Brave puede fallar si el navegador está abierto.",
                          "Use your browser session for sites that require login (Instagram, X, Facebook, private or age-restricted videos…).\nFirefox is the most reliable; Chrome, Edge or Brave may fail while the browser is open.");

        // ---- Sitios, listas y recortes -----------------------------------------
        Add("SitesButton", "Sitios compatibles", "Supported sites");
        Add("SitesTitle", "Sitios compatibles", "Supported sites");
        Add("SitesIntro", "Pega el enlace de cualquiera de estos sitios (y de cientos más). Algunos exigen iniciar sesión: usa el selector «Cookies».",
                          "Paste a link from any of these sites (and hundreds more). Some require login: use the \"Cookies\" selector.");
        Add("SitesMore", "…y muchos más: televisiones, medios de noticias, universidades. Todo lo que aparezca en la lista completa funciona.",
                         "…and many more: TV channels, news outlets, universities. Anything in the full list works.");
        Add("SitesNoDrm", "No funcionan los servicios con DRM (Netflix, Disney+, Prime Video, Spotify…).",
                          "Services with DRM do not work (Netflix, Disney+, Prime Video, Spotify…).");
        Add("SitesFullList", "Ver la lista completa", "See the full list");
        Add("Playlist", "Descargar lista / álbum completo", "Download whole playlist / album");
        Add("CutLabel", "Recortar (opcional):", "Trim (optional):");
        Add("CutFrom", "desde", "from");
        Add("CutTo", "hasta", "to");
        Add("CutHint", "Formato 90, 1:30 o 1:02:03. Vacío = sin recorte.", "Format 90, 1:30 or 1:02:03. Empty = no trim.");
        Add("SumPlaylist", "lista completa", "whole playlist");

        // ---- Errores de yt-dlp explicados ----------------------------------------
        Add("ErrLogin", "Este contenido exige iniciar sesión (o es privado / con restricción de edad). Inicia sesión en tu navegador y elige ese navegador en «Cookies».",
                        "This content requires login (or is private / age-restricted). Log in in your browser and pick that browser under \"Cookies\".");
        Add("ErrLoginWithCookies", "Sigue pidiendo sesión. Comprueba que has iniciado sesión en ese navegador con una cuenta con acceso, o prueba con Firefox.",
                                   "It still asks for login. Make sure you are logged in in that browser with an account that has access, or try Firefox.");
        Add("ErrCookiesDb", "No se pudieron leer las cookies del navegador. Cierra el navegador y reintenta, o elige Firefox.",
                            "Could not read the browser cookies. Close the browser and retry, or pick Firefox.");
        Add("CookiesSkippedNote", "No se pudieron leer las cookies del navegador: se usó sin ellas.",
                                  "Browser cookies could not be read: used without them.");
        Add("ErrDrm", "Este contenido tiene protección DRM y no se puede descargar.", "This content is DRM-protected and cannot be downloaded.");
        Add("ErrGeo", "Este vídeo no está disponible en tu país o región.", "This video is not available in your country or region.");
        Add("ErrUnsupported", "Este enlace no es de un sitio compatible o no apunta a un vídeo. Revisa la URL.",
                              "This link is not from a supported site or does not point to a video. Check the URL.");
        Add("ErrUnavailable", "El vídeo no está disponible (borrado, privado o enlace incorrecto).",
                              "The video is unavailable (deleted, private or wrong link).");
        Add("ErrRate", "El sitio está limitando las peticiones. Espera unos minutos y reintenta.",
                       "The site is rate-limiting requests. Wait a few minutes and retry.");
        Add("ErrNetwork", "No se pudo conectar con el sitio. Comprueba tu conexión a internet y reintenta.",
                          "Could not connect to the site. Check your internet connection and retry.");

        // ---- Opciones de los desplegables ------------------------------------
        Add("ResBest", "Mejor disponible", "Best available");
        Add("ContMp4", "MP4 (más compatible)", "MP4 (most compatible)");
        Add("ContMkv", "MKV (admite todo)", "MKV (supports everything)");
        Add("CodecAuto", "Automático (mejor calidad)", "Automatic (best quality)");
        Add("CodecH264", "H.264 (máx. compatibilidad)", "H.264 (max compatibility)");
        Add("CodecAv1", "AV1 (menor tamaño)", "AV1 (smaller size)");
        Add("FmtWav", "WAV (sin compresión)", "WAV (uncompressed)");
        Add("FmtFlac", "FLAC (sin pérdida)", "FLAC (lossless)");
        Add("QualBest", "Mejor calidad", "Best quality");

        // ---- Resumen de cada descarga ----------------------------------------
        Add("SumUpTo", "hasta {0}p", "up to {0}p");
        Add("SumBest", "mejor calidad", "best quality");
        Add("SumLossless", "sin pérdida", "lossless");

        // ---- Estados de una descarga -----------------------------------------
        Add("State_Queued", "En cola", "Queued");
        Add("State_Downloading", "Descargando", "Downloading");
        Add("State_Processing", "Procesando", "Processing");
        Add("State_Paused", "En pausa", "Paused");
        Add("State_Completed", "Completado", "Completed");
        Add("State_Error", "Error", "Error");
        Add("State_Cancelled", "Cancelado", "Cancelled");
        Add("Eta", "ETA", "ETA");

        // ---- Mensajes de la barra de estado ----------------------------------
        Add("Starting", "Iniciando…", "Starting…");
        Add("ToolsReady", "Herramientas listas.", "Tools ready.");
        Add("ToolsDownloadFailed", "No se pudieron descargar las herramientas: {0}", "Could not download the tools: {0}");
        Add("CheckingYtDlp", "Buscando actualizaciones de yt-dlp…", "Checking for yt-dlp updates…");
        Add("YtDlpCheckFailed", "No se pudo comprobar yt-dlp: {0}", "Could not check yt-dlp: {0}");
        Add("WaitDownloads", "Espera a que terminen las descargas en curso antes de actualizar.",
                             "Wait for the running downloads to finish before updating.");
        Add("UpdateFailed", "Error al actualizar: {0}", "Update failed: {0}");
        Add("Analyzing", "Analizando…", "Analyzing…");
        Add("AnalyzeFailed", "No se pudo analizar: {0}", "Could not analyze: {0}");

        // ---- Gestor de herramientas ------------------------------------------
        Add("DlYtDlp", "Descargando yt-dlp…", "Downloading yt-dlp…");
        Add("DlFfmpeg", "Descargando ffmpeg (~200 MB, solo la primera vez)…", "Downloading ffmpeg (~200 MB, first time only)…");
        Add("DlDeno", "Descargando deno (necesario para YouTube)…", "Downloading deno (required for YouTube)…");
        Add("YtDlpInstalled", "yt-dlp instalado.", "yt-dlp installed.");
        Add("YtDlpUnchanged", "yt-dlp sin cambios.", "yt-dlp unchanged.");
        Add("CouldNotLaunch", "No se pudo lanzar yt-dlp.", "Could not launch yt-dlp.");
        Add("CouldNotStart", "No se pudo iniciar yt-dlp.", "Could not start yt-dlp.");
        Add("DenoUpToDate", "deno {0} ya está al día.", "deno {0} is up to date.");
        Add("DenoVersionUnknown", "No se pudo leer la última versión de deno.", "Could not read the latest deno version.");
        Add("DenoUpdated", "deno actualizado a {0}.", "deno updated to {0}.");
        Add("FfmpegVersionUnknown", "No se pudo consultar la última versión de ffmpeg.", "Could not query the latest ffmpeg version.");
        Add("FfmpegUpToDate", "ffmpeg ya está al día.", "ffmpeg is up to date.");
        Add("FfmpegUpdated", "ffmpeg actualizado.", "ffmpeg updated.");
        Add("UpdatingTool", "Actualizando {0}…", "Updating {0}…");
        Add("ToolStepError", "{0}: error ({1})", "{0}: error ({1})");

        // ---- Servicio de descarga --------------------------------------------
        Add("Cancelled", "Cancelado.", "Cancelled.");
        Add("ExitCode", "yt-dlp terminó con código {0}.", "yt-dlp exited with code {0}.");

        Spanish = es;
        English = en;
    }
}
