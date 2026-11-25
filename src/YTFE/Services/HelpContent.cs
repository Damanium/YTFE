namespace YTFE.Services;

/// <summary>Un tema de la ayuda. El cuerpo usa un marcado mínimo: ver HelpRenderer.</summary>
public sealed record HelpTopic(string Id, string TitleEs, string TitleEn, string BodyEs, string BodyEn)
{
    public string Title(string lang) => lang == "es" ? TitleEs : TitleEn;
    public string Body(string lang) => lang == "es" ? BodyEs : BodyEn;
}

/// <summary>
/// Texto de la ayuda en español e inglés. Marcado: "## " subtítulo, "- " viñeta, "1. " paso numerado,
/// "> " consejo destacado, **negrita**, `código`. Una línea en blanco separa párrafos.
/// </summary>
public static class HelpContent
{
    public static IReadOnlyList<HelpTopic> Topics { get; } =
    [
        new("start",
            "Primeros pasos", "Getting started",
            """
            YTFE descarga **vídeo y audio** de YouTube y de cientos de sitios más. No necesitas instalar nada aparte: la primera vez, la aplicación descarga sola las herramientas que usa.

            ## Tu primera descarga en 5 pasos
            1. **Copia el enlace** del vídeo desde la barra de direcciones de tu navegador (o con «Copiar enlace» de la web).
            2. **Pégalo** en el campo grande de arriba.
            3. Elige **Vídeo** o **Solo audio**.
            4. Comprueba la carpeta de **Guardar en**.
            5. Pulsa **Descargar** (o la tecla Enter).

            La descarga aparece en la lista de abajo con su barra de progreso. Cuando termina, pulsa **Abrir** para verla o escucharla.

            > Si no quieres complicarte, deja todas las opciones como están: «Mejor disponible», MP4 y códec Automático dan un buen resultado en la mayoría de casos.

            ## El primer arranque
            La primera vez que abres la aplicación se descargan tres herramientas (unos 260 MB en total): **yt-dlp**, **ffmpeg** y **deno**. Verás el progreso en la barra de estado de abajo. Mientras tanto, los botones **Descargar** y **Analizar** están desactivados. Los arranques siguientes son inmediatos.

            ## Atajos útiles
            - **Enter**: descargar el enlace pegado.
            - **F1**: abrir esta ayuda.
            - **Clic derecho** sobre una descarga: todas sus acciones.
            """,
            """
            YTFE downloads **video and audio** from YouTube and hundreds of other sites. You don't need to install anything else: the first time, the app downloads the tools it uses by itself.

            ## Your first download in 5 steps
            1. **Copy the video link** from your browser's address bar (or with the site's "Copy link" button).
            2. **Paste it** into the big field at the top.
            3. Choose **Video** or **Audio only**.
            4. Check the **Save to** folder.
            5. Press **Download** (or the Enter key).

            The download appears in the list below with its progress bar. When it finishes, press **Open** to watch or listen to it.

            > If you don't want to bother with settings, leave everything as it is: "Best available", MP4 and Automatic codec give a good result in most cases.

            ## The first launch
            The first time you open the app, three tools are downloaded (about 260 MB in total): **yt-dlp**, **ffmpeg** and **deno**. You'll see the progress in the status bar at the bottom. Meanwhile, the **Download** and **Analyze** buttons are disabled. Later launches are instant.

            ## Handy shortcuts
            - **Enter**: download the pasted link.
            - **F1**: open this help.
            - **Right-click** on a download: all of its actions.
            """),

        new("link",
            "Pegar el enlace y Analizar", "Pasting the link and Analyze",
            """
            ## ¿Qué enlaces valen?
            Cualquier enlace que empiece por `http://` o `https://` de un sitio compatible (mira «Sitios compatibles»). El botón **Descargar** se activa en cuanto el texto es un enlace válido.

            ## El botón Analizar
            **Analizar** consulta el vídeo **sin descargarlo** y te muestra, debajo del campo, su título, el canal o autor y la duración. Además:
            - Sustituye la lista de **Resolución** por las que existen de verdad para ese vídeo.
            - Te sirve para comprobar que el enlace es correcto antes de empezar.

            Es **opcional**: puedes pulsar Descargar directamente. Si cambias el enlace después de analizar, la lista de resoluciones vuelve a la estándar.

            > Si Analizar da error, el mensaje te explica la causa (por ejemplo, que el vídeo exige iniciar sesión) y cómo solucionarlo.

            ## Enlaces de listas y canales
            Por defecto, si el enlace es de un vídeo dentro de una lista, se descarga **solo ese vídeo**. Para bajar la lista entera, marca **Descargar lista / álbum completo** (mira «Opciones extra»).
            """,
            """
            ## Which links work?
            Any link starting with `http://` or `https://` from a supported site (see "Supported sites"). The **Download** button turns on as soon as the text is a valid link.

            ## The Analyze button
            **Analyze** queries the video **without downloading it** and shows, under the field, its title, the channel or author and the duration. It also:
            - Replaces the **Resolution** list with the ones that really exist for that video.
            - Lets you check that the link is right before starting.

            It is **optional**: you can press Download directly. If you change the link after analyzing, the resolution list goes back to the standard one.

            > If Analyze fails, the message explains the cause (for example, that the video requires login) and how to fix it.

            ## Playlist and channel links
            By default, if the link points to a video inside a playlist, **only that video** is downloaded. To get the whole playlist, tick **Download whole playlist / album** (see "Extra options").
            """),

        new("video",
            "Opciones de vídeo", "Video options",
            """
            Se ven cuando está seleccionado **Vídeo**.

            ## Resolución
            Desde «Mejor disponible» hasta 8K (4320p), 4K (2160p), 2K (1440p), 1080p, 720p, 480p, 360p, 240p y 144p.
            - Es un **tope**, no una exigencia: si eliges 1080p y el vídeo solo llega a 720p, se descarga a 720p.
            - A más resolución, **más tamaño** y más tiempo de descarga. Para ver en el móvil, 720p suele sobrar.
            - Pulsa **Analizar** para ver qué resoluciones tiene realmente ese vídeo.

            ## Contenedor (el tipo de archivo)
            - **MP4 (más compatible)**: se abre en casi todo (Windows, móvil, televisores). Es la opción recomendada.
            - **MKV (admite todo)**: acepta cualquier combinación de imagen y sonido sin problemas. Algunos reproductores antiguos no lo abren.
            - **WEBM (VP9/AV1 + Opus)**: formato abierto de la web. Es **estricto**: si el vídeo no ofrece esas combinaciones, la descarga puede fallar; en ese caso usa MKV.

            ## Códec de vídeo
            - **Automático (mejor calidad)**: elige lo mejor que haya. Con MP4 puede salir VP9 o AV1, que algunos reproductores de Windows no leen bien.
            - **H.264 (máx. compatibilidad)**: se reproduce en prácticamente cualquier aparato. Elígelo si tu vídeo «no se ve» o va a trompicones.
            - **VP9**: buena calidad con menos tamaño que H.264.
            - **AV1 (menor tamaño)**: el más eficiente, pero exige reproductores y equipos modernos.

            > El códec es una **preferencia**: si el vídeo no tiene el que pides, se usa el siguiente mejor.

            ## ¿Qué elijo?
            - Para ver en cualquier sitio: **MP4 + H.264**.
            - Para guardar la máxima calidad sin pensar: **MKV + Automático**.
            - Para ahorrar espacio: **MP4 o MKV + AV1** (si tu reproductor lo admite).
            """,
            """
            Shown when **Video** is selected.

            ## Resolution
            From "Best available" down to 8K (4320p), 4K (2160p), 2K (1440p), 1080p, 720p, 480p, 360p, 240p and 144p.
            - It is a **ceiling**, not a demand: if you pick 1080p and the video only goes up to 720p, it downloads at 720p.
            - Higher resolution means **bigger files** and longer downloads. For watching on a phone, 720p is usually plenty.
            - Press **Analyze** to see which resolutions that video really has.

            ## Container (the file type)
            - **MP4 (most compatible)**: opens on almost anything (Windows, phones, TVs). Recommended.
            - **MKV (supports everything)**: accepts any combination of picture and sound without problems. Some older players can't open it.
            - **WEBM (VP9/AV1 + Opus)**: the web's open format. It is **strict**: if the video doesn't offer those combinations, the download may fail; in that case use MKV.

            ## Video codec
            - **Automatic (best quality)**: picks the best available. With MP4 you may get VP9 or AV1, which some Windows players don't handle well.
            - **H.264 (max compatibility)**: plays on practically any device. Choose it if your video "won't show" or stutters.
            - **VP9**: good quality at a smaller size than H.264.
            - **AV1 (smaller size)**: the most efficient, but needs modern players and hardware.

            > The codec is a **preference**: if the video doesn't have the one you ask for, the next best is used.

            ## What should I pick?
            - To watch anywhere: **MP4 + H.264**.
            - To keep maximum quality without thinking: **MKV + Automatic**.
            - To save space: **MP4 or MKV + AV1** (if your player supports it).
            """),

        new("audio",
            "Opciones de audio", "Audio options",
            """
            Selecciona **Solo audio** para guardar únicamente el sonido (música, podcasts, charlas).

            ## Formato
            - **MP3**: el más universal. Lo lee cualquier reproductor, coche o móvil.
            - **OGG (Vorbis)**: libre y eficiente; menos compatible que MP3.
            - **WAV (sin compresión)**: calidad original, pero archivos **muy grandes**. Útil para editar audio.
            - **FLAC (sin pérdida)**: calidad original comprimida, la mitad de grande que WAV aproximadamente.
            - **M4A (AAC)**: buena calidad y tamaño pequeño; el estándar de Apple.
            - **Opus**: el más eficiente a bajas tasas; ideal para voz.

            ## Calidad (bitrate)
            «Mejor calidad», 320, 256, 192, 128 o 96 kbps. Más kbps = mejor sonido y más tamaño. **192 kbps** es un buen equilibrio; **320 kbps** es casi imposible de distinguir del original.

            > **WAV y FLAC** conservan la calidad original, así que el selector de calidad se desactiva. Con MP3, «Mejor calidad» usa el máximo de VBR (V0).

            ## Un detalle importante
            El audio de internet ya viene comprimido. Convertir a WAV o FLAC **no mejora** el sonido; solo evita perder más calidad en la conversión.

            ## Sitios de música
            Si pegas un enlace de **SoundCloud, Bandcamp o Mixcloud**, la app cambia sola a «Solo audio» (puedes volver a Vídeo cuando quieras).
            """,
            """
            Select **Audio only** to save just the sound (music, podcasts, talks).

            ## Format
            - **MP3**: the most universal. Any player, car or phone reads it.
            - **OGG (Vorbis)**: free and efficient; less compatible than MP3.
            - **WAV (uncompressed)**: original quality, but **very large** files. Useful for audio editing.
            - **FLAC (lossless)**: original quality, compressed to roughly half the size of WAV.
            - **M4A (AAC)**: good quality and small size; Apple's standard.
            - **Opus**: the most efficient at low rates; ideal for voice.

            ## Quality (bitrate)
            "Best quality", 320, 256, 192, 128 or 96 kbps. More kbps = better sound and bigger size. **192 kbps** is a good balance; **320 kbps** is nearly impossible to tell apart from the original.

            > **WAV and FLAC** keep the original quality, so the quality selector is disabled. With MP3, "Best quality" uses maximum VBR (V0).

            ## An important detail
            Audio from the internet is already compressed. Converting to WAV or FLAC does **not improve** the sound; it only avoids losing more quality in the conversion.

            ## Music sites
            If you paste a link from **SoundCloud, Bandcamp or Mixcloud**, the app switches to "Audio only" by itself (you can go back to Video whenever you like).
            """),

        new("extras",
            "Opciones extra: metadatos, miniatura, listas y recorte", "Extra options: metadata, thumbnail, playlists and trimming",
            """
            ## Incrustar metadatos
            Guarda dentro del archivo el título, el artista o canal, la fecha y la descripción. Así tu reproductor muestra bien los datos. Viene **activado** por defecto.

            ## Incrustar miniatura / carátula
            Guarda la imagen del vídeo como portada del archivo (se ve en el Explorador y en los reproductores de música). Se **desactiva** automáticamente cuando no es posible: con el contenedor WEBM y con el formato WAV.

            ## Descargar lista / álbum completo
            Marca esta casilla si el enlace es de una **lista de reproducción, álbum o set** y quieres todos sus elementos.
            - Se crea **una sola fila** en la lista de descargas; el título y el porcentaje que ves son los del elemento que se está bajando en ese momento.
            - Todos los archivos se guardan en tu carpeta de descargas, cada uno con su título.
            - Para listas largas, ten paciencia: puede tardar bastante.

            > Con la casilla **sin marcar**, solo se descarga el vídeo del enlace, aunque pertenezca a una lista.

            ## Recortar (opcional)
            Descarga **solo un trozo** del vídeo o audio, escribiendo desde dónde y hasta dónde.
            - Formatos válidos: segundos (`90`), minutos y segundos (`1:30`) u horas (`1:02:03`).
            - Solo **desde**: de ahí hasta el final. Solo **hasta**: desde el principio hasta ahí.
            - Déjalo **vacío** para descargar todo.
            - Si el formato no es válido, el botón Descargar se desactiva hasta que lo corrijas.

            > Es muy útil para quedarte con una canción de un directo, un fragmento de una charla o un momento de una retransmisión de Twitch.
            """,
            """
            ## Embed metadata
            Saves the title, artist or channel, date and description inside the file, so your player shows the details properly. It is **on** by default.

            ## Embed thumbnail / cover art
            Saves the video's image as the file's cover (shown in Explorer and music players). It is automatically **disabled** when it isn't possible: with the WEBM container and the WAV format.

            ## Download whole playlist / album
            Tick this box if the link is a **playlist, album or set** and you want all of its items.
            - **One single row** is created in the download list; the title and percentage you see are those of the item being downloaded at that moment.
            - All files are saved in your download folder, each with its own title.
            - For long playlists, be patient: it can take a while.

            > With the box **unticked**, only the video in the link is downloaded, even if it belongs to a playlist.

            ## Trim (optional)
            Downloads **only a part** of the video or audio, by typing where it starts and ends.
            - Valid formats: seconds (`90`), minutes and seconds (`1:30`) or hours (`1:02:03`).
            - **from** only: from there to the end. **to** only: from the start up to there.
            - Leave it **empty** to download everything.
            - If the format is invalid, the Download button is disabled until you fix it.

            > Very handy to keep one song from a live show, a piece of a talk or a moment from a Twitch broadcast.
            """),

        new("cookies",
            "Cookies: sitios que piden iniciar sesión", "Cookies: sites that require login",
            """
            ## El problema
            Algunas webs (**Instagram, Vimeo, Reddit, X, Facebook**…) solo dejan bajar ciertos vídeos si eres un usuario con sesión iniciada. También ocurre con vídeos **privados**, de **pago a tu cuenta** o con **restricción de edad**. La aplicación, por sí sola, no tiene ninguna sesión y esos sitios la rechazan.

            ## La solución: el selector «Cookies»
            Las cookies son los datos que tu navegador guarda para recordar que ya iniciaste sesión. El selector **Cookies** (en la tarjeta de opciones) hace que la app use las de tu navegador.

            1. **Inicia sesión** en la web en tu navegador normal.
            2. En YTFE, abre **Cookies** y elige ese navegador: Chrome, Edge, Firefox, Brave, Opera o Vivaldi.
            3. Pega el enlace y descarga como siempre.

            ## Cuándo NO hace falta
            Para vídeos públicos de YouTube, TikTok, SoundCloud, TED, Streamable, Archive.org y similares, déjalo en **Ninguno**.

            ## Consejos
            - **Firefox es el más fiable.** Chrome, Edge y Brave pueden fallar si el navegador está abierto, porque protegen sus cookies.
            - Si las cookies **no se pueden leer** (navegador no instalado, bloqueado…), la app reintenta sola **sin cookies** y te avisa. Los vídeos públicos seguirán funcionando.
            - El ajuste se **recuerda** al cerrar la aplicación.
            - Afecta a Analizar y a las descargas **nuevas**; no a las que ya estén en marcha.

            ## Privacidad
            La aplicación usa tus cookies solo para pedir el vídeo a la web. **No las guarda ni las envía a ningún otro sitio**, y nunca ve tu contraseña.
            """,
            """
            ## The problem
            Some websites (**Instagram, Vimeo, Reddit, X, Facebook**…) only let you download certain videos if you are a logged-in user. The same happens with **private** videos, videos available only to **your account**, or **age-restricted** ones. The app alone has no session, so those sites turn it away.

            ## The fix: the "Cookies" selector
            Cookies are the data your browser keeps to remember that you already logged in. The **Cookies** selector (in the options card) makes the app use your browser's.

            1. **Log in** to the website in your normal browser.
            2. In YTFE, open **Cookies** and pick that browser: Chrome, Edge, Firefox, Brave, Opera or Vivaldi.
            3. Paste the link and download as usual.

            ## When you DON'T need it
            For public videos from YouTube, TikTok, SoundCloud, TED, Streamable, Archive.org and similar, leave it on **None**.

            ## Tips
            - **Firefox is the most reliable.** Chrome, Edge and Brave may fail while the browser is open, because they protect their cookies.
            - If the cookies **can't be read** (browser not installed, locked…), the app retries by itself **without cookies** and tells you. Public videos will keep working.
            - The setting is **remembered** when you close the app.
            - It affects Analyze and **new** downloads; not those already running.

            ## Privacy
            The app uses your cookies only to request the video from the website. It **doesn't store them or send them anywhere else**, and it never sees your password.
            """),

        new("sites",
            "Sitios compatibles", "Supported sites",
            """
            YTFE usa **yt-dlp**, que admite más de **1.000 sitios**. Algunos de los más conocidos:

            ## Vídeo y redes sociales
            YouTube, Vimeo, Dailymotion, Twitch (grabaciones y clips), TikTok, Instagram, Facebook, X (Twitter), Reddit, LinkedIn, Pinterest, Tumblr, VK, Bilibili.

            ## Audio y música
            SoundCloud, Bandcamp, Mixcloud.

            ## Otros
            Archive.org, Streamable, Imgur, TED, RTVE, BBC, y muchas cadenas de televisión y medios de noticias.

            ## Ver la lista
            El botón **Sitios compatibles** (arriba a la derecha) muestra estos sitios y enlaza con la **lista completa y actualizada** de yt-dlp.

            ## Lo que NO funciona
            - Los servicios con **DRM** (protección anticopia): **Netflix, Disney+, HBO Max, Prime Video, Spotify**… No se pueden descargar.
            - Algunos vídeos **no están disponibles en tu país** (restricción geográfica).
            - **Sin sesión**, Instagram, Vimeo, Reddit o Facebook pueden negarse: usa el selector **Cookies**.

            > Cada fila de descarga muestra una **etiqueta con el nombre del sitio** (por ejemplo, «YouTube»), para que sepas de dónde viene.

            ## ¿Mi sitio no aparece en la lista?
            La lista de arriba es solo una muestra. Si el sitio está en la lista completa de yt-dlp, funcionará aunque aquí no aparezca; en la fila verás el nombre de su dominio.
            """,
            """
            YTFE uses **yt-dlp**, which supports more than **1,000 sites**. Some of the best known:

            ## Video and social networks
            YouTube, Vimeo, Dailymotion, Twitch (recordings and clips), TikTok, Instagram, Facebook, X (Twitter), Reddit, LinkedIn, Pinterest, Tumblr, VK, Bilibili.

            ## Audio and music
            SoundCloud, Bandcamp, Mixcloud.

            ## Others
            Archive.org, Streamable, Imgur, TED, RTVE, BBC, and many TV channels and news outlets.

            ## See the list
            The **Supported sites** button (top right) shows these sites and links to yt-dlp's **full, up-to-date list**.

            ## What does NOT work
            - Services with **DRM** (copy protection): **Netflix, Disney+, HBO Max, Prime Video, Spotify**… They can't be downloaded.
            - Some videos are **not available in your country** (geo-restriction).
            - **Without a session**, Instagram, Vimeo, Reddit or Facebook may refuse: use the **Cookies** selector.

            > Each download row shows a **tag with the site's name** (for example, "YouTube"), so you know where it came from.

            ## My site isn't in the list?
            The list above is only a sample. If the site is in yt-dlp's full list, it will work even if it isn't shown here; in the row you'll see its domain name.
            """),

        new("manage",
            "Gestionar las descargas", "Managing downloads",
            """
            Cada descarga es una **tarjeta** con: la etiqueta del sitio, el título, un resumen de lo elegido (formato, calidad…), la barra de progreso y el estado con velocidad y tiempo restante.

            ## Estados
            - **En cola**: espera su turno. Se descargan **2 a la vez**.
            - **Descargando**: en marcha.
            - **Procesando**: se están uniendo vídeo y audio o convirtiendo el archivo. Es normal que el progreso no se mueva.
            - **En pausa**: detenida, conservando lo descargado.
            - **Completado** (verde), **Error** (rojo) y **Cancelado**.

            ## Botones de cada descarga
            Solo se ven los que tienen sentido en cada estado:
            - **Pausar**: detiene la descarga y **conserva** lo ya bajado.
            - **Reanudar**: continúa justo donde se quedó.
            - **Cancelar**: la abandona y **descarta** lo descargado.
            - **Reintentar**: vuelve a empezar (si hubo error o la cancelaste).
            - **Abrir**: abre el archivo con tu programa predeterminado (cuando está completada).

            ## Menú ⋯ y clic derecho
            - **Mostrar en carpeta**: abre el Explorador con el archivo seleccionado.
            - **Copiar enlace** y **Abrir en el navegador**.
            - **Quitar de la lista**: quita la tarjeta **sin borrar ningún archivo**.
            - **Eliminar archivos…**: si está completada, manda el archivo a la **Papelera** (se puede recuperar). Si no, borra solo los archivos parciales de esa descarga.

            ## Pausar o Cancelar: ¿cuál uso?
            - **Pausar** = «sigo luego». Se reanuda con un clic. No se puede pausar durante «Procesando».
            - **Cancelar** = «lo dejo». Si llevaba más del 5 %, pide confirmación. Si luego la reintentas, empieza de cero.

            ## Acciones sobre toda la lista
            **Pausar todo**, **Reanudar todo** y **Limpiar finalizadas** (quita de la lista las completadas, con error y canceladas; no borra archivos).

            ## Al cerrar la aplicación
            - Si hay descargas en marcha, **te pregunta** antes de salir. Al salir quedan **en pausa**.
            - Al volver a abrirla, la lista se **restaura**. Las descargas a medias aparecen en pausa y **nunca arrancan solas**: pulsa **Reanudar** cuando quieras.
            """,
            """
            Each download is a **card** with: the site tag, the title, a summary of what you chose (format, quality…), the progress bar and the status with speed and time left.

            ## States
            - **Queued**: waiting for its turn. **2 at a time** are downloaded.
            - **Downloading**: running.
            - **Processing**: video and audio are being merged or the file is being converted. It's normal for progress not to move.
            - **Paused**: stopped, keeping what was downloaded.
            - **Completed** (green), **Error** (red) and **Cancelled**.

            ## Buttons on each download
            Only those that make sense in each state are shown:
            - **Pause**: stops the download and **keeps** what was already downloaded.
            - **Resume**: continues right where it stopped.
            - **Cancel**: abandons it and **discards** what was downloaded.
            - **Retry**: starts again (after an error or if you cancelled it).
            - **Open**: opens the file with your default program (when completed).

            ## The ⋯ menu and right-click
            - **Show in folder**: opens Explorer with the file selected.
            - **Copy link** and **Open in browser**.
            - **Remove from list**: removes the card **without deleting any file**.
            - **Delete files…**: if completed, sends the file to the **Recycle Bin** (recoverable). Otherwise, deletes only that download's partial files.

            ## Pause or Cancel: which one?
            - **Pause** = "I'll continue later". Resumes with one click. You can't pause during "Processing".
            - **Cancel** = "I give up". If it was past 5%, it asks for confirmation. If you retry later, it starts from scratch.

            ## Actions on the whole list
            **Pause all**, **Resume all** and **Clear finished** (removes completed, failed and cancelled ones from the list; doesn't delete files).

            ## When closing the app
            - If downloads are running, it **asks** before exiting. On exit they are left **paused**.
            - When you reopen it, the list is **restored**. Half-done downloads appear paused and **never start on their own**: press **Resume** whenever you like.
            """),

        new("folder",
            "Dónde se guardan los archivos", "Where files are saved",
            """
            ## Carpeta de descargas
            Se elige en la fila **Guardar en**. Por defecto es **Vídeos\YTFE**. Pulsa **Examinar…** para elegir otra, o escribe la ruta directamente.
            - Si la carpeta no existe, se crea sola.
            - El cambio vale para las descargas **nuevas**.

            ## Nombre de los archivos
            Se guardan como **Título [identificador].extensión**, por ejemplo `Mi vídeo [abc123].mp4`. El identificador evita que dos vídeos con el mismo título se pisen entre sí. Puedes renombrar los archivos después sin problema.

            ## Los datos de la aplicación
            La aplicación guarda sus propios datos en `%LOCALAPPDATA%\YTFE\`:
            - `tools\`: las herramientas descargadas (yt-dlp, ffmpeg, deno).
            - `settings.json`: tus preferencias (idioma, tema, cookies).
            - `downloads.json`: la lista de descargas.

            > Para **empezar de cero** (como recién instalada) cierra la app y borra esa carpeta. Volverá a descargar las herramientas.

            ## Espacio en disco
            Los vídeos en alta resolución pesan mucho (un vídeo de una hora en 4K puede superar varios GB). Si te quedas sin espacio, la descarga falla: libera espacio y pulsa **Reintentar**.
            """,
            """
            ## Download folder
            Chosen in the **Save to** row. By default it is **Videos\YTFE**. Press **Browse…** to pick another one, or type the path directly.
            - If the folder doesn't exist, it is created automatically.
            - The change applies to **new** downloads.

            ## File names
            Files are saved as **Title [identifier].extension**, for example `My video [abc123].mp4`. The identifier prevents two videos with the same title from overwriting each other. You can rename files afterwards without any problem.

            ## The app's data
            The app keeps its own data in `%LOCALAPPDATA%\YTFE\`:
            - `tools\`: the downloaded tools (yt-dlp, ffmpeg, deno).
            - `settings.json`: your preferences (language, theme, cookies).
            - `downloads.json`: the download list.

            > To **start from scratch** (like a fresh install), close the app and delete that folder. It will download the tools again.

            ## Disk space
            High-resolution videos are heavy (a one-hour 4K video can exceed several GB). If you run out of space, the download fails: free up space and press **Retry**.
            """),

        new("tools",
            "Herramientas y actualizaciones", "Tools and updates",
            """
            YTFE se apoya en tres programas que descarga y gestiona por ti:
            - **yt-dlp**: encuentra y descarga el vídeo.
            - **ffmpeg**: une imagen y sonido y convierte formatos.
            - **deno**: necesario para YouTube.

            Las versiones instaladas aparecen en la barra inferior.

            ## Actualizaciones automáticas
            **yt-dlp** se comprueba solo al abrir la aplicación, como máximo **una vez cada 24 horas**. Es importante: YouTube y otros sitios cambian a menudo y rompen las descargas; actualizar suele arreglarlo.

            ## El botón «Actualizar herramientas»
            Actualiza yt-dlp, deno y ffmpeg, cada uno solo si hay una versión más nueva.
            - ffmpeg (~200 MB) y deno (~40 MB) **no** se actualizan solos.
            - **No se actualiza** si hay descargas en curso: espera a que terminen.
            - Mientras actualiza, los botones Descargar y Analizar se bloquean unos instantes.

            > **Regla de oro:** si de pronto todo empieza a fallar con un sitio que antes funcionaba, pulsa **Actualizar herramientas** y vuelve a probar.
            """,
            """
            YTFE relies on three programs that it downloads and manages for you:
            - **yt-dlp**: finds and downloads the video.
            - **ffmpeg**: merges picture and sound and converts formats.
            - **deno**: required for YouTube.

            The installed versions appear in the bottom bar.

            ## Automatic updates
            **yt-dlp** is checked on its own when the app opens, at most **once every 24 hours**. This matters: YouTube and other sites change often and break downloads; updating usually fixes it.

            ## The "Update tools" button
            Updates yt-dlp, deno and ffmpeg, each only if a newer version exists.
            - ffmpeg (~200 MB) and deno (~40 MB) are **not** updated automatically.
            - It **won't update** while downloads are running: wait for them to finish.
            - While updating, the Download and Analyze buttons are blocked for a few moments.

            > **Golden rule:** if everything suddenly starts failing on a site that used to work, press **Update tools** and try again.
            """),

        new("appearance",
            "Tema e idioma", "Theme and language",
            """
            Los dos selectores están arriba a la derecha.

            ## Tema
            - **Sistema**: sigue el tema de Windows y cambia con él (es lo predeterminado).
            - **Claro**: fondo claro.
            - **Oscuro**: fondo oscuro, más cómodo de noche o con poca luz.

            El cambio es **inmediato**, sin reiniciar, y se recuerda al volver a abrir la aplicación.

            ## Idioma
            **Español** o **English**. El cambio es instantáneo, incluida esta ayuda. La primera vez, la app usa el idioma de tu Windows (español si está en español, inglés en cualquier otro caso).
            """,
            """
            Both selectors are at the top right.

            ## Theme
            - **System**: follows the Windows theme and changes with it (the default).
            - **Light**: light background.
            - **Dark**: dark background, more comfortable at night or in low light.

            The change is **immediate**, with no restart, and is remembered next time.

            ## Language
            **Español** or **English**. The change is instant, including this help. The first time, the app uses your Windows language (Spanish if Windows is in Spanish, English otherwise).
            """),

        new("trouble",
            "Solución de problemas", "Troubleshooting",
            """
            Cuando una descarga falla, la fila se pone en **rojo** y explica la causa. Estas son las más habituales:

            ## «Este contenido exige iniciar sesión…»
            El vídeo es privado, tiene restricción de edad o el sitio pide cuenta. Inicia sesión en tu navegador y elige ese navegador en **Cookies** (mira el tema «Cookies»). Si ya lo hiciste, comprueba que la cuenta tiene acceso, o prueba con **Firefox**.

            ## «No se pudieron leer las cookies del navegador»
            Cierra el navegador y reintenta, o elige **Firefox**. Mientras tanto, la app intenta bajar el vídeo sin cookies.

            ## «Este contenido tiene protección DRM»
            No se puede descargar. Es el caso de Netflix, Disney+, Prime Video, Spotify…

            ## «No está disponible en tu país o región»
            El vídeo tiene restricción geográfica. No hay solución dentro de la aplicación.

            ## «Este enlace no es de un sitio compatible…»
            Revisa que has copiado la URL completa y que apunta a un **vídeo concreto**, no a la portada del sitio.

            ## «El vídeo no está disponible»
            Fue borrado, es privado o el enlace está mal. Ábrelo en el navegador («Abrir en el navegador» en el menú ⋯) para comprobarlo.

            ## «El sitio está limitando las peticiones»
            Has hecho muchas peticiones seguidas. Espera unos minutos y pulsa **Reintentar**.

            ## «No se pudo conectar con el sitio»
            Revisa tu conexión a internet y pulsa **Reintentar**.

            ## Otros problemas
            - **Descargar y Analizar están desactivados**: se están descargando o actualizando las herramientas. Espera a que termine (mira la barra de estado).
            - **Todo falla de repente con un sitio que iba bien**: pulsa **Actualizar herramientas**.
            - **El vídeo no se ve o va a trompicones**: vuelve a descargarlo con **MP4 + H.264**.
            - **Falla con WEBM**: usa **MKV** o **MP4**.
            - **El progreso se queda quieto en «Procesando»**: es normal en archivos grandes; ffmpeg está uniendo el vídeo y el audio.
            - **Una descarga se interrumpió**: pulsa **Reintentar**; continúa desde lo ya descargado siempre que sea posible.
            - **Windows bloquea la aplicación** («Una directiva de Control de aplicaciones bloqueó este archivo»): es una protección de Windows que bloquea programas sin firmar. Pide a tu administrador que la permita o ajusta esa política.
            """,
            """
            When a download fails, its row turns **red** and explains the cause. These are the most common:

            ## "This content requires login…"
            The video is private, age-restricted, or the site demands an account. Log in in your browser and pick that browser under **Cookies** (see the "Cookies" topic). If you already did, check that the account has access, or try **Firefox**.

            ## "Could not read the browser cookies"
            Close the browser and retry, or pick **Firefox**. Meanwhile, the app tries to download the video without cookies.

            ## "This content is DRM-protected"
            It can't be downloaded. That's the case for Netflix, Disney+, Prime Video, Spotify…

            ## "Not available in your country or region"
            The video is geo-restricted. There is no fix inside the app.

            ## "This link is not from a supported site…"
            Check that you copied the whole URL and that it points to a **specific video**, not the site's home page.

            ## "The video is unavailable"
            It was deleted, is private or the link is wrong. Open it in the browser ("Open in browser" in the ⋯ menu) to check.

            ## "The site is rate-limiting requests"
            You made many requests in a row. Wait a few minutes and press **Retry**.

            ## "Could not connect to the site"
            Check your internet connection and press **Retry**.

            ## Other problems
            - **Download and Analyze are disabled**: the tools are being downloaded or updated. Wait for it to finish (see the status bar).
            - **Everything suddenly fails on a site that used to work**: press **Update tools**.
            - **The video won't show or stutters**: download it again with **MP4 + H.264**.
            - **It fails with WEBM**: use **MKV** or **MP4**.
            - **Progress stays still on "Processing"**: normal for big files; ffmpeg is merging video and audio.
            - **A download was interrupted**: press **Retry**; it continues from what was already downloaded whenever possible.
            - **Windows blocks the app** ("An Application Control policy blocked this file"): it is a Windows protection that blocks unsigned programs. Ask your administrator to allow it or adjust that policy.
            """),

        new("faq",
            "Preguntas frecuentes", "FAQ",
            """
            ## ¿Es legal descargar vídeos?
            Depende del contenido y del país. Descarga solo lo que tengas **derecho a descargar**: tus propios vídeos, contenido con licencia que lo permita o de dominio público, y respeta las condiciones de uso de cada sitio. Tú eres el responsable del uso que hagas.

            ## ¿Cuántos vídeos puedo descargar a la vez?
            Se descargan **2 a la vez**; el resto espera en cola y empieza solo cuando hay hueco. Puedes añadir tantos como quieras.

            ## ¿Puedo descargar una lista de reproducción entera?
            Sí: pega el enlace de la lista y marca **Descargar lista / álbum completo**.

            ## ¿Puedo bajar solo una parte de un vídeo?
            Sí, con **Recortar**: escribe desde dónde y hasta dónde (por ejemplo `1:30` y `4:00`).

            ## ¿Puedo descargar solo la música?
            Sí: selecciona **Solo audio** y elige MP3 (o el formato que prefieras).

            ## ¿Guarda la app mi contraseña?
            No. Nunca la ve ni la pide. Con el selector Cookies solo se usa la sesión que ya tienes abierta en tu navegador.

            ## ¿Funciona sin internet?
            No. Necesita conexión para descargar las herramientas la primera vez y para bajar los vídeos.

            ## ¿Por qué ocupan tanto los vídeos?
            Por la resolución y la duración. Baja la **resolución** (720p o 480p) o elige el códec **AV1** para reducir el tamaño.

            ## ¿Puedo mover o renombrar un archivo descargado?
            Sí, pero la app ya no podrá abrirlo desde su fila (el botón **Abrir** deja de funcionar). El archivo en sí no se estropea.

            ## ¿Cómo desinstalo la aplicación?
            Borra la carpeta de la aplicación y, si quieres limpiar del todo, `%LOCALAPPDATA%\YTFE\`. Tus vídeos descargados no se tocan.
            """,
            """
            ## Is it legal to download videos?
            It depends on the content and the country. Download only what you have the **right to download**: your own videos, content with a licence that allows it or in the public domain, and respect each site's terms of use. You are responsible for how you use it.

            ## How many videos can I download at once?
            **2 at a time** are downloaded; the rest wait in the queue and start by themselves when there's room. You can add as many as you like.

            ## Can I download an entire playlist?
            Yes: paste the playlist link and tick **Download whole playlist / album**.

            ## Can I download just part of a video?
            Yes, with **Trim**: type where it starts and ends (for example `1:30` and `4:00`).

            ## Can I download just the music?
            Yes: select **Audio only** and choose MP3 (or the format you prefer).

            ## Does the app store my password?
            No. It never sees or asks for it. With the Cookies selector only the session you already have open in your browser is used.

            ## Does it work offline?
            No. It needs a connection to download the tools the first time and to fetch videos.

            ## Why are videos so big?
            Because of resolution and length. Lower the **resolution** (720p or 480p) or pick the **AV1** codec to reduce the size.

            ## Can I move or rename a downloaded file?
            Yes, but the app will no longer be able to open it from its row (the **Open** button stops working). The file itself is not harmed.

            ## How do I uninstall the app?
            Delete the app's folder and, to clean up completely, `%LOCALAPPDATA%\YTFE\`. Your downloaded videos are not touched.
            """),

        new("about",
            "Acerca de y créditos", "About and credits",
            """
            **YTFE** · versión 1.1.1
            Creado por **Damanium** · Copyright © 2026 · Licencia MIT.
            Código abierto: [github.com/Damanium/YTFE](https://github.com/Damanium/YTFE)
            ¿Te resulta útil? Puedes apoyar el proyecto, sin ninguna obligación: [paypal.me/damanium](https://paypal.me/damanium)

            YTFE es solo una interfaz gráfica. Todo el trabajo pesado lo hacen proyectos de terceros, creados por otras personas, que merecen todo el crédito:

            ## Herramientas que usa
            - [yt-dlp](https://github.com/yt-dlp/yt-dlp): el motor que encuentra y descarga los vídeos. Creado por [pukkandan](https://github.com/pukkandan) y mantenido por **los colaboradores de yt-dlp**, a partir de [youtube-dl](https://github.com/ytdl-org/youtube-dl) (de **Ricardo Garcia** y su comunidad). Licencia Unlicense.
            - [FFmpeg](https://github.com/FFmpeg/FFmpeg): une imagen y sonido y convierte formatos. Es obra de **los desarrolladores de FFmpeg** (proyecto fundado por **Fabrice Bellard**). Se descarga la compilación de [yt-dlp/FFmpeg-Builds](https://github.com/yt-dlp/FFmpeg-Builds). Licencia GPL.
            - [Deno](https://github.com/denoland/deno): entorno de ejecución que yt-dlp necesita para YouTube. Creado por [Ryan Dahl](https://github.com/ry) y **los colaboradores de Deno** (Deno Land). Licencia MIT.

            ## Tecnología y recursos
            - **.NET** ([dotnet/runtime](https://github.com/dotnet/runtime)) y **WPF** ([dotnet/wpf](https://github.com/dotnet/wpf)), de **Microsoft** y la comunidad de .NET. Licencia MIT.
            - [Simple Icons](https://github.com/simple-icons/simple-icons): los logos de los sitios, de **los colaboradores de Simple Icons**. Licencia CC0.

            > Los nombres y logos de YouTube, Vimeo, TikTok y demás sitios son marcas de sus respectivos propietarios y se usan solo para identificar el sitio. YTFE no está afiliado ni respaldado por ellos, ni por los autores de las herramientas anteriores.

            ## Gracias
            A todas las personas que desarrollan y mantienen estos proyectos libres, sin los cuales esta aplicación no existiría.
            """,
            """
            **YTFE** · version 1.1.1
            Created by **Damanium** · Copyright © 2026 · MIT licence.
            Open source: [github.com/Damanium/YTFE](https://github.com/Damanium/YTFE)
            Find it useful? You can support the project, no strings attached: [paypal.me/damanium](https://paypal.me/damanium)

            YTFE is just a graphical interface. All the heavy lifting is done by third-party projects, made by other people, who deserve full credit:

            ## Tools it uses
            - [yt-dlp](https://github.com/yt-dlp/yt-dlp): the engine that finds and downloads the videos. Created by [pukkandan](https://github.com/pukkandan) and maintained by **the yt-dlp contributors**, based on [youtube-dl](https://github.com/ytdl-org/youtube-dl) (by **Ricardo Garcia** and its community). Unlicense.
            - [FFmpeg](https://github.com/FFmpeg/FFmpeg): merges picture and sound and converts formats. The work of **the FFmpeg developers** (project founded by **Fabrice Bellard**). The build from [yt-dlp/FFmpeg-Builds](https://github.com/yt-dlp/FFmpeg-Builds) is downloaded. GPL licence.
            - [Deno](https://github.com/denoland/deno): the runtime yt-dlp needs for YouTube. Created by [Ryan Dahl](https://github.com/ry) and **the Deno contributors** (Deno Land). MIT licence.

            ## Technology and resources
            - **.NET** ([dotnet/runtime](https://github.com/dotnet/runtime)) and **WPF** ([dotnet/wpf](https://github.com/dotnet/wpf)), by **Microsoft** and the .NET community. MIT licence.
            - [Simple Icons](https://github.com/simple-icons/simple-icons): the site logos, by **the Simple Icons contributors**. CC0 licence.

            > The names and logos of YouTube, Vimeo, TikTok and other sites are trademarks of their respective owners and are used only to identify the site. YTFE is not affiliated with or endorsed by them, nor by the authors of the tools above.

            ## Thanks
            To everyone who develops and maintains these free projects, without which this app would not exist.
            """),
    ];
}
