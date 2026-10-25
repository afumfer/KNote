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

- `KNote.slnx` — la app completa: `Server`, `Model`, `Service`, `KNote.Ai`, `KNote.Reports`, `Repository*`, `ClientWin`, `Client`,
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
  `Tests/WebApiIntegrationTests/*.cs` (`FoldersTests`, `KAttributesTests`, `NoteTypesTests`,
  `NotesTests`, `UsersTests`) son **tests de integración HTTP reales**, no tests unitarios.
  `Tests/Helpers/WebApiTestBase.cs` inicia sesión vía `POST {testsWebApiUrlBase}api/users/login` contra una
  instancia real de `Server` en ejecución **una sola vez por ejecución** y todos los tests reutilizan ese JWT: el
  `Server` limita los inicios de sesión por minuto y dirección (`RateLimits:SignInPerMinute`), y uno por test lo
  superaba (429). Si no puede iniciar sesión, cada test falla diciendo por qué (contraseña, `Server` parado, 429).
  Los datos que crean los tests tienen nombres fijos (`__TEST_..._###__`): cada clase que los crea los borra antes y
  después de cada test (`DeleteNamedAsync` en `[TestInitialize]`/`[TestCleanup]`), para que una ejecución que falle a
  medias no deje restos que impidan las siguientes, y ningún test depende de datos concretos de la BD salvo las
  búsquedas de `NotesTests` (una nota con "Hopper"). Configura
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
  │    ├─ KNote.Reports (carpeta Reports/) (informes imprimibles en HTML y exportación CSV de las notas, comunes a las dos apps)
  │    └─ ClientWin                   (→ también KNote.Ai, KNote.Reports, HtmlEditorControl, KntEditViewControl, KntIcons, KntScript)
  └─ Client                           (Blazor WASM; habla con Server por HTTP, no con Service/Repository)

Server → Client, Model, Service, KNote.Ai, KNote.Reports
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
el ORM que sirva las consultas después; en Sqlite también pone la base de datos en modo WAL, que queda guardado en
el fichero: en el modo por defecto las escrituras concurrentes del Server fallaban al momento con "database is
locked"):

- En `Server`: `Server/Program.cs` abre los repositorios configurados (`KntRepositories.OpenAsync`, vía la factoría)
  y `builder.Services.KntAddServices(repositories)` (`Server/Helpers/KntExtensions.cs`) registra en DI el `IKntService`
  de cada petición, el del repositorio de la sesión.
- En `ClientWin`: no hay contenedor de DI — `Service/Core/ServiceRef.cs` llama a la factoría de forma
  perezosa, y `ClientWin/Core/Store.cs` mantiene una lista de estos `ServiceRef` (cada app puede tener varias
  bases de datos de notas configuradas abiertas a la vez).

`RepositoryRef` (en `Model`) también incluye `Provider` (`Microsoft.Data.SqlClient` vs
`Microsoft.Data.Sqlite`) y `ConnectionString`, por lo que el motor de BD y el ORM son elecciones
independientes. Se configuran en `Server/appsettings.json` → sección `RepositoryRef` (`Orm`, `Provider`,
`ConnectionString`) o, con varios repositorios, la lista `Repositories`.

### Capa Service y el motor de scripting

- `Service/Core` — clases base `KntService`/`IKntService` más `ServiceRef` (selección de repo/ORM, ver
  arriba).
