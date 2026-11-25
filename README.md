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

## Descargar e instalar

1. Ve a [**Releases**](https://github.com/Damanium/YTFE/releases/latest) y descarga `YTFE-<versión>-win-x64.zip`.
2. *(Recomendado)* Antes de descomprimir: clic derecho en el ZIP → **Propiedades** → marca **Desbloquear** → Aceptar.
3. Descomprime y ejecuta `YTFE.exe`. No hace falta instalar .NET: el ejecutable lo lleva incluido.

Cada release incluye el **SHA-256** del ZIP (en las notas y en `SHA256SUMS.txt`). Para comprobarlo:

```powershell
Get-FileHash .\YTFE-1.1.1-win-x64.zip -Algorithm SHA256
```

Y, si tienes la [CLI de GitHub](https://cli.github.com/), puedes verificar que el ZIP salió realmente de este
repositorio (atestación de procedencia, disponible en las releases generadas por la tarea automática):

```powershell
gh attestation verify .\YTFE-1.1.1-win-x64.zip --repo Damanium/YTFE
```

### Si Windows bloquea la aplicación

**Las versiones publicadas no están firmadas digitalmente** (de momento no se prevé firmarlas). Windows desconfía de los programas sin firma
ni reputación, y puede reaccionar de tres formas distintas:

| Qué ves | Qué es | Qué hacer |
|---|---|---|
| «Windows protegió su PC» | **SmartScreen** (reputación) | Pulsa **Más información → Ejecutar de todas formas** |
| «Una directiva de Control de aplicaciones bloqueó este archivo» en tu PC personal | **Smart App Control** | Ver abajo: no hay botón para saltarlo |
| El mismo mensaje en un PC de trabajo o de estudios | **WDAC / AppLocker** (política de la organización) | Pide a quien administre el equipo que permita la aplicación |

**Smart App Control (SAC)** es una protección de Windows 11 que bloquea todo programa que no esté firmado por una
entidad de confianza o no sea conocido. No admite excepciones por aplicación. Tus opciones son:

- **Desactivar Smart App Control** en *Seguridad de Windows → Control de aplicaciones y del navegador →
  Configuración de Smart App Control → Desactivado*. Mientras esté desactivado, el equipo queda **menos protegido**
  frente a programas desconocidos: hazlo solo si entiendes el riesgo y confías en el origen del programa. En las
  versiones recientes de Windows 11 (con las actualizaciones de 2026) puedes **volver a activarlo** desde el mismo
  sitio cuando quieras; en versiones antiguas, una vez desactivado no se podía reactivar sin restablecer o reinstalar
  Windows, así que comprueba antes que tu Windows está actualizado y que la opción te deja elegir *Activado*.
- **Compilarlo tú mismo no lo evita**: un ejecutable compilado en local tampoco está firmado, así que SAC lo bloquea
  igual. Por eso Microsoft recomienda tener SAC desactivado en los equipos de desarrollo.

> **Aviso sobre las herramientas descargadas.** En el primer arranque YTFE descarga `yt-dlp.exe`, `ffmpeg.exe` y
> `deno.exe`, que son programas de terceros y los firmantes no somos nosotros. Con Smart App Control activo podrían
> bloquearse también. Esto **no se ha verificado** en un Windows con SAC activado.

Sea cual sea el caso, descarga siempre desde la página oficial de [Releases](https://github.com/Damanium/YTFE/releases)
y comprueba el SHA-256.

---

## Compilar desde el código

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

### Publicar una versión

**Automático (recomendado).** El flujo [`.github/workflows/release.yml`](.github/workflows/release.yml) compila en un
Windows limpio, empaqueta el ZIP, calcula el SHA-256, genera la atestación de procedencia y crea la release:

```powershell
git tag v1.1.1
git push origin v1.1.1
```

También se puede lanzar a mano desde la pestaña **Actions → Release → Run workflow**: compila y deja el ZIP como
artefacto de esa ejecución, pero no crea ninguna release.

> El flujo automático ya se ha ejecutado en GitHub con éxito (releases 1.1.0 y 1.1.1). La versión 1.0.0 se generó
> a mano con el comando de abajo.

**A mano.** Es el mismo comando que usa el flujo. Genera un único `YTFE.exe` de unos 65 MB, con .NET incluido
(el ZIP pesa ~57 MB):

```powershell
dotnet publish src\YTFE -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish\win-x64
```

La versión 1.0.0 se compiló así y se comprobó que el ejecutable arranca y responde. No hay instalador (MSI/MSIX):
se distribuye como ZIP portátil.

### Firma de código (para mantenedores)

El flujo ya incluye los pasos de firma con [SignPath](https://signpath.org), que ofrece certificados **gratuitos para
proyectos de código abierto** (SignPath Foundation). Están **desactivados** hasta que se complete esto:

1. **Solicitar el certificado** a SignPath Foundation (el repositorio es público y la licencia, MIT). Cada proyecto
   se revisa a mano; la aprobación no está garantizada. *Estado: no solicitada; de momento no se prevé firmar.*
2. En SignPath, crear el proyecto, una política de firma de release y una configuración de artefacto que espere un
   ZIP con `YTFE.exe` en la raíz, e instalar su conector de GitHub.
3. En el repositorio (*Settings → Secrets and variables → Actions*):
   - **Secreto** `SIGNPATH_API_TOKEN`
   - **Variables** `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG`
   - **Variable** `SIGNPATH_ENABLED` = `true` (el interruptor que activa la firma)
4. Añadir a este README la **política de firma de código** que exige SignPath y la frase
   *«Free code signing provided by SignPath.io, certificate by SignPath Foundation»*.

Alternativas si SignPath no lo aprueba: Azure Trusted Signing o un certificado de firma de código de pago;
en ambos casos solo cambian los pasos marcados como «Firma de código» del flujo.

---

## Licencia

YTFE se distribuye bajo la **licencia MIT** (ver [LICENSE](LICENSE)): puedes usarlo, copiarlo, modificarlo y
redistribuirlo, también con fines comerciales, conservando el aviso de copyright. Se ofrece "tal cual", sin garantía.

## Créditos

YTFE es solo una interfaz; el trabajo lo hacen proyectos de otras personas (también visibles en la app: **Ayuda → Acerca de y créditos**):

- **[yt-dlp](https://github.com/yt-dlp/yt-dlp)**: creado por [pukkandan](https://github.com/pukkandan) y mantenido por los colaboradores de yt-dlp, a partir de [youtube-dl](https://github.com/ytdl-org/youtube-dl) (Ricardo Garcia y su comunidad).
- **[FFmpeg](https://github.com/FFmpeg/FFmpeg)** ([ffmpeg.org](https://ffmpeg.org)): los desarrolladores de FFmpeg (fundado por Fabrice Bellard); compilaciones de [yt-dlp/FFmpeg-Builds](https://github.com/yt-dlp/FFmpeg-Builds).
- **[Deno](https://github.com/denoland/deno)**: [Ryan Dahl](https://github.com/ry) y los colaboradores de Deno.
- **[Simple Icons](https://github.com/simple-icons/simple-icons)**: logos de los sitios.
- **.NET / WPF** ([dotnet/runtime](https://github.com/dotnet/runtime), [dotnet/wpf](https://github.com/dotnet/wpf)): Microsoft y la comunidad de .NET.

## Apoyar el proyecto

YTFE es gratuito y lo seguirá siendo. Si te resulta útil y quieres echar una mano, puedes hacer una donación voluntaria por [PayPal](https://paypal.me/damanium). No es necesario y no da ninguna ventaja.

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
- Los ejecutables no están firmados, así que Windows puede bloquearlos (ver [Si Windows bloquea la aplicación](#si-windows-bloquea-la-aplicación)).
- Si se reanuda una descarga restaurada antes de que termine la comprobación inicial de herramientas, podría fallar.
- Al comprobar versiones de ffmpeg/deno se usa la API de GitHub sin autenticar (60 consultas/hora; suficiente).

## Ideas pendientes
- Varias URL a la vez; límite de velocidad; programación.
- Pruebas automatizadas (el parseo de progreso, `ToArgs()` y `DownloadStore` son fáciles de probar).
- Instalador (MSIX o Inno Setup), firma de código (ver arriba) y actualización de la propia aplicación.
- Notificación al completar una descarga.

## Nota sobre contribuir a yt-dlp
El `CONTRIBUTING.md` de yt-dlp prohíbe el uso de IA en issues, pull requests y comentarios de su repositorio.
Esto **no afecta a usar yt-dlp como herramienta** en este proyecto, pero sí si piensas reportar errores o enviar
cambios allí.
