# KNote

[English](README.md) | Español

KNote es un gestor de notas y tareas minimalista. Las tareas o notas se almacenan en bases de datos y se pueden organizar en carpetas temáticas. Estas notas pueden contener diferentes recursos (imágenes, archivos adjuntos, ...), vincular diversas alarmas y enriquecerse con atributos personalizados. KNote integra también un lenguaje minimalista con el que puedes automatizar tareas. Además de gestor de tareas puedes darle otros usos a KNote, por ejemplo un pequeño gestor de contenido.

Se soportan dos motores diferentes de bases de datos, Sqlite para tareas personales y SQL server para uso corporativo.

KNote tiene también dos front-end diferentes, una aplicación windows de escritorio a la vieja escuela (WinForm app) y un aplicativo Web (Blazor app).

Funcionalidades principales:

- Notas organizadas en un árbol de carpetas, con etiquetas, prioridades, atributos personalizados, tareas, alarmas, recursos (imágenes, ficheros) y notas de traza (relaciones entre notas).
- Descripciones escritas en HTML o en Markdown, con diagramas [Mermaid](https://mermaid.js.org/).
- Varios repositorios (bases de datos) abiertos a la vez en la aplicación de escritorio, cada uno con sus propios usuarios y roles (Guest, Staff, Project manager, Admin).
- Informes imprimibles (lista de notas, libro, detalle de una nota) y exportación a CSV.
- Un asistente de IA (OpenAI, Anthropic u Ollama) que puede buscar en tus notas y crear tareas, con sus conversaciones guardadas como notas.
- Un chat room compartido por la aplicación de escritorio y la aplicación Web.
- KntScript, un lenguaje minimalista de automatización, y una consola de scripts.

La guía de usuario está en [Docs/Manual_es.md](Docs/Manual_es.md) (en inglés, [Docs/Manual.md](Docs/Manual.md)).


## Compilación y uso

**Requisitos**

- [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Windows, para la aplicación de escritorio (es una aplicación WinForms), con el [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (ya incluido en Windows 11).
- Para desarrollar y depurar: Visual Studio 2026 (cargas de trabajo *Desarrollo de escritorio de .NET* y *Desarrollo de ASP.NET y web*) o Visual Studio Code con la extensión C# Dev Kit.

**Soluciones**

El código fuente está en `Src/KNote`. No hay un único `.sln`, sino varias soluciones `.slnx`:

| Solución | Contenido |
|---|---|
| `KNote.slnx` | La aplicación completa: servidor, cliente Web, cliente de escritorio y sus librerías. |
| `KNoteTest.slnx` | Los tests de integración del backend. |

**Compilar y ejecutar desde la línea de comandos** (desde `Src/KNote`):

```powershell
# Compilar la aplicación completa
dotnet build KNote.slnx

# Aplicación de escritorio
dotnet run --project ClientWin/KNote.ClientWin.csproj

# Servidor Web (sirve también la aplicación Web, en https://localhost:44339/KNote)
dotnet run --project Server/KNote.Server.csproj --launch-profile KNote.Server
```

**Visual Studio.** Abre `Src/KNote/KNote.slnx` y elige el proyecto de inicio: `KNote.ClientWin` para la aplicación de escritorio, o `KNote.Server` (perfil `KNote.Server`) para la aplicación Web; al depurar el servidor se depura también el cliente Web.

**Visual Studio Code.** Abre la carpeta `Src/KNote`. La vista *Run and Debug* tiene las configuraciones `KNote.ClientWin` (aplicación de escritorio), `KNote.Web (Server + Client WASM)` (servidor y cliente Web) y `KNote.Server` (solo el servidor).

**Primer arranque de la aplicación de escritorio.** La primera vez que arranca, la aplicación de escritorio crea una base de datos Sqlite personal en `%LocalAppData%\KNote\Data` y te identifica con tu cuenta de Windows, como Admin de ese repositorio. Desde el menú **Repository** puedes crear o vincular otros repositorios (Sqlite o SQL Server).

**Configurar el servidor Web.** Los ajustes del servidor están en `Server/appsettings.json`, que solo contiene marcadores. Escribe los valores reales (base de datos, secreto del JWT, claves de API, contraseña del correo...) en los user secrets del servidor, nunca en `appsettings.json`:

```powershell
dotnet user-secrets set "AppSettings:Secret" "<una clave larga y aleatoria>" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:Orm" "EntityFramework" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:Provider" "Microsoft.Data.Sqlite" --project Server/KNote.Server.csproj
dotnet user-secrets set "RepositoryRef:ConnectionString" "Data Source=D:\KNoteData\knote.db" --project Server/KNote.Server.csproj
```

| Ajuste | Significado |
|---|---|
| `AppSettings:Secret` | La clave que firma las sesiones de los usuarios (JWT). |
| `RepositoryRef:Orm` | `EntityFramework` o `Dapper`. |
| `RepositoryRef:Provider` | `Microsoft.Data.Sqlite` o `Microsoft.Data.SqlClient` (SQL Server). |
| `RepositoryRef:ConnectionString` | La cadena de conexión de la base de datos. |
| `RepositoryRef:Name` | El nombre que recibe una base de datos nueva. |
| `RepositoryRef:ResourcesContainer`, `ResourcesContainerRootPath`, `ResourcesContainerRootUrl` | Dónde se guardan los recursos de las notas (imágenes, ficheros) y la URL desde la que se sirven (vacíos: los valores por defecto del servidor). |

La base de datos se crea, o se actualiza a la versión actual, al arrancar el servidor. El primer usuario que se registra en una base de datos nueva pasa a ser su Admin; los siguientes se registran como Guest hasta que un Admin les sube el rol. El asistente de IA y el restablecimiento de la contraseña por correo tienen sus propios ajustes, descritos en el manual.

**Tests.** `dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj` ejecuta los tests unitarios de la aplicación de escritorio. `dotnet test KNoteTest.slnx` ejecuta los tests del backend; los de `Tests/WebApiIntegrationTests` necesitan un servidor en ejecución (ver `Tests/appsettings.json`).


## Instalación y despliegue

**Aplicación de escritorio.** Publícala con:

```powershell
dotnet publish ClientWin/KNote.ClientWin.csproj -c Release -o <carpeta de salida>
```

La aplicación de escritorio no necesita instalador:

1. En el equipo donde se vaya a usar, instala el [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (Windows x64) y, en Windows 10, el [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).
2. Copia la carpeta de salida donde quieras (por ejemplo, `C:\Program Files\KNote` o una carpeta de tu usuario) y ejecuta `KNote.ClientWin.exe`. Puedes crearle un acceso directo en el escritorio o en el menú de inicio.
3. Para actualizarla, cierra KNote y sustituye los ficheros de la carpeta por los nuevos. Cada usuario de Windows guarda su configuración y su base de datos personal en `%LocalAppData%\KNote`, que no se toca, y las bases de datos se actualizan solas la primera vez que la nueva versión las abre (guarda antes una copia de una base de datos compartida).

Para desinstalarla, borra la carpeta; borra también `%LocalAppData%\KNote` si quieres eliminar además tu configuración y tu base de datos personal.

**Aplicación Web.** Publica el servidor, que incluye la aplicación Web:

```powershell
dotnet publish Server/KNote.Server.csproj -c Release -o <carpeta de salida>
```

La aplicación Web se sirve siempre bajo la ruta `/KNote` (p. ej. `https://miservidor/KNote`). Para alojarla en IIS:

1. Instala el [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0) de .NET 10.
2. Copia la carpeta publicada al servidor y crea una aplicación de IIS con el alias `KNote` que apunte a ella, con un grupo de aplicaciones *Sin código administrado*.
3. Da a la identidad del grupo de aplicaciones permiso de escritura en la carpeta `Log`, en la carpeta de la base de datos Sqlite (si usas Sqlite) y en la carpeta de los recursos.
4. Configura los ajustes descritos en *Compilación y uso* con variables de entorno (por ejemplo, `RepositoryRef__ConnectionString`, en las `environmentVariables` del `web.config` o del grupo de aplicaciones) o con el almacén de secretos de tu hosting. No escribas secretos en `appsettings.json`.

También puede ejecutarse sin IIS, como un proceso independiente: `KNote.Server.exe --urls "https://*:5001"`.

Mantén una base de datos Sqlite en un disco local del servidor, no en una carpeta de red. Para una base de datos compartida por muchos usuarios, el motor recomendado es SQL Server.


## Desarrollo asistido por IA

Desde la versión 0.0.10.11, este software se desarrolla con una fuerte asistencia de agentes de programación con IA ([Claude Code](https://claude.com/claude-code)) y con personas que dirigen las ideas, el diseño, las pruebas y la depuración. La mayor parte del código, los tests y la documentación técnica los genera Claude Code bajo la dirección del autor, y cada cambio se revisa antes de integrarlo. Lo decimos abiertamente porque ha marcado la forma en que se ha construido el proyecto.


## Licencia

Este proyecto está bajo la GNU General Public License, versión 2 o (a tu elección) cualquier versión posterior; consulta el fichero LICENSE.txt para más detalles.

(This program is under construction and the documentation isn't ready yet. Thanks for your patience!)