- `Service/Interfaces` + `Service/Services` — un par interfaz/implementación por objeto de dominio (Note,
  Folder, KAttribute, NoteType, SystemValues, User, AiSession, ChatRoom).
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
- **Identidad del repositorio**: dos filas de `SystemValues` (scope `SYSTEM`, claves en `KntConst`):
  `DB_REPOSITORYID`, un `Guid` propio de cada base de datos, y `DB_REPOSITORYNAME`, el nombre que ven los
  usuarios en las dos apps. Las añade `KntSchemaUpdater` si faltan (revisión 5 y en cada `EnsureUpToDate`, también
  en una base de datos recién creada; no van en `Seed()`, cuyo `HasData` se construye una vez por proceso) con el
  `RepositoryRef.Name` con que se abre o, sin él, `RepositoryRef.GetDefaultName()` (el fichero o la base de datos).
  Se leen sin sesión (`IKntSystemValuesService.GetRepositoryInfoAsync`, `[KntAllowAnonymous]`) y el nombre solo
  cambia con `SaveRepositoryNameAsync` (Admin); los comandos genéricos de `SystemValues` no tocan esas filas ni
  `DB_VERSION`. No se guardan en `KNoteData.config` (`RepositoryRef.Name`/`RepositoryId` son `[XmlIgnore]`);
  `ClientWin` usa `RepositoryId` como clave de un repositorio (ver `ClientWin/CLAUDE.md`). La compatibilidad con
  el antiguo alias (`<Alias>` de la configuración, `RepositoryRef:Alias` del `Server`) lleva el marcador
  `LEGACY(repository-alias)`.
- **Cuenta del usuario y contraseñas** (`KntUserCommands.cs`). `GetProfileAsync`/`SaveProfileAsync`/`ChangePasswordAsync`
  actúan siempre sobre el usuario actual (`UserIdentityName`), nunca sobre un id que llegue de fuera (Guest; un usuario
  deshabilitado se rechaza también donde no se comprueba el rol): el nombre de usuario no se cambia ahí y un email nuevo
  pide la contraseña actual. El nombre de usuario y el correo son únicos: es una regla del dominio que comprueban todos
  los comandos que guardan un usuario (`KntUserUniqueness`: `SaveAsync` del editor del Admin, `CreateAsync`/`RegisterAsync`,
  `SaveProfileAsync`) y que devuelve un `Result` no válido que nombra el valor repetido; los índices únicos de la BD
  siguen ahí como última defensa. Toda contraseña nueva (registro, alta, `SetPassword`, cambio, restablecimiento) pasa por
  `KntPasswordPolicy` (`Model`); el hash y su comprobación solo están en `Service/Core/KntPasswordHasher`, que da
  también la huella de la contraseña (`Stamp`, para los tokens del `Server`, `GetPasswordStampAsync`). "Olvidé mi
  contraseña": `CreatePasswordResetAsync`/`ResetPasswordAsync`, anónimos, con un token firmado con la clave de quien lo
  emite (`Service/Core/KntPasswordResetToken`: usuario, caducidad y huella; sin nada en la BD, sirve una vez porque la
  huella cambia). `KntServiceBase` escribe el `Param` de cada comando en el log de Trace: un parámetro con una
  contraseña o una clave es una clase con un `ToString()` que no la muestra (`KntSetPasswordParam`...), nunca una
  tupla. El envío de correo (`Service/Core/EmailSender`: `SmtpSettings`, `IEmailSender`) lo comparten `ClientWin` y
  `Server`. Tests en `Tests/ServiceTests/UserAccountServiceTests` y `UserPasswordResetServiceTests`.
- **Alarmas que vencen** (`IKntNoteService`, comandos en `KntNoteCommands.cs`): `GetAlarmNotesIdAsync` (ids de nota,
  `ClientWin`), `ConsumeDueAlarmsAsync` (las del usuario actual, Staff) y `ConsumeAllUsersDueAlarmsAsync` (de todos,
  Admin) devuelven los mensajes vencidos de un tipo **tal como estaban al vencer** y los reprograman o desactivan
  (`KMessageDto.ApplyAlarmControl`) con una actualización condicional (`IKntNoteRepository.ConsumeDueMessagesAsync`:
  solo si siguen activados y vencidos), así que cada alarma la recibe un solo consumidor aunque consulten a la vez
  `ClientWin`, el `Server` y varias pestañas de la Web; quien no pudo avisar (el SMTP caído) la guarda como estaba.
  `GetAlarmNoticesAsync` da los datos actuales (`AlarmNoticeDto`) de los avisos *Application info* del usuario de una
  lista de ids (`KMessageDto.IsAppInfoNoticeFor`). El correo de una alarma *Email* es común
  (`Service/Core/KntAlarmEmail`). Tests en `Tests/ServiceTests/AlarmNoticesServiceTests`. Usos: el panel de alarmas de
  `ClientWin`, la campana de la Web y el servicio de correo del `Server` (ver los `CLAUDE.md` de cada uno).
