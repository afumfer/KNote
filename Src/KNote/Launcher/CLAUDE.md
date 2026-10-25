# CLAUDE.md

Guía de `Launcher/` (`KNote.Launcher`), el lanzador de **KNote Local**. Lee antes el `CLAUDE.md` raíz y la sección
"KNote Local" de `Server/CLAUDE.md`. Guía de usuario: `Docs/KNoteLocal.md` / `Docs/KNoteLocal_es.md`.

## Qué hace

Es lo que el usuario abre (menú de inicio, `KNote.app`, el menú de aplicaciones): arranca su servidor local si no está
en marcha, espera a que responda y abre el navegador en KNote. Después termina; **el servidor sigue vivo**.

1. Lee la carpeta de datos del usuario (`LocalInstance`): `--data`, `Local__DataPath` o la predeterminada, la misma que la
   del `Server` (`KntLocalData`). De `knote-local.json` solo lee dónde escucha (`Kestrel:Endpoints:Http:Url`; una dirección
   de todas las interfaces se alcanza en `127.0.0.1`) y el id de la instalación (`Local:InstanceId`).
2. Pregunta a `/KNote/health` quién responde (`ServerHealth`): su KNote (mismo `InstanceId`), el KNote de otro usuario
   del equipo, otro programa o nadie. Si el puerto es de otro, lo explica (con `--port`) y termina.
3. Si no responde nadie y no hay un PID vivo de un servidor arrancando (otro clic del lanzador), arranca
   `dotnet server/KNote.Server.dll` **oculto** (`ServerStarter`: `CreateNoWindow`, sin redirigir la salida), con
   `ASPNETCORE_ENVIRONMENT=Local`, `Local__DataPath` y el directorio de trabajo en la carpeta del servidor.
4. Espera (máximo 90 s) a que la salud responda; el primer arranque crea antes la configuración y el puerto. Mira la
   salud antes que si el proceso terminó: con dos clics a la vez el segundo servidor falla al abrir el puerto, pero el
   primero responde.
5. Abre el navegador predeterminado (`BrowserOpener`: el shell en Windows, `open` en macOS, `xdg-open` en Linux, como
   `ClientWin` su ayuda) o, con `--app-window`, una ventana de aplicación de Edge o Chrome (`--app=url`) si hay uno.

Los problemas se avisan con el aviso del sistema (`Notifier`: `MessageBoxW` de `user32` en Windows, `osascript` en
macOS, `notify-send` en Linux) y todo se apunta en `Logs/launcher.log` de la carpeta de datos.

## Argumentos (`LauncherOptions`)

| Argumento | Uso |
|---|---|
| `--no-browser` | Arranca el servidor sin abrir el navegador (arranque al iniciar sesión) |
| `--app-window` | Ventana de aplicación en lugar del navegador predeterminado |
| `--stop` | Para el servidor: `POST /KNote/local/stop` con el token de control y, si no para en 15 s, termina el proceso del PID (solo si es `dotnet` o KNote: un PID se puede reutilizar). Lo usan los instaladores y las actualizaciones |
| `--port N` | El puerto desde ahora (1024-65535), en una instalación ya creada; la primera vez el servidor elige uno libre solo |
| `--data PATH` | Otra carpeta de datos (pruebas, desarrollo) |
| `--server PATH` | Otro `KNote.Server.dll` |

Código de salida: 0 bien, 1 problema (ya avisado), 2 argumentos no válidos.

## Reglas

- **Sin referencias a otros proyectos de KNote**: solo APIs de .NET. Lo que comparte con el `Server` (nombres de
  ficheros de la carpeta de datos, cabecera `X-KNote-Control`, forma de la respuesta de salud) se mantiene a mano en los
  dos lados.
- `WinExe`: sin ventana de consola en Windows; en macOS y Linux es una aplicación de consola normal que se ejecuta con
  `dotnet KNote.Launcher.dll` (la `.app` y el `.desktop` lo hacen así).
- Busca `dotnet` (`ServerStarter.FindDotnet`): el que ejecuta el lanzador, `DOTNET_ROOT`, el `PATH` y las rutas de los
  instaladores de cada sistema (una app abierta desde Finder o un menú no recibe el `PATH` de la shell).
- Busca el servidor (`FindServer`): `--server`, `server/KNote.Server.dll` junto al lanzador (instalado) o, ejecutado desde
  su carpeta de compilación (`Launcher/bin/<Configuración>/net10.0`), la compilación del `Server` de la misma
  configuración.

## Compilar, ejecutar, depurar y probar

- El lanzador no referencia al `Server`: compila los dos. VS: compila la solución y ejecuta el perfil `KNote.Launcher`
  (también `KNote.Launcher (app window)` y `KNote.Launcher (stop)`). VS Code: configuración `KNote.Launcher` (tarea previa
  `build: Local`, que compila los dos) y `KNote.Launcher (stop)`.
- Sin `--data` usa la carpeta de datos real del usuario. Para probar: `KNote.Launcher --no-browser --data <carpeta de
  pruebas>` y `--stop --data <la misma>`.
- Tests: `Tests/LauncherTests.cs` (argumentos, configuración, puerto, salud, `dotnet` y servidor; no arranca nada).
- **Paquete e instaladores** (`DeployLocalServer/`): se construye como los demás proyectos, publicando a una carpeta.
  VS: botón derecho en **KNote.Launcher → Publish → LocalServerPackage → Publish**; consola: `dotnet publish
  Launcher/KNote.Launcher.csproj -p:PublishProfile=LocalServerPackage`. El perfil
  (`Properties/PublishProfiles/LocalServerPackage.pubxml`, excepción en el `.gitignore`, que ignora los `*.pubxml`) activa
  `KntLocalServerPackage`, y `DeployLocalServer/KNote.LocalServer.targets` (importado por el `.csproj`) publica además el
  `Server` en `server\`, copia los scripts, los iconos y los README y lo comprime: `DeployLocalServer/out/KNote-Local` y
  `KNote-Local-<versión>.zip`. Sin ese perfil no hace nada. Las propiedades globales de la publicación del lanzador que
  no son del servidor se quitan en su llamada (`KntNotForTheServer`; `PublishDir` se le pasa, no se quita). Los `.sh`
  van con LF (`DeployLocalServer/.gitattributes`). Los `README.md`/`README_es.md` de esa carpeta son para el usuario
  final.
