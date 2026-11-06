# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visión general del proyecto

KaNote ("KNote") es un gestor de notas/tareas escrito en C#/.NET 10. Tiene dos frontends que comparten la
misma lógica de backend: una app de escritorio WinForms (`ClientWin`) y una app Blazor WebAssembly
(`Client`) servida por una API ASP.NET Core (`Server`). Soporta dos motores de BD (Sqlite, SQL Server)
detrás de dos implementaciones de repositorio intercambiables (Dapper, Entity Framework Core).

La app Web se despliega de dos formas: en un servidor (IIS, un repositorio o varios) o como **KNote Local**, el mismo
`Server` y la misma app en `127.0.0.1` del equipo del usuario (Windows, macOS, Linux), arrancado por `KNote.Launcher`.
Las diferencias solo están en la configuración (ver "KNote Local" en `Server/CLAUDE.md` y `Launcher/CLAUDE.md`).

## Archivos de solución

No hay un único `.sln` en la raíz; hay varios `.slnx` ("VS solution XML"), cada uno cubriendo una parte
distinta del código:

- `KNote.slnx` — la app completa: `Server`, `KNote.Launcher`, `Model`, `Service`, `KNote.Ai`, `KNote.Reports`, `Repository*`, `ClientWin`, `Client`,
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

# KNote Local: el lanzador (arranca la compilación del Server en el entorno Local; compila los dos antes)
dotnet run --project Launcher/KNote.Launcher.csproj -- --data <carpeta de pruebas>
# paquete e instaladores de KNote Local (en VS: botón derecho en KNote.Launcher > Publish > LocalServerPackage;
# deja el paquete y su zip en DeployLocalServer/out, ver DeployLocalServer/README.md)
dotnet publish Launcher/KNote.Launcher.csproj -p:PublishProfile=LocalServerPackage

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
  secretos `f25fed7b-9b03-406c-8e4a-b98eb14f5579`) y, si el usuario vale en varios repositorios del `Server`,
  `testsRepositoryId` con el de pruebas (los tests nunca eligen uno: sin él, el error lista los disponibles con su
  id) — **debe haber una instancia de `Server` en ejecución y
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
  │    ├─ KNote.Ai (carpeta Ai/)      (IChatClient e IEmbeddingGenerator por proveedor de IA, turno con streaming + tools sobre IKntService; todos los paquetes NuGet de IA)
  │    ├─ KNote.Reports (carpeta Reports/) (informes imprimibles en HTML y exportación CSV de las notas, comunes a las dos apps)
  │    └─ ClientWin                   (→ también KNote.Ai, KNote.Reports, HtmlEditorControl, KntEditViewControl, KntIcons, KntScript)
  └─ Client                           (Blazor WASM; habla con Server por HTTP, no con Service/Repository)