- **Ruta de una carpeta** (`IKntFolderService.GetPathAsync`, p. ej. `Home\Proyectos\KNote`): la usan
  `ClientWin` (`Store.GetKNoteFolerPath`) y la Web (`api/folders/{id}/path`).
- **HTML de las notas** (`Service/Core/KntHtmlSanitizer`, paquete HtmlSanitizer): las descripciones las escriben
  los usuarios, así que su HTML (tras Markdig) se sanea antes de mostrarse, con las mismas reglas en las dos
  apps: `IKntNoteService.UtilMarkdownToHtml`/`UtilSanitizeHtml` en `ClientWin` y `POST api/notes/html` en la Web.
  Tests en `Tests/KntHtmlSanitizerTests`.
- **Diagramas Mermaid de las notas**: Markdig ya escribe un bloque ` ```mermaid ` como `<pre class="mermaid">` con la
  definición como texto (escapada por el saneador; tests en `Tests/ServiceTests/NoteHtmlMarkdownTests`), y cada app lo
  dibuja en el navegador con `mermaid.min.js` + `kntMermaid.js` (común: tema según el color del texto, cola de
  dibujos, redibujo al cambiar el tema). **Una sola copia** en `Client/wwwroot/lib/mermaid/` (Mermaid 11.17.2, MIT,
  comprobado con la integridad del paquete npm); `ClientWin` la enlaza en su `.csproj` a `Resources\mermaid\`. Para
  subir de versión se sustituye `mermaid.min.js` (el de `dist/` del paquete npm, script clásico, sin `eval`) y su
  `LICENSE` ahí, y se prueban las dos apps. Detalles de cada app en `ClientWin/CLAUDE.md` y `Client/CLAUDE.md`.
- **Sesiones del asistente de IA** (`IKntService.AiSessions`, comandos en `KntAiSessionCommands.cs`, Staff).
  Cada conversación es una nota del tipo `@ChatSessions` (`KntConst.ChatSessionsTag`) en la carpeta
  `AI Assistant sessions`, con el proveedor y el modelo en los atributos `AiProvider`/`AiModel` del tipo y una
  `NoteTask` del usuario (fecha de inicio = creación de la sesión) que la vincula a él; las sesiones de un
  usuario se buscan con `NotesFilterDto.TaskUserId`. Todo pasa por los servicios de cada dominio (nunca por el
  repositorio). Tipo, atributos y carpeta se crean la primera vez con sus propios comandos, que piden
  Admin/ProjectManager: para que el asistente funcione con Staff, esa creación se hace dentro de un ámbito
  `KntAuthorizationBypass` (`Service/Core`, `internal`, `AsyncLocal`, con motivo en el log), que solo omite la
  comprobación de rol (no la validación, las reglas ni los eventos de los comandos) y solo mientras dura el
  `using`. Úsalo solo así: dentro de un comando ya autorizado y para operaciones concretas. Esa creación (y el
  usuario actual) la comparten las funciones que guardan sus datos en notas de un tipo del sistema:
  `Service/Core/KntSystemNotes` (`internal`), que usan las sesiones y los hilos del chat room. Esa creación se
  serializa por base de datos (`FindOrCreateAsync`: un `SemaphoreSlim` por cadena de conexión y una segunda búsqueda
  dentro): sin ello, varias peticiones a la vez en el primer uso creaban cada una el tipo y la carpeta (el tipo y los
  atributos fallaban por sus índices únicos y la carpeta se duplicaba, ver `Tests/ServiceTests/SystemNotesConcurrencyTests`).
  El candado es del proceso: si otro proceso crea antes el elemento, la creación fallida lo vuelve a buscar. Con
  varias carpetas del mismo nombre se usa la más antigua (menor `FolderNumber`). Solo el usuario de
  la sesión puede leerla o guardarla. La conversación se guarda en `Description`
  con `Model/Dto/AiChatSessionTranscript`: Markdown legible con un comentario HTML oculto por mensaje (el del
  asistente lleva el uso en JSON), del que se recuperan los turnos (`AiChatTurnDto`) exactamente; es el
  formato común de `ClientWin` y la Web, que se retoman las sesiones la una a la otra (con su proveedor y
  modelo si siguen configurados: `AiProviderSelection.ForSession`). `search_notes` deja fuera estas notas y los
  hilos del chat room (`NotesSearchDto.ExcludeNoteTypeIds`). Usos: `KNoteAIAssistantCtrl` (ver
  `ClientWin/CLAUDE.md`) y `AiAssistantController` + página `AIAssistant` (ver `Server/CLAUDE.md` y
  `Client/CLAUDE.md`).
- **Hilos del chat room** (`IKntService.ChatRooms`, comandos en `KntChatRoomCommands.cs`, Staff). Cada hilo es una
  nota del tipo `@KNoteChatRoom` (`KntConst.ChatRoomTag`) en la carpeta `KNote Chat Room`, creados la primera vez
  como los de las sesiones (`KntSystemNotes`). Los hilos son públicos para Staff+; quien crea un hilo, escribe en
  él o le envía una imagen tiene una `NoteTask` que lo hace participante (`IsParticipant`, `TaskUserId`). La
  conversación va en `Description` con `Model/Dto/ChatRoomTranscript` (Markdown con un comentario
  `<!-- knt-chat:message {...} -->` por mensaje: id, usuario, fecha), y el usuario de cada mensaje lo pone el
  servicio. Las imágenes son recursos de la nota (`AddImageAsync`: png/jpg/gif/webp, `KntConst.MaxLenResourceFile`)
  y se envían como un mensaje con su Markdown, con la ruta relativa a los recursos como las descripciones. El hilo
  `General` (`GetGeneralAsync`) es el de KntScript. **El servicio no ordena las escrituras**: añadir un mensaje lee
  y reescribe la nota, así que el que llama las serializa por hilo (el hub del Server, ver `Server/CLAUDE.md`).
  Los clientes son `KntChatCtrl` (ClientWin, a través del hub) y la página `ChatRoom` (Web); el contrato del hub
  está en `Model/Dto/ChatRoomHubContract`.

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
  `NoteTypesController`, `TraceNoteTypesController`, `SystemValuesController`, `UsersController`, `AiAssistantController` (asistente de IA:
  proveedores, chat con streaming SSE y sesiones; ver `Server/CLAUDE.md`), `ChatRoomController` (imágenes del chat
  room), `AlarmsController` (avisos de alarmas de la Web), `ReportsController` (informes imprimibles y CSV de las listas de notas, con `KNote.Reports`), además del scaffold
  `WeatherForecastController`. Capa fina sobre `IKntService`; responden siempre
  con un `Result<T>` y autorizan con `[Authorize(Roles = ...)]`.
- `Server/Helpers/KntAlarms.cs` — `EmailAlarmsService`, que envía las alarmas de correo de todos los repositorios
  (apagado por defecto, sección `Alarms`). Ver `Server/CLAUDE.md`.
- `Server/Hubs/ChatHub.cs` — hub de SignalR del chat room, mapeado en `/chathub`: exige el JWT del usuario (Staff+),
  un grupo por hilo y `ChatRoomManager` (presencia y escrituras en orden por hilo). Ver `Server/CLAUDE.md`.
- **Varios repositorios, uno por sesión** (`Server/Helpers/KntRepositories`, configuración `Repositories` o el
  `RepositoryRef` de siempre): el usuario elige repositorio al iniciar sesión (donde valen sus credenciales), el token
  lo lleva (claim `knt_repo`, el `RepositoryId`) y el `IKntService` de cada petición es el de ese repositorio; sin
  sesión, el predeterminado. Los controladores y las páginas no cambian. Ver "Repositorios" en `Server/CLAUDE.md`.
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
