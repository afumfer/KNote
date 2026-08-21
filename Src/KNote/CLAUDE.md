# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visión general del proyecto

KaNote ("KNote") es un gestor de notas/tareas escrito en C#/.NET 10. Tiene dos frontends que comparten la
misma lógica de backend: una app de escritorio WinForms (`ClientWin`) y una app Blazor WebAssembly
(`Client`) servida por una API ASP.NET Core (`Server`). Soporta dos motores de BD (Sqlite, SQL Server)
detrás de dos implementaciones de repositorio intercambiables (Dapper, Entity Framework Core).

## Archivos de solución

No hay un único `.sln` en la raíz; hay varios `.slnx` ("VS solution XML"), cada uno cubriendo una parte
distinta del código:

- `KNote.slnx` — la app completa: `Server`, `Model`, `Service`, `Repository*`, `ClientWin`, `Client`,
  `KntScript`, `MessageBroker*`, `HtmlEditorControl`, `KntEditViewControl`. Úsalo para la mayoría del trabajo.
- `KNoteTest.slnx` — solo `Model` + `Tests`, para ejecutar la suite de tests de integración de forma aislada.

Existen además otros `.slnx` específicos de subproyectos (por ejemplo el de `KntRedmineApi`, documentado en
su propio `CLAUDE.md`); elige el `.slnx` correspondiente al área en la que trabajes en lugar de compilarlo
todo.

## Comandos habituales

```powershell
# Compilar la app principal
dotnet build KNote.slnx

# Compilar/ejecutar el servidor web (también sirve el Client Blazor)
dotnet run --project Server/KNote.Server.csproj

# Compilar/ejecutar el cliente de escritorio WinForms
dotnet run --project ClientWin/KNote.ClientWin.csproj

# Ejecutar la suite de tests de integración (ver "Tests" más abajo — requiere un Server en ejecución)
dotnet test KNoteTest.slnx
# test individual:
dotnet test KNoteTest.slnx --filter "FullyQualifiedName~NotesTests.SomeTestMethod"
```

No hay ningún workflow de CI configurado (`.github/workflows/` está vacío) ni linter más allá de la única
regla del `.editorconfig` (`csharp_prefer_braces = false:silent`) — los cuerpos de `if` en una línea sin
llaves están permitidos y son habituales en este código.

## Tests

`Tests/WebApiIntegrationTests/*.cs` (`ChatGPTTests`, `FoldersTests`, `KAttributesTests`, `NoteTypesTests`,
`NotesTests`, `UsersTests`) son **tests de integración HTTP reales**, no tests unitarios.
`Tests/Helpers/WebApiTestBase.cs` inicia sesión vía `POST {testsWebApiUrlBase}api/users/login` contra una
instancia real de `Server` en ejecución y reutiliza el JWT en las siguientes peticiones. Configura
`testsUserName`, `testsUserPwd`, `testsWebApiUrlBase` en `Tests/appsettings.json` o en user-secrets (id de
secretos `f25fed7b-9b03-406c-8e4a-b98eb14f5579`) — **debe haber una instancia de `Server` en ejecución y
accesible en esa URL antes de correr el proyecto de tests.**

## Arquitectura

### Grafo de dependencias entre proyectos

```
Model  (hoja: DTOs en Model/Dto, tipos compartidos, RepositoryRef/AppConfig — sin referencias a otros proyectos)
  ├─ Repository                       (solo interfaces: IKntNoteRepository, IKntFolderRepository, ...)
  │    ├─ Repository.Dapper           (implementación con Dapper de las mismas interfaces)
  │    └─ Repository.EntityFramework  (implementación con EF Core + KntDbContext + Entities/)
  ├─ MessageBroker
  │    └─ MessageBroker.RabbitMQ
  ├─ Service                          (→ Repository, Repository.Dapper, Repository.EntityFramework, MessageBroker*)
  │    └─ ClientWin                   (→ también HtmlEditorControl, KntEditViewControl, KntScript)
  └─ Client                           (Blazor WASM; habla con Server por HTTP, no con Service/Repository)

Server → Client, Model, Service
KntEditViewControl → HtmlEditorControl
```

`KntScript` y `HtmlEditorControl` no tienen referencias a otros proyectos (son hojas usadas solo por
`ClientWin`).

### Patrón Repository (ORM intercambiable)

`Repository/` define solo interfaces (`IKntRepository`, `IKntNoteRepository`, `IKntFolderRepository`,
`IKntKAttributeRepository`, `IKntNoteTypeRepository`, `IKntSystemValuesRepository`, `IKntUserRepository`).
`Repository.Dapper` y `Repository.EntityFramework` son dos implementaciones independientes de esas mismas
interfaces. Cuál está activa se decide en tiempo de ejecución mediante `RepositoryRef.Orm` ("Dapper" o
"EntityFramework") en la configuración:

- En `Server`: `Server/Program.cs` llama a `builder.Services.KntAddServices(appSettings, repositoryRef)`
  (`Server/Helpers/KntExtensions.cs`), que bifurca según `Orm` y registra `IKntRepository` con la
  implementación Dapper o EF vía DI.
- En `ClientWin`: no hay contenedor de DI — `Service/Core/ServiceRef.cs` hace la misma bifurcación de forma
  manual, construyendo `DP.KntRepository` o `EF.KntRepository` directamente, y `ClientWin/Core/Store.cs`
  mantiene una lista de estos `ServiceRef` (cada app puede tener varias bases de datos de notas configuradas
  abiertas a la vez).

`RepositoryRef` (en `Model`) también incluye `Provider` (`Microsoft.Data.SqlClient` vs
`Microsoft.Data.Sqlite`) y `ConnectionString`, por lo que el motor de BD y el ORM son elecciones
independientes. Se configuran en `Server/appsettings.json` → sección `RepositoryRef` (`Orm`, `Provider`,
`ConnectionString`).

### Capa Service y el motor de scripting

- `Service/Core` — clases base `KntService`/`IKntService` más `ServiceRef` (selección de repo/ORM, ver
  arriba).
- `Service/Interfaces` + `Service/Services` — un par interfaz/implementación por objeto de dominio (Note,
  Folder, KAttribute, NoteType, SystemValues, User).
- `Service/ServicesCommands` — clases de comando (`KntNoteCommands`, `KntFolderCommands`, etc.) construidas
  sobre `IPluginCommand`/`KntCommandServiceBase`. Exponen las operaciones de servicio a `KntScript`, el
  "lenguaje minimalista de automatización" mencionado en el README, invocado desde la consola de scripts de
  ClientWin (`KntScriptConsoleCtrl`). Si vas a cambiar qué puede hacer una acción de script sobre una
  nota/carpeta, esta es la capa a tocar — no `Service/Services` directamente.

### Client (Blazor) vs ClientWin (WinForms) — dos caminos de acceso a datos muy distintos

- **Client** es un frontend Blazor WASM puro: no tiene ninguna referencia a `Service`/`Repository`. Habla
  con los controladores REST de `Server` vía `HttpClient` (ver `Client/AppStoreService/ClientDataServices`).
- **ClientWin** habla con `Service`/`Repository` **en el mismo proceso**, dentro del mismo proceso del
  sistema operativo que la UI — no llama a la API HTTP de `Server` para los datos principales de
  notas/carpetas/usuarios. Trata a `Server` y `ClientWin` como dos consumidores independientes de la misma
  capa `Service`, no como cliente/servidor entre sí.

### Server

- `Server/Controllers` — API REST: `FoldersController`, `NotesController`, `KAttributesController`,
  `NoteTypesController`, `SystemValuesController`, `UsersController`, `ChatGPTController` (integración con
  OpenAI), además del scaffold `WeatherForecastController`.
- `Server/Hubs/ChatHub.cs` — hub de SignalR, mapeado en `/chathub`.
- `Server` también sirve la app `Client` Blazor compilada
  (`Microsoft.AspNetCore.Components.WebAssembly.Server`).

### Model

`Model/` contiene tipos compartidos transversales (`AppConfig`, `RepositoryRef`, `Result`/`ResultBase`,
`EntityModelBase`, métodos de extensión) y `Model/Dto/` contiene los DTOs de la API (`NoteDto`, `FolderDto`,
`UserDto`, `KAttributeDto`, etc.) compartidos entre las interfaces de `Repository` y la superficie de la API
de `Server`. Las entidades de EF Core son un concepto aparte, viven en `Repository.EntityFramework/Entities`,
no en `Model`.

## Notas para editar

- Al añadir una nueva capacidad de dominio, normalmente hay que tocar cuatro capas en cadena: `Model/Dto`
  → `Repository` (interfaz) → `Repository.Dapper` **y** `Repository.EntityFramework` (ambas
  implementaciones) → `Service/Interfaces` + `Service/Services` → `Server/Controllers` (si se expone vía
  API) y/o `Service/ServicesCommands` (si se expone a KntScript).
- Cadenas localizadas: `Docs/Manual.md` (inglés) y `Docs/Manual_es.md` (español) son el manual de usuario —
  actualiza ambos si cambia el comportamiento de cara al usuario, siguiendo la convención bilingüe ya
  existente en este repo.