Server → Client, Model, Service, KNote.Ai, KNote.Reports
KntEditViewControl → HtmlEditorControl, KntIcons
HtmlEditorControl → KntIcons
```

`KntScript`, `KntIcons` y `KNote.Launcher` (carpeta `Launcher/`, el lanzador de KNote Local, ver `Launcher/CLAUDE.md`) no
tienen referencias a otros proyectos (son hojas). `KntIcons` dibuja los iconos de
la UI WinForms (`ClientWin`, `HtmlEditorControl`, `KntEditViewControl`) a partir de una fuente vectorial para
que se vean nítidos con cualquier escalado de Windows; ver `KntIcons/CLAUDE.md`.

`KNote.Ai` es lo común del asistente de IA: `AiChatClientFactory.Create(AiProviderRef, tools)` construye el
`IChatClient` de `Microsoft.Extensions.AI` para OpenAI (Responses API), Anthropic u Ollama, y `KNoteAiTools`
expone a los modelos sus herramientas sobre la capa `Service`: `search_notes`, `get_note_details` y `create_task`, y las
de solo lectura de KNote Atlas (`KNoteAiQueryTools.cs`, ver «Herramientas de consulta del asistente» más abajo). Lo que
`create_task` hace distinto en cada aplicación (dónde guarda la nota y cómo se la muestra al usuario) lo
aporta un `IKNoteAiToolsHost` (el de `ClientWin` es `Core/KNoteAiToolsHost`). Para KNote Atlas (ver «Índice
semántico» más abajo) da también los modelos de embeddings: `AiEmbeddingGeneratorFactory` (Ollama y OpenAI;
Anthropic no tiene) y `AiEmbeddingGenerators`, el resolvedor por nombre de modelo (`IKntEmbeddingGenerators` de
`Service/Core`) común a los dos hosts, construido sobre una función que da la lista de proveedores; y
`AiProviderTester.TestAsync`, el botón «Test» de los proveedores de las dos apps (chat: una petición mínima;
embeddings: un vector, sus dimensiones y el tiempo). Un proveedor es de chat o de embeddings por
`AiProviderRef.Kind` (`EnumAiProviderKind`; los ficheros anteriores cargan como `Chat`) y todas las listas de modelos
de chat solo ven los de tipo `Chat`; `ContextWindow` es el `num_ctx` del chat de Ollama. Las versiones de los paquetes de
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
- **Calendario de alarmas y panel de tareas de la Web** (`IKntNoteService.GetAlarmsCalendarAsync` y
  `GetTasksPanelAsync`, Guest, solo lectura). Las reglas son lógica pura de `Model/Core`, comunes al `Server` y a la Web,
  con sus tests: `AlarmOccurrences.Expand` (las fechas en que salta una alarma en un rango: la periódica desde la creación
  de su nota, con los mismos pasos que `ApplyAlarmControl`, también la deriva de las mensuales; desactivada o *Once*,
  solo en su `AlarmDateTime`), `TasksWindow` (la ventana de N días, semanas de lunes a domingo o meses alrededor de una
  fecha, centrada, con la unidad sobrante al pasado, y la fecha de corte D = min(hoy, fin)) y `TaskPanelRules` (una tarea
  está resuelta si y solo si tiene `EndDate`; sin `StartDate` empieza en su creación; solape con la ventana, columna en D,
  indicadores e inconsistencias). Los repositorios hacen la versión SQL de `Overlaps`/`Classify`
  (`IKntNoteRepository.GetTasksPanelCountsAsync`/`PageAsync`/`NearestAsync`, sin `GREATEST` ni `COALESCE` en el solape
  para usar los índices de `NoteTasks.StartDate`/`EndDate`, revisión 6 del esquema): **si cambia una regla, cambian las
  dos** (`TasksPanelServiceTests` compara cada tarea devuelta con `TaskPanelRules`). Las tareas de las notas del sistema
  (`@ChatSessions`, `@KNoteChatRoom`) nunca salen en el panel. Tests en `Tests/AlarmOccurrencesTests`,
  `TasksWindowTests`, `TaskPanelRulesTests` y `Tests/ServiceTests/AlarmsCalendarServiceTests`/`TasksPanelServiceTests`.
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
  modelo si siguen configurados: `AiProviderSelection.ForSession`). Las herramientas del asistente dejan fuera estas
  notas y las demás del sistema. Usos: `KNoteAIAssistantCtrl` (ver
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
- **Índice semántico y búsqueda por significado (KNote Atlas, etapa 0)** (`IKntService.SemanticSearch`, comandos en
  `KntSemanticSearchCommands.cs` y `KntSemanticSearchEvaluationCommands.cs`). La especificación y sus decisiones están
  fuera del repositorio, en `C:\Dev\KNote-Atlas` (`03-Etapa-0.md`).
  - **Modelos del repositorio** (`RepositoryAiModelsDto`: modelo de embeddings, dimensiones, instrucción de consulta y
    LLM recomendado), en `SystemValues` ámbito `AI` (`IKntSystemValuesService.Get/SaveRepositoryAiModelsAsync`, Guest /
    Admin). Esas claves están protegidas: los comandos genéricos de `SystemValues` no las tocan. Dónde corre cada
    modelo lo dice cada instalación (sus proveedores de embeddings, ver `KNote.Ai`). `Service` no referencia
    `KNote.Ai`: recibe el resolvedor en `IKntService.EmbeddingGenerators` (nulo = sin embeddings), que fija un único
    punto de cada host (`Store.AddServiceRef` en `ClientWin`, la fábrica por petición de `KntAddServices` y los
    trabajos de índice en `Server`).
  - **Fragmentos** (tabla `NoteChunks`, revisiones 7 y 8 del esquema; `IKntSemanticIndexRepository` en Dapper y EF, con
    paridad en `Tests/RepositoryParityTests`): `NoteChunkBuilder` (`Model/Core`, regla pura) parte cada nota en un
    fragmento de datos (tema, tipo, carpeta, etiquetas, atributos, recursos), los de su texto por apartados de Markdown
    (limpio con `NoteTextCleaner`, sin solapamiento), uno por tarea y uno por recurso con descripción. Cada fila lleva su
    vector (`EmbeddingVector`, float32), su `ContentHash` (un vector se reutiliza si el texto no cambia) y la clave del
    índice en `Model` (`RepositoryAiModelsDto.GetIndexKey()`: el modelo, o `modelo@dimensiones`), así que conviven los
    índices de varios modelos (la evaluación los compara; un Admin borra los que no son el actual).
  - **Indexado** (`Service/Core/KntSemanticIndexer`, independiente del host, con progreso y cancelación entre notas; lo
    hecho se conserva): *crear / actualizar* procesa las notas pendientes, *rehacer* recalcula todos los vectores
    mientras la búsqueda sigue con los anteriores, ambos Admin; *poner al día* (antes de cada búsqueda: 50 notas o unos
    10 s, nada si hay otra ejecución en marcha) es Guest. Los vectores de una nota se piden en lotes de 32 fragmentos
    (`EmbeddingBatchSize`: una nota de cientos en una llamada superaba el tiempo máximo del proveedor) y sus filas se
    sustituyen una vez al final; las puestas al día rápidas (antes de una búsqueda y de las parecidas) dejan para la
    indexación completa las notas con más de 64 fragmentos por calcular (`QuickRefreshMaxChunks`). Estado en `AI_INDEX_STATE` (`KntSemanticIndexState`). El
    índice contiene **todas** las notas salvo las de los tipos del sistema (`KntSystemNotes.SystemNoteTypeNames`:
    sesiones, chat room, consultas de prueba, informes): los permisos se aplican al buscar. Los comandos que cambian una
    nota (guardar, carpeta, etiquetas, recursos, tareas) y los que cambian su fragmento de datos sin tocarla (renombrar
    o mover una carpeta, renombrar un tipo o un atributo) llaman a `MarkStaleAsync`; un fallo al marcar no hace fallar
    el guardado. **Un comando nuevo que cambie lo que se indexa de una nota tiene que marcarla.**
  - **Búsqueda** (`SearchAsync`): **Palabras y Significado son excluyentes**; la de palabras no cambia (código,
    paginación y API). Por significado, sin paginar: las 100 notas más parecidas desde 0,45, de más a menos parecida
    (`SemanticSearchRules`, `Model/Core`), entre todas o entre las del filtro (`NotesFilterDto.MeaningText`, «Talks
    about…»), cruzadas con las que da el filtro de siempre leído por el servicio. Los vectores se leen de
    `KntSemanticIndexCache` (uno por base de datos y clave del índice en el proceso, con delta por marca de agua y 5
    minutos de margen); `KntSemanticSearcher` es lo común de la búsqueda y de las parecidas. El resultado
    (`NoteSearchHitDto : NoteMinimalDto`, con parecido y fragmento) lo muestran las listas de siempre; `NoteListSorter`
    ordena en memoria como el repositorio. Sin modelo, sin su generador o sin índice, un error claro: una búsqueda no
    crea el índice.
  - **Notas parecidas** (`GetSimilarNotesAsync`): `SemanticSearchRules.RankSimilar` (media de los mejores parecidos de
    cada fragmento), desde 0,60, 10 como mucho, sin las ya relacionadas; «Possible duplicate» desde 0,95. Al vuelo, no se
    guarda nada; solo esa nota se pone al día.
  - **Evaluación de la búsqueda** (ProjectManager, solo en la Web): la nota del sistema «Search test queries»
    (`@KNoteTestQueries`, tabla `SearchTestQueries`) en la carpeta «KNote Atlas», una consulta por petición
    (`EvaluateQueryAsync`: palabras y significado en cada índice, con y sin la instrucción; por cada una, posición,
    esperadas entre las 10 primeras, el parecido de cada resultado y el tiempo) y el informe (`SearchEvaluationReport`,
    con el parecido de la primera esperada y del resultado 10 y la tabla «si el corte fuera…», `Cutoffs`), que se guarda como nota `@KNoteSearchEvaluation` (`KntSearchEvaluationNotes`).
  - Tests: reglas puras en `Tests/*` (`NoteChunkBuilderTests`, `SemanticSearchRulesTests`...), servicio en
    `Tests/ServiceTests/Semantic*`, `SimilarNotesServiceTests` y `SearchEvaluationServiceTests` con un generador falso y
    determinista (`Tests/Helpers/FakeEmbeddingGenerators`, 1024 dimensiones: con menos, textos sin relación salían
    parecidos), y la API en `Tests/InProcessIntegrationTests/Semantic*` y `SearchByMeaningInProcessTests`.
- **Contexto de la IA (KNote Atlas, etapa 1)** (`IKntService.AtlasContext`, comandos en `KntAtlasContextCommands.cs`;
  especificación en `C:\Dev\KNote-Atlas\03-Etapa-1.md`). Tres notas del sistema en la carpeta «KNote Atlas»
  (`Service/Core/KntAtlasNotes`, creadas con `KntSystemNotes`): el perfil (`@KNoteProfile`) y el glosario
  (`@KNoteGlossary`, tabla `Term | Meaning | Also written as`), uno de cada uno (con varios, el más antiguo), y las
  instrucciones de cada tarea (`@KNoteTaskPrompt`, atributos `Task`, `Mode` = `Add`/`Replace` y `Active`, una activa por
  tarea). Las ventanas (Guest) no crean nada; crear las notas es de ProjectManager. Lo que recibe la IA lo monta
  `Model/Core/AiPromptComposer` en capas (reglas fijas de `AiTaskCatalog` con el usuario, su rol, el repositorio y la
  fecha → instrucciones de la tarea, de serie más o en lugar de la nota → perfil recortado a 2.000 palabras → términos
  del glosario de la conversación), sin los comentarios HTML (las ayudas de las plantillas, `AtlasTemplates`) ni los
  apartados vacíos (`AtlasMarkdown.Clean`). Los términos (`GlossaryTable.Find`: sin mayúsculas ni acentos, por palabra
  completa y por sus variantes) se ordenan por su primera aparición en la conversación: un término nuevo solo se añade
  al final, para no romper la caché del proveedor. `ComposeAsync` es Staff; `KNote.Ai/AiAssistantContext` lo usa en el
  asistente de las dos apps y, si falla, monta solo las reglas y las instrucciones de serie. **Regla de guardado**: las
  notas del perfil, el glosario, las instrucciones y las consultas de prueba (`KntAtlasNotes.ProtectedNoteTypeNames`)
  solo las guardan, borran, mueven o reetiquetan ProjectManager y Admin, también con la autorización de comandos
  apagada (el `Server`): lo comprueban `KntNotesSaveAsyncCommand` (tipo nuevo y anterior), `Delete*` y los `Patch*`;
  sin usuario, permitido. Los tres tipos son del sistema (`KntSystemNotes.SystemNoteTypeNames`). Tests en
  `Tests/AtlasContextRulesTests`, `Tests/ServiceTests/AtlasContextServiceTests` y
  `Tests/InProcessIntegrationTests/AtlasInProcessTests`.

- **Herramientas de consulta del asistente (KNote Atlas, etapa 1)** (`Ai/KNoteAiQueryTools.cs`, parte de `KNoteAiTools`):
  `find_notes_about`, `filter_notes`, `get_folder_tree`, `get_note_items`, `get_tasks` y `lookup_glossary`, además de
  `search_notes` y `get_note_details`. La lista se calcula **en cada turno** (`GetToolsAsync`: `find_notes_about` solo con
  índice utilizable y `lookup_glossary` solo con términos; `AiChatTurnStreamer.StreamAsync` recibe las del turno). El
  modelo escribe nombres: `AiFolders` y los `Find*` los traducen (sin mayúsculas ni acentos) y, si faltan o podrían ser
  varios, devuelven las opciones. Las notas del sistema (`KntConst.IsSystemNoteType`, `SystemFolderNames`) quedan fuera de
  todas; `filter_notes` lo hace con `NotesFilterDto.ExcludeNoteTypeIds`, en la consulta, para que la paginación y el total
  sean correctos. `get_note_details` corta la descripción en `MaxNoteTextLength` con un aviso y añade los términos del
  glosario de la nota (`IKntAtlasContextService.FindGlossaryTermsAsync`). `get_tasks` usa `TasksPanelFilterDto.WindowFrom/To`,
  una ventana exacta. `get_note_items` no da el texto de las alarmas *Execute KntScript*. Tests con repositorio real en
  `Tests/ServiceTests/AiQueryToolsTests`, llamando a las herramientas por nombre como el modelo.
- **Citas `#123` como enlaces** (`Model/Core/NoteCitationLinker`, sobre el HTML ya saneado, sin consultar si la nota
  existe): solo en texto (no en etiquetas, enlaces ni código; no `#urgente`, `C#123`, `#0` ni entidades `&#123;`). Abrir una
  nota por número pasa siempre por `IKntNoteService.GetCitedNoteAsync` (`api/notes/cited/{n}`), que no da las notas del
  sistema: los números son consecutivos y probarlos no debe llevar a las conversaciones de IA de otros. Web: ver
  `Client/CLAUDE.md`; `ClientWin`: ver `ClientWin/CLAUDE.md`.

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
- **KNote Local** (`Server/Local`, `appsettings.Local.json`): activo solo si hay carpeta de datos del usuario
  (`Local__DataPath` o el entorno `Local`), con su configuración (`knote-local.json`, secretos cifrados con Data
  Protection), `/health`, parada con token y la API del equipo (`api/local`: repositorios, IA, correo, puerto). Ver
  "KNote Local" en `Server/CLAUDE.md`.
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
