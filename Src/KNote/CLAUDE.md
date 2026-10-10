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

- `KNote.slnx` — la app completa: `Server`, `Model`, `Service`, `KNote.Ai`, `Repository*`, `ClientWin`, `Client`,
  `KntScript`, `MessageBroker*`, `HtmlEditorControl`, `KntEditViewControl`, `KntIcons`. Úsalo para la mayoría del
  trabajo.
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

# Tests unitarios de ClientWin (parte de KNote.slnx, no necesitan Server ni BD real)
dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj
# excluyendo los smoke tests de proveedores de IA reales (ver ClientWin.Tests/CLAUDE.md):
dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj --filter "TestCategory!=RequiresRealAiProvider"
```

No hay ningún workflow de CI configurado (`.github/workflows/` está vacío) ni linter más allá de la única
regla del `.editorconfig` (`csharp_prefer_braces = false:silent`) — los cuerpos de `if` en una línea sin
llaves están permitidos y son habituales en este código.

## Tests

Hay dos suites de test independientes, en dos `.slnx` distintos, con propósitos distintos:

- **`Tests/` (vía `KNoteTest.slnx`)** — backend (`Server`/`Model`/`Service`/`Repository*`).
  `Tests/WebApiIntegrationTests/*.cs` (`ChatGPTTests`, `FoldersTests`, `KAttributesTests`, `NoteTypesTests`,
  `NotesTests`, `UsersTests`) son **tests de integración HTTP reales**, no tests unitarios.
  `Tests/Helpers/WebApiTestBase.cs` inicia sesión vía `POST {testsWebApiUrlBase}api/users/login` contra una
  instancia real de `Server` en ejecución y reutiliza el JWT en las siguientes peticiones. Configura
  `testsUserName`, `testsUserPwd`, `testsWebApiUrlBase` en `Tests/appsettings.json` o en user-secrets (id de
  secretos `f25fed7b-9b03-406c-8e4a-b98eb14f5579`) — **debe haber una instancia de `Server` en ejecución y
  accesible en esa URL antes de correr el proyecto de tests.** `Tests/InProcessIntegrationTests/*.cs` es la
  variante que no necesita un `Server` externo (usa `WebApplicationFactory` en el mismo proceso, ver
  `Tests/Helpers/KNoteWebApplicationFactory.cs`).

- **`ClientWin.Tests/` (parte de `KNote.slnx`)** — `ClientWin` (MSTest, fakes hechos a mano en `Fakes/`, no
  frameworks de mocking; ver `ClientWin.Tests/CLAUDE.md` para la convención y para las guías de
  configuración de la suite `RequiresRealAiProvider`, que hace llamadas reales a OpenAI/Anthropic/Ollama
  para detectar roturas tras actualizar los paquetes NuGet de IA — no corre por defecto.

## Arquitectura

### Grafo de dependencias entre proyectos

```
Model  (hoja: DTOs en Model/Dto, tipos compartidos, RepositoryRef/AppUserSettings/AppUserState — sin referencias a otros proyectos)
  ├─ Repository                       (solo interfaces: IKntNoteRepository, IKntFolderRepository, ...)
  │    ├─ Repository.Dapper           (implementación con Dapper de las mismas interfaces)
  │    └─ Repository.EntityFramework  (implementación con EF Core + KntDbContext + Entities/)
  ├─ MessageBroker
  │    └─ MessageBroker.RabbitMQ
  ├─ Service                          (→ Repository, Repository.Dapper, Repository.EntityFramework, MessageBroker*)
  │    ├─ KNote.Ai (carpeta Ai/)      (IChatClient por proveedor de IA, turno con streaming + tools sobre IKntService; todos los paquetes NuGet de IA)
  │    └─ ClientWin                   (→ también KNote.Ai, HtmlEditorControl, KntEditViewControl, KntIcons, KntScript)
  └─ Client                           (Blazor WASM; habla con Server por HTTP, no con Service/Repository)

Server → Client, Model, Service, KNote.Ai
KntEditViewControl → HtmlEditorControl, KntIcons
HtmlEditorControl → KntIcons
```

`KntScript` y `KntIcons` no tienen referencias a otros proyectos (son hojas). `KntIcons` dibuja los iconos de
la UI WinForms (`ClientWin`, `HtmlEditorControl`, `KntEditViewControl`) a partir de una fuente vectorial para
que se vean nítidos con cualquier escalado de Windows; ver `KntIcons/CLAUDE.md`.

`KNote.Ai` es lo común del asistente de IA: `AiChatClientFactory.Create(AiProviderRef, tools)` construye el
`IChatClient` de `Microsoft.Extensions.AI` para OpenAI (Responses API), Anthropic u Ollama, y `KNoteAiTools`
expone a los modelos `search_notes`, `get_note_details` y `create_task` sobre la capa `Service`. Lo que
`create_task` hace distinto en cada aplicación (dónde guarda la nota y cómo se la muestra al usuario) lo
aporta un `IKNoteAiToolsHost` (el de `ClientWin` es `Core/KNoteAiToolsHost`). Las versiones de los paquetes de
IA se suben solo aquí; tras subirlas, pasa los smoke tests `RequiresRealAiProvider` (ver
`ClientWin.Tests/CLAUDE.md`).

### Patrón Repository (ORM intercambiable)

`Repository/` define solo interfaces (`IKntRepository`, `IKntNoteRepository`, `IKntFolderRepository`,
`IKntKAttributeRepository`, `IKntNoteTypeRepository`, `IKntSystemValuesRepository`, `IKntUserRepository`).
`Repository.Dapper` y `Repository.EntityFramework` son dos implementaciones independientes de esas mismas
interfaces. Cuál está activa se decide en tiempo de ejecución mediante `RepositoryRef.Orm` ("Dapper" o
"EntityFramework") en la configuración:

La bifurcación vive en un único punto, `Service/Core/KntRepositoryFactory.Create(repositoryRef)`, que antes
pasa por `KntSchemaUpdater.EnsureUpToDate` de EF (EF es el único que crea/actualiza el esquema, sea cual sea
el ORM que sirva las consultas después):

- En `Server`: `Server/Program.cs` llama a `builder.Services.KntAddServices(appSettings, repositoryRef)`
  (`Server/Helpers/KntExtensions.cs`), que registra `IKntRepository` (vía la factoría) e `IKntService` en DI.
- En `ClientWin`: no hay contenedor de DI — `Service/Core/ServiceRef.cs` llama a la factoría de forma
  perezosa, y `ClientWin/Core/Store.cs` mantiene una lista de estos `ServiceRef` (cada app puede tener varias
  bases de datos de notas configuradas abiertas a la vez).

`RepositoryRef` (en `Model`) también incluye `Provider` (`Microsoft.Data.SqlClient` vs
`Microsoft.Data.Sqlite`) y `ConnectionString`, por lo que el motor de BD y el ORM son elecciones
independientes. Se configuran en `Server/appsettings.json` → sección `RepositoryRef` (`Orm`, `Provider`,
`ConnectionString`).

### Capa Service y el motor de scripting

- `Service/Core` — clases base `KntService`/`IKntService` más `ServiceRef` (selección de repo/ORM, ver
  arriba).
- `Service/Interfaces` + `Service/Services` — un par interfaz/implementación por objeto de dominio (Note,
  Folder, KAttribute, NoteType, SystemValues, User, AiSession).
- `Service/ServicesCommands` — clases de comando (`KntNoteCommands`, `KntFolderCommands`, etc.) construidas
  sobre `IPluginCommand`/`KntCommandServiceBase`. Exponen las operaciones de servicio a `KntScript`, el
  "lenguaje minimalista de automatización" mencionado en el README, invocado desde la consola de scripts de
  ClientWin (`KntScriptConsoleCtrl`). Si vas a cambiar qué puede hacer una acción de script sobre una
  nota/carpeta, esta es la capa a tocar — no `Service/Services` directamente.
- **Autorización por comando.** Cada comando declara su rol mínimo con `[KntAuthorize(rol)]` o
  `[KntAllowAnonymous]` (`Model/KntAuthorization.cs`; roles jerárquicos `EnumRoles` `Guest < Staff <
  ProjectManager < Admin`, leídos de `User.RoleDefinition` con `KntRoles`). `KntServiceBase.ExecuteCommand`
  lo comprueba (`KntCommandServiceBase.ValidateAuthorizationAsync`) contra el rol de `UserIdentityName` en ese
  repositorio (`IKntService.GetCurrentUserAsync`/`GetCurrentUserRoleAsync`, leído del repositorio y cacheado;
  `ResetCurrentUser` lo olvida), **solo si `IKntService.EnforceAuthorization` está activo**: lo activa
  `ServiceRef` (ClientWin); `Server` lo deja apagado y sigue con sus `[Authorize]`, igual que los tests de
  servicio que no van de seguridad. Un comando sin atributo se deniega; uno denegado devuelve un `Result` con
  `NotAuthorized = true`. Un comando nuevo necesita su atributo y su fila en
  `Tests/ServiceTests/CommandAuthorizationMatrixTests`. El registro de usuarios (`Users.RegisterAsync`)
  decide el rol del usuario nuevo (Admin mientras haya como mucho un Admin, Guest después); `CreateAsync` es
  el alta que hace un Admin, con el rol que elija. ClientWin añade encima la autorización de sus casos de
  uso (ver "Autenticación y autorización" en `ClientWin/CLAUDE.md`).
- **Sesiones del asistente de IA** (`IKntService.AiSessions`, comandos en `KntAiSessionCommands.cs`, Staff).
  Cada conversación es una nota del tipo `@ChatSessions` (`KntConst.ChatSessionsTag`) en la carpeta
  `AI Assistant sessions`, con el proveedor y el modelo en los atributos `AiProvider`/`AiModel` del tipo y una
  `NoteTask` del usuario (fecha de inicio = creación de la sesión) que la vincula a él; las sesiones de un
  usuario se buscan con `NotesFilterDto.TaskUserId`. Todo pasa por los servicios de cada dominio (nunca por el
  repositorio). Tipo, atributos y carpeta se crean la primera vez con sus propios comandos, que piden
  Admin/ProjectManager: para que el asistente funcione con Staff, esa creación se hace dentro de un ámbito
  `KntAuthorizationBypass` (`Service/Core`, `internal`, `AsyncLocal`, con motivo en el log), que solo omite la
  comprobación de rol (no la validación, las reglas ni los eventos de los comandos) y solo mientras dura el
  `using`. Úsalo solo así: dentro de un comando ya autorizado y para operaciones concretas. Solo el usuario de
  la sesión puede leerla o guardarla. La conversación se guarda en `Description`
  con `Model/Dto/AiChatSessionTranscript`: Markdown legible con un comentario HTML oculto por mensaje (el del
  asistente lleva el uso en JSON), del que se recuperan los turnos (`AiChatTurnDto`) exactamente; es el
  formato común de `ClientWin` y la Web. `search_notes` deja fuera estas notas
  (`NotesSearchDto.ExcludeNoteTypeId`).

### Client (Blazor) vs ClientWin (WinForms) — dos caminos de acceso a datos muy distintos

- **Client** es un frontend Blazor WASM puro: no tiene ninguna referencia a `Service`/`Repository`. Habla
  con los controladores REST de `Server` vía `HttpClient` (ver `Client/AppStoreService/ClientDataServices`).
- **ClientWin** habla con `Service`/`Repository` **en el mismo proceso**, dentro del mismo proceso del
  sistema operativo que la UI — no llama a la API HTTP de `Server` para los datos principales de
  notas/carpetas/usuarios. Trata a `Server` y `ClientWin` como dos consumidores independientes de la misma
  capa `Service`, no como cliente/servidor entre sí.

Ambas UIs siguen el mismo diseño: la UI no contiene lógica de negocio, que está en `Service` y en los
SmartDTO de `Model/Dto` (compartidos). `Client` reproduce la estructura de `ClientWin` (un `Store` con el
estado global y el acceso a datos, páginas que hacen de casos de uso, componentes de presentación) y su UI se
construye con la librería de componentes **Radzen.Blazor** (<https://blazor.radzen.com/>). Detalles en
`Client/CLAUDE.md`.

### Server

- `Server/Controllers` — API REST: `FoldersController`, `NotesController`, `KAttributesController`,
  `NoteTypesController`, `SystemValuesController`, `UsersController`, `AiAssistantController` (asistente de IA:
  proveedores, chat con streaming SSE y sesiones; ver `Server/CLAUDE.md`), `ChatGPTController` (obsoleto, se
  retira con la página "ChatGPT room"), además del scaffold `WeatherForecastController`. Capa fina sobre `IKntService`; responden siempre
  con un `Result<T>` y autorizan con `[Authorize(Roles = ...)]`.
- `Server/Hubs/ChatHub.cs` — hub de SignalR, mapeado en `/chathub`.
- `Server` también sirve la app `Client` Blazor compilada
  (`Microsoft.AspNetCore.Components.WebAssembly.Server`), todo bajo la base `/KNote`.
- Plantilla de controlador, JWT, configuración y tests: `Server/CLAUDE.md`.

### Model

`Model/` contiene tipos compartidos transversales (`AppUserSettings`/`AppUserState`, `RepositoryRef`, `Result`/`ResultBase`,
`EntityModelBase`, métodos de extensión) y `Model/Dto/` contiene los DTOs de la API (`NoteDto`, `FolderDto`,
`UserDto`, `KAttributeDto`, etc.) compartidos entre las interfaces de `Repository` y la superficie de la API
de `Server`. Las entidades de EF Core son un concepto aparte, viven en `Repository.EntityFramework/Entities`,
no en `Model`.

## Notas para editar

- Al añadir una nueva capacidad de dominio, normalmente hay que tocar cuatro capas en cadena: `Model/Dto`
  → `Repository` (interfaz) → `Repository.Dapper` **y** `Repository.EntityFramework` (ambas
  implementaciones) → `Service/Interfaces` + `Service/Services` → `Server/Controllers` (si se expone vía
  API) y/o `Service/ServicesCommands` (si se expone a KntScript).
- Configuración de `ClientWin`: dos ficheros (`KNoteData.config` = ajustes del usuario, con secretos cifrados con
  DPAPI; `KNoteState.config` = estado recordado por la app), modelo en `Model/Config` (`AppUserSettings`,
  `AppUserState`) y E/S + cifrado en `ClientWin/Core`. Antes de tocarlo lee "Configuración persistida" en
  `ClientWin/CLAUDE.md`: el formato antiguo sigue soportado por migración y su lector (`AppConfigV1`) no debe
  cambiarse.
- Cadenas localizadas: `Docs/Manual.md` (inglés) y `Docs/Manual_es.md` (español) son el manual de usuario —
  actualiza ambos si cambia el comportamiento de cara al usuario, siguiendo la convención bilingüe ya
  existente en este repo.
