# YTFE

Aplicación de escritorio para **Windows 10/11** que descarga vídeo y audio de internet con una interfaz gráfica.
Por debajo usa [yt-dlp](https://github.com/yt-dlp/yt-dlp) (que admite YouTube y cientos de sitios más) y
[ffmpeg](https://ffmpeg.org/) para convertir y unir los archivos.

- **Interfaz:** WPF sobre .NET 10 (C#), patrón MVVM.
- **Motor:** `yt-dlp.exe` se ejecuta como proceso hijo. No hay Python embebido.
- **Idiomas:** español e inglés, con cambio al instante.
- **Tema:** claro, oscuro o el del sistema (tema Fluent de WPF).
- **Logo e icono:** `Assets\app.ico` y `logo.png` (generados por código; el logo de cada sitio es un trazo vectorial en `SiteLogos.cs`).
- **Ayuda integrada:** botón **Ayuda** o tecla **F1**, con buscador y 13 temas en español e inglés (`HelpContent.cs`).

---

## Para qué sirve

- Descargar **vídeo** eligiendo resolución (hasta 8K), contenedor (MP4/MKV/WEBM) y códec (H.264/VP9/AV1).
- Descargar solo el **audio** en MP3, OGG, WAV, FLAC, M4A u Opus, eligiendo el bitrate.
- Descargar listas/álbumes completos, recortar un fragmento y usar las **cookies del navegador** para sitios que exigen sesión.
- Gestionar varias descargas a la vez: **pausar, reanudar, cancelar, reintentar y eliminar**.
- Conservar la lista de descargas entre sesiones.
- Mantener actualizadas las herramientas externas sin recompilar la aplicación.

> **Uso responsable.** Descarga solo contenido que tengas derecho a descargar (tuyo, con licencia que lo permita,
> o dominio público) y respeta las condiciones de uso de cada sitio.

---

## Requisitos

| Para… | Necesitas |
|---|---|
| **Ejecutar** | Windows 10 (1607 o posterior, requisito de .NET 10) o Windows 11, conexión a internet la primera vez |
| **Compilar** | [SDK de .NET 10](https://dotnet.microsoft.com/download) |

No hace falta instalar Python, yt-dlp, ffmpeg ni deno: la aplicación los descarga sola la primera vez.

### Si Windows bloquea la aplicación
Una política de **Control de aplicaciones** (Smart App Control o WDAC) puede bloquear binarios sin firmar, y
una compilación local lo es. Verás el error `Una directiva de Control de aplicaciones bloqueó este archivo`.
Las salidas son: ajustar esa política (Smart App Control, al desactivarlo, **no se puede volver a activar sin
reinstalar Windows**), pedir a TI que permita la carpeta, o **firmar los binarios** con un certificado de firma de código.

---

## Puesta en marcha

```powershell
git clone https://github.com/Damanium/YTFE.git
cd YTFE
dotnet build
dotnet run --project src\YTFE
```

El ejecutable queda en `src\YTFE\bin\Debug\net10.0-windows\YTFE.exe`.

**Primer arranque:** descarga `yt-dlp.exe` (~22 MB), `deno.exe` (~40 MB) y `ffmpeg` (~200 MB).
Verás el progreso en la barra de estado; hasta que termine, los botones "Descargar" y "Analizar" están desactivados.
Los arranques siguientes son inmediatos.

---

## Uso

### Descargar un vídeo o un audio
1. Pega la **URL** en el campo superior (solo se aceptan `http://` y `https://`).
2. *(Opcional)* Pulsa **Analizar**: muestra título, canal y duración, y sustituye la lista de resoluciones por las que
   existen realmente para ese vídeo.
3. Elige **Vídeo** o **Solo audio** y ajusta las opciones.
4. Comprueba la carpeta de **Guardar en** (por defecto `Vídeos\YTFE`; **Examinar…** para cambiarla).
5. Pulsa **Descargar** (o Enter).

### Opciones de vídeo
| Opción | Valores |
|---|---|
| **Resolución** | Mejor disponible, 8K (4320p), 4K (2160p), 2K (1440p), 1080p, 720p, 480p, 360p, 240p, 144p |
| **Contenedor** | **MP4** (más compatible), **MKV** (admite todo), **WEBM** (solo VP9/AV1 + Opus) |
| **Códec** | Automático, **H.264** (máxima compatibilidad), **VP9**, **AV1** (menor tamaño) |

- La resolución es un **tope**: si eliges 1080p y el vídeo solo llega a 720p, descarga 720p.
- El códec es una **preferencia**, no un filtro: si no existe el elegido, usa el siguiente mejor.
- Con **MP4 + Automático** puede venir VP9/AV1, que algunos reproductores de Windows leen mal. Para máxima
  compatibilidad elige **H.264**.
- **WEBM** es estricto: si el vídeo no ofrece VP9/AV1 con Opus, la descarga puede fallar. En ese caso usa MKV.

### Opciones de audio
| Opción | Valores |
|---|---|
| **Formato** | MP3, OGG (Vorbis), WAV, FLAC, M4A (AAC), Opus |
| **Calidad (bitrate)** | Mejor calidad, 320, 256, 192, 128 o 96 kbps |

- **WAV** y **FLAC** conservan la calidad original: el selector de bitrate se desactiva.
- "Mejor calidad" en MP3 usa VBR máximo (V0); en los demás formatos elige yt-dlp.

### Opciones comunes
- **Incrustar metadatos** (título, artista…).
- **Incrustar miniatura / carátula**: se desactiva donde no es posible (WAV y WEBM).

### Gestionar las descargas
Cada descarga muestra título, resumen de lo elegido, barra de progreso, estado, velocidad y tiempo restante.
Solo se ven los botones que tienen sentido en su estado actual; el resto de acciones están en el menú **⋯**
o con **clic derecho**.

| Acción | Qué hace | Disponible en |
|---|---|---|
| **Pausar** | Detiene el proceso y **conserva** lo descargado | Descargando |
| **Reanudar** | Continúa desde donde quedó | En pausa |
| **Cancelar** | Detiene el proceso y **descarta** lo descargado | En cola, descargando, procesando, en pausa |
| **Reintentar** | Vuelve a empezar | Error, cancelada |
| **Abrir** | Abre el archivo con el programa predeterminado | Completada |
| **Mostrar en carpeta** | Abre el Explorador con el archivo seleccionado | Siempre |
| **Copiar enlace** / **Abrir en el navegador** | | Siempre |
| **Quitar de la lista** | Quita la tarjeta, **sin borrar archivos** | Completada, error, cancelada |
| **Eliminar archivos…** | Completada: manda el archivo a la **Papelera**. Sin completar: borra solo los parciales de ese vídeo | Completada, en pausa, error, cancelada |

**Pausar frente a Cancelar**
- *Pausar* = "sigo luego": mantiene los archivos `.part` y se reanuda con un clic. No se puede pausar mientras se
  unen vídeo y audio ("Procesando"), porque interrumpir ffmpeg ahí corrompería el archivo.
- *Cancelar* = "lo dejo": borra los `.part` y temporales **de esa descarga** (nunca otros archivos). Pide confirmación
  solo si ya llevaba más del 5 %. Si después pulsas Reintentar, empieza desde cero.

**Acciones globales** (sobre la lista): **Pausar todo**, **Reanudar todo**, **Limpiar finalizadas**.

**Concurrencia:** se descargan 2 a la vez; el resto espera en cola. Una descarga en pausa no ocupa turno.

### Cerrar la aplicación
- Si hay descargas **en marcha**, pregunta antes de salir. Al salir se detienen y quedan **en pausa**.
- Al volver a abrirla, la lista se restaura. Las descargas a medias aparecen **en pausa** y **nunca arrancan solas**:
  pulsa **Reanudar** cuando quieras.

### Idioma
Desplegable de la barra inferior (Español / English). El cambio es inmediato y se recuerda. La primera vez usa el
idioma de tu Windows (español si está en español, inglés en cualquier otro caso).

### Herramientas y actualizaciones
La barra inferior muestra las versiones instaladas (`yt-dlp · ffmpeg · deno`).

- **yt-dlp** se comprueba solo **al arrancar, como máximo una vez cada 24 h**. YouTube cambia a menudo y
  rompe los extractores; si las descargas empiezan a fallar, actualizar suele ser la solución.
- El botón **Actualizar herramientas** actualiza yt-dlp, deno y ffmpeg, cada uno solo si hay versión más nueva.
  ffmpeg y deno **no** se actualizan solos (pesan ~200 MB y ~40 MB). No actualiza si hay descargas en curso.
- Mientras se actualiza, se bloquean "Descargar" y "Analizar".

---

## Dónde guarda las cosas

Todo en `%LOCALAPPDATA%\YTFE\`:

| Ruta | Contenido |
|---|---|
| `tools\` | `yt-dlp.exe`, `ffmpeg.exe`, `ffprobe.exe`, `deno.exe` |
| `settings.json` | Idioma, tema y navegador de cookies |
| `state.json` | Fecha de la última comprobación automática y referencia de versión de ffmpeg |
| `downloads.json` | Lista de descargas (se guarda sola; se restaura al abrir) |

Las descargas se guardan en la carpeta que elijas (por defecto `Vídeos\YTFE`) con el nombre `Título [id].ext`.
Para "empezar de cero" basta con borrar `%LOCALAPPDATA%\YTFE\`.

---

## Arquitectura

```
src/YTFE/
├── App.xaml(.cs)                     Arranque: fija el idioma antes de crear la ventana
├── MainWindow.xaml(.cs)              Interfaz (solo enlaces; sin lógica de negocio)
├── Models/
│   ├── DownloadOptions.cs            Opciones elegidas → argumentos de yt-dlp; Choice<T>, VideoInfo, etc.
│   └── DownloadState.cs              Estados de una descarga
├── ViewModels/
│   ├── MainViewModel.cs              Pantalla principal: opciones, lista, acciones globales, persistencia
│   └── DownloadItemViewModel.cs      Una descarga: ciclo de vida, pausa/cancelar, borrado
├── Services/
│   ├── YtDlpService.cs               Lanza yt-dlp, parsea progreso/título/id/ruta; análisis de URL
│   ├── ToolsManager.cs               Descarga, versiona y actualiza yt-dlp / ffmpeg / deno
│   ├── DownloadStore.cs              downloads.json (escritura atómica, validación al cargar)
│   └── AppSettings.cs                settings.json
├── Localization/
│   ├── Loc.cs, TExtension.cs         Servicio de idioma y extensión XAML {loc:T Clave}
│   └── Strings.cs                    Todos los textos, en español e inglés
└── Infrastructure/                   ObservableObject, RelayCommand, Dialogs
```

**Cómo se comunica con yt-dlp**
- Los argumentos se pasan con `ArgumentList` (cada uno escapado) y la URL va tras `--`: no hay inyección de opciones.
- El progreso se lee con `--progress-template`; el título, el id y la ruta final, con `--print`.
- Se pasa siempre `--encoding utf-8`. Sin él, el `.exe` de yt-dlp devuelve `?`/`�` en lugar de tildes y ñ
  aunque se configure el entorno.
- **Pausa** = matar el proceso conservando los `.part`; **reanudar** = relanzar (yt-dlp continúa solo).

### Tareas habituales para desarrolladores
- **Añadir un texto:** una línea `Add("Clave", "español", "english")` en `Strings.cs`; en XAML `{loc:T Clave}`,
  en código `Loc.T("Clave")`. Una clave sin traducir se ve como `[Clave]`.
- **Añadir un idioma:** añadir el código en `Loc.Supported`, otra tabla en `Strings.cs` y una entrada en `Languages`
  del `MainViewModel`.
- **Añadir un formato o resolución:** ajustar las listas del constructor de `MainViewModel` y, si hace falta,
  `DownloadOptions.ToArgs()`.

### Publicar (sin probar)
```powershell
dotnet publish src\YTFE -c Release -r win-x64 --self-contained
```
Genera una carpeta que no requiere tener .NET instalado. Aún no se ha probado el empaquetado ni se ha creado instalador.

---

## Licencia

YTFE se distribuye bajo la **licencia MIT** (ver [LICENSE](LICENSE)): puedes usarlo, copiarlo, modificarlo y
redistribuirlo, también con fines comerciales, conservando el aviso de copyright. Se ofrece "tal cual", sin garantía.

## Licencias y componentes de terceros

Lo siguiente **no** está cubierto por la licencia MIT de YTFE; cada componente mantiene la suya.

| Componente | Origen | Licencia |
|---|---|---|
| yt-dlp | github.com/yt-dlp/yt-dlp | Unlicense (dominio público) |
| ffmpeg (build GPL de yt-dlp/FFmpeg-Builds) | se **descarga en ejecución** desde su fuente | GPL |
| deno | github.com/denoland/deno | MIT |
| Logos de sitios (trazos vectoriales) | [Simple Icons](https://simpleicons.org) | CC0. Las marcas siguen siendo de sus propietarios; se usan solo para identificar el sitio |

YTFE no incluye ni redistribuye ffmpeg: lo descarga el usuario desde su origen. **Si algún día lo empaquetas dentro
de un instalador, pasarías a redistribuirlo** y deberás cumplir la GPL (o cambiar a un build LGPL).

---

## Limitaciones conocidas

- **Estado de las pruebas.** La interfaz se ha ejecutado y se han corregido varios fallos, pero **no se ha hecho una
  batería de pruebas manual completa** de todos los botones. Lo verificado con yt-dlp real: la pausa y reanudación de
  una descarga, la codificación UTF-8 y la lectura de versiones. No hay pruebas automatizadas.
- La lista se guarda, pero **no se reanudan solas** las descargas al reabrir (a propósito).
- Las listas se descargan con la casilla «Descargar lista / álbum completo» y aparecen como **una sola fila** (no se probó con listas reales).
- No hay límite de velocidad ni programación de descargas.
- Los errores de yt-dlp se traducen solo los más comunes (`ErrorHints.cs`); el resto aparece tal cual, en inglés.
- Las cookies de Chrome/Edge/Brave pueden no leerse con el navegador abierto; si falla, se reintenta sin cookies.
- Una compilación local sin firmar puede ser bloqueada por Windows (ver arriba).
- Si se reanuda una descarga restaurada antes de que termine la comprobación inicial de herramientas, podría fallar.
- Al comprobar versiones de ffmpeg/deno se usa la API de GitHub sin autenticar (60 consultas/hora; suficiente).

## Ideas pendientes
- Varias URL a la vez; límite de velocidad; programación.
- Pruebas automatizadas (el parseo de progreso, `ToArgs()` y `DownloadStore` son fáciles de probar).
- Icono, instalador y firma de código; actualización de la propia aplicación.
- Notificación al completar una descarga.

## Nota sobre contribuir a yt-dlp
El `CONTRIBUTING.md` de yt-dlp prohíbe el uso de IA en issues, pull requests y comentarios de su repositorio.
Esto **no afecta a usar yt-dlp como herramienta** en este proyecto, pero sí si piensas reportar errores o enviar
cambios allí.
