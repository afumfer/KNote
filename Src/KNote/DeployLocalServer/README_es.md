# KNote Local — instalación

KNote Local es la aplicación Web de KNote funcionando entera dentro de tu equipo, sin internet, en **Windows, macOS y
Linux**. Esta carpeta es su paquete de instalación. La guía completa (qué es, cómo funciona, la pantalla
**This computer**...) está en la documentación de KNote: `Docs/KNoteLocal_es.md`.

## 1. Antes de instalar (una sola vez)

KNote necesita dos componentes gratuitos de Microsoft, el **.NET Runtime 10** y el **ASP.NET Core Runtime 10**. Se
descargan de <https://dotnet.microsoft.com/download/dotnet/10.0>:

| Sistema | Qué instalar |
|---|---|
| Windows | En el bloque **ASP.NET Core Runtime 10.0.x**, el **Hosting Bundle**: un solo instalador con los dos componentes. (El instalador "x64" de ese bloque **no** trae el .NET Runtime: si lo usas, instala también el **.NET Runtime 10.0.x** x64.) |
| Mac | En el bloque **SDK 10.0.x**, el instalador de tu Mac (Arm64 si es Apple Silicon, x64 si es Intel): trae los dos componentes (el ASP.NET Core Runtime no tiene instalador propio en Mac). |
| Linux | El paquete `aspnetcore-runtime-10.0` de tu distribución (instala también el .NET Runtime). Las instrucciones de cada distribución están en esa misma página. |

Para comprobarlo, abre una terminal y escribe `dotnet --list-runtimes`: deben aparecer **dos** líneas,
`Microsoft.NETCore.App 10.0.x` y `Microsoft.AspNetCore.App 10.0.x`.

## 2. Preparar el paquete

- **Windows**: si el zip viene de internet o de otro equipo, desbloquéalo antes de descomprimirlo: botón derecho sobre el
  zip → **Propiedades** → marca **Desbloquear** → **Aceptar**. Después, botón derecho → **Extraer todo**.
- **Mac y Linux**: descomprímelo (doble clic o `unzip`).

Queda una carpeta con el lanzador (`KNote.Launcher`), la carpeta `server` y los scripts de instalación.

## 3. Instalar

Abre una terminal **en la carpeta descomprimida** (en Windows: botón derecho en un espacio vacío de la carpeta → **Abrir
en Terminal**) y ejecuta el script de tu sistema:

| Sistema | Comando | Después |
|---|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File .\install-windows.ps1` | Abre **KNote** desde el menú de inicio |
| Mac | `bash install-macos.sh` | Abre **KNote** desde `~/Applications` (o Launchpad); puedes arrastrarlo al Dock |
| Linux | `bash install-linux.sh` | Abre **KNote** desde el menú de aplicaciones |

Opciones:

- `-StartAtSignIn` (Windows) o `--start-at-login` (Mac, Linux): el servidor local arranca solo al iniciar sesión, para
  que KNote se abra al instante.
- `-AppWindow` (Windows): el acceso del menú de inicio abre KNote en una ventana de aplicación, sin barra de direcciones.

Los programas se instalan en tu perfil de usuario (no hace falta ser administrador del equipo):

| Sistema | Programas | Tus datos (notas, configuración, registros) |
|---|---|---|
| Windows | `%LocalAppData%\Programs\KNote` | `%LocalAppData%\KNote\Server` |
| Mac | `~/Applications/KNote.app` | `~/Library/Application Support/KNote/Server` |
| Linux | `~/.local/share/knote/app` | `~/.local/share/KNote/Server` |

## 4. Primer uso

1. Abre **KNote**. Se abre el navegador en `http://127.0.0.1:5280/KNote/` (u otro puerto, si ese estaba ocupado).
2. **Regístrate**: la primera persona que se registra queda como **administradora**.
3. Desde **Admin panel → This computer** puedes crear o vincular repositorios (por ejemplo, la base de datos de la
   aplicación de escritorio), configurar los proveedores de inteligencia artificial y la cuenta de correo de las alarmas.
4. Si quieres, instálalo como aplicación desde el navegador (el icono de instalar de la barra de direcciones de Chrome o
   Edge, o **Archivo → Añadir al Dock** en Safari).

Si Windows muestra **"Windows protegió su PC"** (el programa no está firmado), pulsa **Más información → Ejecutar de
todas formas**. En Mac, si dice que no puede abrirlo, abre **Ajustes del Sistema → Privacidad y seguridad** y pulsa
**Abrir igualmente**.

## 5. Actualizar

Descomprime el paquete nuevo e instálalo con el mismo comando: se detiene el servidor, se sustituyen los programas y
**tus notas y tu configuración se conservan**.

## 6. Desinstalar

| Sistema | Comando |
|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\Programs\KNote\uninstall-windows.ps1"` |
| Mac | `bash ~/Applications/KNote.app/Contents/Resources/app/uninstall-macos.sh` |
| Linux | `bash ~/.local/share/knote/app/uninstall-linux.sh` |

Se quitan los programas y los accesos. Tu carpeta de datos se conserva por si vuelves a instalarlo: bórrala a mano si ya
no la quieres.

## Si algo va mal

| Qué pasa | Qué hacer |
|---|---|
| Aviso de que falta el .NET Runtime 10 o el ASP.NET Core Runtime 10, o Windows dice que hay que instalar .NET para ejecutar la aplicación | Instala lo que indica el paso 1 (en Windows, el Hosting Bundle) y vuelve a abrir KNote |
| Aviso de que otro programa usa el puerto | Abre KNote una vez con otro puerto: en Windows `& "$env:LOCALAPPDATA\Programs\KNote\KNote.Launcher.exe" --port 5300`; en Mac y Linux, `dotnet KNote.Launcher.dll --port 5300` desde la carpeta de los programas. KNote lo recuerda |
| No se abre nada | Mira `Logs\launcher.log` y los registros del servidor en tu carpeta de datos |
| Quieres apagar el servidor local | **Admin panel → This computer → Stop local server**, o el lanzador con `--stop` |

---

**Para quien construye el paquete**: en Visual Studio, botón derecho sobre el proyecto **KNote.Launcher** → **Publish** →
perfil **LocalServerPackage** → **Publish** (en la consola, `dotnet publish Launcher/KNote.Launcher.csproj
-p:PublishProfile=LocalServerPackage`). El paquete queda en `DeployLocalServer/out/KNote-Local` y su zip,
`KNote-Local-<versión>.zip`, junto a él (ver `KNote.LocalServer.targets`).
