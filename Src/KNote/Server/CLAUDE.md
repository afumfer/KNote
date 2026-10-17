# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this project
(`Server`, el backend ASP.NET Core de la versión Web de KNote). Ver también el `CLAUDE.md` de la raíz del repo
para el contexto general (patrón Repository, capa Service, autorización por comando) y `Client/CLAUDE.md`
para el frontend Blazor que consume esta API.

## Papel de `Server`

Un único proceso ASP.NET Core que:

- expone la **API REST** (`Controllers/`, rutas `api/[controller]`) que usa `Client`;
- expone el **hub SignalR** del chat room (`Hubs/ChatHub`, en `/chathub`; ver «Chat room»);
- **hospeda la app Blazor WebAssembly** `Client` (`UseBlazorFrameworkFiles` + `MapFallbackToFile("index.html")`),
  todo bajo la base `/KNote` (`app.UsePathBase("/KNote")`, a juego con `<base href="/KNote/">` de `Client`).

`Server` es, igual que `ClientWin`, **un consumidor de la capa `Service`**: la lógica de negocio está en
`Service` (comandos de `Service/ServicesCommands`) y en los SmartDTO de `Model/Dto`, compartidos por ambos
desarrollos. **Los controladores son una capa fina**: traducen HTTP ↔ llamada a `IKntService` ↔ `Result<T>`,
sin reglas de negocio. Si algo debe comportarse igual en la Web y en el escritorio, va en `Service`, no aquí.

```
Client (I*WebApiService) ──HTTP/JWT──► XxxController ──► IKntService.Xxx ──► comando ──► IKntRepository
```

## Arranque (`Program.cs`, `Helpers/KntExtensions.cs`)

- Configuración: secciones `AppSettings` (`Helpers/AppSettings`: `Secret` del JWT, `ActivateMessageBroker`,
  `MountResourceContainerOnStartup`), `RepositoryRef` (`Model/Config/RepositoryRef`: `Orm`, `Provider`,
  `ConnectionString`, contenedor de recursos...) y `ai` (proveedores del asistente de IA, ver "Asistente de IA").
  `RepositoryRef:Name` solo da nombre a una base de datos que aún no lo tiene (una nueva o una de antes de la
  revisión 5 de `KntSchemaUpdater`): el nombre del repositorio se guarda en la propia base de datos. Va vacío en
  `appsettings.json` para que, si falta, `Program.cs` use el antiguo `RepositoryRef:Alias` (de los user-secrets de
  un despliegue anterior; `LEGACY(repository-alias)`).
- `KntAddAiAssistant` registra lo del asistente de IA: `AiConfig` enlazado a la sección `ai` **al usarse** (no
  leído al arrancar como `AppSettings`/`RepositoryRef`, así los tests en proceso pueden sobrescribirlo),
  `Ai/AiProvidersCatalog` e `Ai/IAiChatClientProvider` (singletons).
- `KntAddServices` registra, ambos `Scoped` (uno por petición):
  `IKntRepository` vía `KntRepositoryFactory.Create(repositoryRef)` (`Service/Core`; decide Dapper/EF según
  `RepositoryRef.Orm` y antes pasa por `KntSchemaUpdater.EnsureUpToDate` de EF) e `IKntService` → `KntService`.
  Una sola base de datos por instancia de `Server` (a diferencia de `ClientWin`, que puede tener varias).
- Autenticación JWT bearer (clave simétrica `AppSettings.Secret`, sin validar issuer/audience,
  `ClockSkew = 0`), `AddAuthorization`, CORS `KntPolicy` abierto (`*`), SignalR, compresión de respuesta,
  NLog (`nlog.config`, logs en `Log/`). Para el hub (`ChatRoomHubContract.Path`) el token también se lee del
  parámetro `access_token` (`JwtBearerEvents.OnMessageReceived`): WebSockets y SSE no envían la cabecera
  `Authorization`. `ChatRoomManager` se registra como singleton.
- Los estáticos de `Client` (`UseStaticFiles` y `MapFallbackToFile("index.html")`) se sirven con
  `Cache-Control: no-cache`: sus URLs no cambian entre versiones (`index.html`, `css/*.css`,
  `KNote.Client.styles.css`) y sin esa cabecera el navegador sigue usando los antiguos tras un despliegue (HTML
  nuevo con CSS viejo). Con `no-cache` revalida por ETag y recibe `304` si no han cambiado.
- `IFileStore` → `LocalFileStore` (ficheros de recursos de notas en disco). `KntAddResourcesStaticFiles` monta
  el contenedor de recursos como estáticos solo si `MountResourceContainerOnStartup`.
- `KntConfigureMessageBroker`: experimental, crea un `KntService` propio con RabbitMQ si
  `ActivateMessageBroker`.
- `public partial class Program { }` al final existe para `WebApplicationFactory<Program>` de `Tests`: no lo
  quites.

## Patrón de un controlador

Todos siguen la misma plantilla (ver `NoteTypesController`, `FoldersController`):

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    public FoldersController(IKntService service, IHttpContextAccessor httpContextAccessor, ILogger<FoldersController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
        _logger = logger;
    }

    [HttpPost]
    [HttpPut]
    [Authorize(Roles = "Admin, ProjectManager")]
    public async Task<IActionResult> Post([FromBody] FolderDto folder)
    {
        try
        {
            var resApi = await _service.Folders.SaveAsync(folder);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ...);
            var resApi = new Result<FolderDto>();
            resApi.AddErrorMessage(ex.ToApiErrorMessage());
            return BadRequest(resApi);
        }
    }
}
```

- **Contrato de respuesta**: el cuerpo es **siempre un `Result<T>`**, también en los `BadRequest`. `Client`
  (`BaseService.ProcessResultFromHttpResponse`) lee ese cuerpo sin mirar el código de estado para mostrar al
  usuario el motivo real (p. ej. "no se puede borrar este tipo: N notas lo usan"). No devuelvas
  `BadRequest()` vacío, `Problem(...)` ni un string. Excepciones: `UsersController.Login`/`Register`, que
  devuelven `UserTokenDto` (`success`, `token`, `error`).
- Errores en el `catch`: `ex.ToApiErrorMessage()` (`Helpers/ExceptionExtensions`; la capa `Service` envuelve
  las excepciones en `KntServiceException`, así que se devuelve el mensaje de la causa raíz).
- `_service.UserIdentityName` se fija en cada controlador con el usuario del JWT: es la identidad del usuario
  para la capa `Service` (`GetCurrentUserAsync`, rol en el repositorio). No lo olvides en un controlador nuevo.
- Si un DTO nuevo necesita valores por defecto de negocio, que los ponga `Service` (`_service.Xxx.NewAsync()`)
  y se expongan con un `GET api/xxx/new`, como `NotesController.New`, en lugar de construirlos en `Client`.

## Autorización

- Se hace **en los controladores** con `[Authorize]` a nivel de clase + `[Authorize(Roles = "...")]` por acción
  (lectura abierta a cualquier usuario autenticado; escritura con rol). `[AllowAnonymous]` solo en `Login`,
  `Register` y lo que muestra la Home pública de `Client`: `NotesController.HomeNotes` (las notas de Home o, con
  `?folderId=`, de una carpeta hija directa de Home; cualquier otra la rechaza `Service`,
  `KntNotesHomeFolderAsyncCommand`), `FoldersController.HomeChildren` (las carpetas hijas directas de Home) y
  `SystemValuesController.RepositoryName` (el nombre del repositorio, `DB_REPOSITORYNAME`). Lo que se ve ahí es
  público: no amplíes esa zona sin decidirlo.
- Nombres de rol de `EnumRoles` (`Model/KntRoles.cs`): `Guest`, `Staff`, `ProjectManager`, `Admin`. En ASP.NET
  **no son jerárquicos**: enumera todos los roles permitidos.
- La autorización por comando de `Service` (`[KntAuthorize]`) **está apagada en `Server`**
  (`IKntService.EnforceAuthorization = false`, ver `CLAUDE.md` raíz): aquí el único filtro son estos
  atributos. Al añadir o cambiar un endpoint, alinea su rol con el `[KntAuthorize]` del comando que invoca
  (y con el `[Authorize(Roles)]` de la página de `Client` que lo usa), para que Web y escritorio permitan lo
  mismo.
- El token lo emite `UsersController.BuildToken`: claims `Name` (UserName), `UniqueName` (email), `Jti` y un
  `Role` por cada valor de `User.RoleDefinition` (separado por comas); HS256 con `AppSettings.Secret`; caduca
  en un año. El rol de un usuario que se registra lo decide `Service` (`Users.RegisterAsync`), no el cuerpo de
  la petición.

## Recursos de notas y descripciones

- `NotesController` completa en su constructor `RepositoryRef.ResourcesContainerRootPath`/`RootUrl` con los
  valores del host (`IFileStore`) cuando vienen vacíos en la configuración.
- Al leer y guardar notas, la descripción pasa por `UtilUpdateResourceInDescriptionForRead`/`ForWrite`
  (`Service`), y al guardar se ajustan el `ContentType` y los saltos de línea (ver el punto siguiente). Está
  marcado como *hack* de compatibilidad con `ClientWin`: si lo cambias, comprueba que una nota
  editada en la Web se sigue viendo bien en el escritorio y viceversa.
- **`ContentType` y saltos de línea al guardar una nota desde la Web** (`NotesController.Post`): el
  `ContentType` es el JSON de `ClientWin` (`ContentTypeExt`: vista de la descripción `des` = `markdown`,
  `navigation` o `html`; lenguaje del script `scr`; contenido bloqueado `blk`). Se lee con `GetContentTypeExt`
  (que también lee el formato antiguo de texto plano, `markdown`, `navigation#`...) y se guarda siempre con
  `SetContentTypeExt`, en JSON. Solo se decide la vista: `html` si la descripción empieza por `<BODY`; si no,
  Markdown, conservando `navigation` (vista renderizada en `ClientWin`) o `markdown`; `scr` y `blk` se respetan.
  Los saltos de línea de la descripción se normalizan a un CR LF cada uno (`\r*\n|\r`), como los escribe
  `ClientWin`: un `Replace("\n", "\r\n")` anterior convertía los CR LF en CR CR LF.
- **Descripción de las tareas** (`api/notes/{id}/tasks`, `POST/PUT api/notes/tasks`, y las del calendario): es
  Markdown como la de una nota, así que pasa por `UtilUpdateResourceInDescriptionForRead`/`ForWrite` y al guardar
  sus saltos de línea se normalizan a CR LF.
- **Editar un recurso** (`PUT api/notes/resources`, `ResourceInfoDto`): un recurso que ya existe solo cambia su
  descripción y su orden (`KntNotesSaveResourceInfoAsyncCommand` parte del guardado); no hace falta reenviar el
  fichero (`ResourceInfoDto` solo exige `ContentBase64` a un recurso nuevo), y se conservan el fichero y el
  contenido guardado en la BD (`ResourceInfoDto` no dice si lo está).
- **Notas trazadas** (pestaña "Trace notes" del editor de la Web, Staff+ como los comandos): `GET
  api/notes/{id}/tracenotes` da las dos direcciones (`NoteTraceNotesDto`: `TraceNotesFrom`, la nota es `ToId`;
  `TraceNotesTo`, es `FromId`), cada relación como `TraceNoteInfoDto` (el `TraceNoteDto` más la otra nota, número,
  tema y etiquetas, y el nombre del tipo), resueltas con `TraceNoteRows` de `KNote.Reports`, las mismas filas que el
  editor de `ClientWin`. `POST/PUT api/notes/tracenotes` guarda una relación **al momento** (`Guid.Empty` = nueva;
  en `ClientWin` se guardan con la nota) y `DELETE api/notes/tracenotes/{id}` la borra. `Service` rechaza una nota
  trazada consigo misma (validación del DTO, un 400 de `[ApiController]`) y una relación repetida (mismas notas,
  sentido y tipo, también sin tipo: `KntNotesSaveTraceNoteAsyncCommand`). Los tipos: `TraceNoteTypesController`
  (`api/tracenotetypes`, lectura para cualquier usuario autenticado, escritura Admin); un tipo en uso no se borra
  (la clave ajena de la BD, el error vuelve en el `Result`). Tests en `NotesInProcessTests.Execute_TraceNotesCRUD`
  y `TraceNoteTypesInProcessTests`.
- **Ruta de una carpeta**: `GET api/folders/{id}/path` (`Folders.GetPathAsync` de `Service`, la misma que usa
  `ClientWin`), para el editor de notas.
- **HTML de las notas para la Web**: `POST api/notes/html` (cualquier usuario autenticado; cuerpo: el Markdown
  como cadena JSON) devuelve el HTML de una descripción saneado (`Notes.UtilMarkdownToHtml`: Markdig +
  `KntHtmlSanitizer` de `Service`, las mismas reglas que `ClientWin`); `Client` no genera HTML de notas.
  `HomeNotes`, que es anónimo, devuelve las descripciones ya convertidas así (no hay un endpoint anónimo que
  convierta Markdown arbitrario).
- **Informes y exportación** (`ReportsController`, `api/reports`, Staff+ como en `ClientWin`): `POST noteslist`
  (lista apaisada), `POST notesbook` (libro, un capítulo por nota) y `POST noteslistcsv` (CSV) reciben un
  `NotesListReportRequestDto` (carpeta, búsqueda o filtro, el orden de la lista, vista compacta y la cultura del
  usuario) y `GET notes/{id}?culture=` da el detalle de una nota. Devuelven `ReportDto` (la página HTML completa,
  que `Client` muestra e imprime) o `ReportFileDto` (el CSV, con BOM). La lógica está en `KNote.Reports`
  (`WebReports`, los mismos generadores que `ClientWin`): el servidor recarga la lista entera con el orden pedido,
  las imágenes se cargan de la URL de los recursos de este host (`WebReportResources`) y el HTML de las notas
  sale saneado. La cultura solo vale para esa petición (`UseCulture`). El índice del libro no lleva números de
  página (el navegador no los puede calcular). Tests en `Tests/InProcessIntegrationTests/ReportsInProcessTests`.

## Asistente de IA (`AiAssistantController`, `api/aiassistant`)

Staff, ProjectManager y Admin (como `KNoteAIAssistantCtrl` en `ClientWin` y los comandos de sesiones):

- `GET providers` — los proveedores configurados (`AiProviderInfoDto`: alias, proveedor, modelo, cuál es el
  predefinido), **nunca** su `ApiKey` ni su `Host`.
- `POST chat` (`AiChatRequestDto`: alias del proveedor, turnos anteriores y pregunta) — un turno con streaming
  como **server-sent events** (`TypedResults.ServerSentEvents`): un `AiChatStreamEventDto` por evento, con su
  `Type` como tipo de evento SSE (`AiChatStreamEventTypes`: `delta`, `tool`, `noteCreated`, `completed` con el
  turno y su uso, `error`). El servidor no guarda la conversación: la manda entera el cliente en cada
  pregunta. Lo que falla antes de empezar (pregunta vacía, proveedor desconocido) es un 400 con `Result`, como
  siempre; lo que falla después, un evento `error`. La lógica del turno es la común de `KNote.Ai`
  (`AiChatTurnStreamer`), con las tools de `KNoteAiTools`; su host en el Server (`Ai/ServerAiToolsHost`)
  guarda las notas de `create_task` en la carpeta Home y las anuncia con eventos `noteCreated`. Cancelar la
  petición (`RequestAborted`) corta la llamada al proveedor.
- `GET sessions`, `GET sessions/{noteId}`, `POST sessions` — las sesiones del usuario
  (`IKntService.AiSessions`, ver `CLAUDE.md` raíz). El cliente guarda la sesión tras cada respuesta.

Proveedores: sección `ai.providers` de `appsettings.json`, con los mismos campos que `AiProviderRef` en
`ClientWin` (`alias`, `provider` = `OpenAI`/`Anthropic`/`Ollama`, `model`, `apiKey`, `host` solo para Ollama).
`Ai/AiProvidersCatalog` descarta (y registra en el log) los que no validan; el primero válido es el
predefinido. Las `apiKey` no van en `appsettings.json`: user-secrets (`ai:providers:0:apiKey`, por posición) o,
si están vacías, las variables `OPENAI_API_KEY`/`ANTHROPIC_API_KEY` (`AiChatClientFactory.ResolveApiKey`).
`Ai/IAiChatClientProvider` construye el `IChatClient` (`AiChatClientFactory` de `KNote.Ai`); existe para que los
tests lo sustituyan.

## Chat room (`Hubs/ChatHub`, `ChatRoomController`)

El contrato (rutas, métodos y eventos) está en `Model/Dto/ChatRoomHubContract`, compartido con `Client` y `ClientWin`.
Los hilos los guarda `IKntService.ChatRooms` (ver `CLAUDE.md` raíz).

- **`ChatHub`** (`/chathub`, `[Authorize(Roles = "Staff, ProjectManager, Admin")]`): capa fina sobre
  `IKntService.ChatRooms`. Métodos `GetThreads`, `CreateThread`, `GetGeneralThread`, `Subscribe`, `Unsubscribe` y
  `SendMessage`, que devuelven `Result<T>` (un error es un `Result` inválido, nunca una excepción al cliente).
  Eventos: `MessageReceived` (al grupo del hilo), `ThreadCreated` y `ThreadActivity` (a todos) y
  `OnlineUsersChanged` (al grupo). **Un grupo de SignalR por hilo** (`chatroom:{id}`): un cliente recibe los mensajes
  de los hilos a los que se suscribe en su conexión, y se suscribe de nuevo al reconectar (los grupos son de la
  conexión). Escribir no exige suscripción.
- **`ChatRoomManager`** (singleton; un `Hub` vive una llamada): qué hilos sigue cada conexión (los usuarios
  conectados de cada hilo, `OnDisconnectedAsync` los limpia), **un bloqueo por hilo** para guardar y difundir los
  mensajes en orden (el servicio no serializa: añadir un mensaje reescribe la nota) y otro para no crear dos veces
  el hilo `General`.
- **Imágenes**: no van por el hub (los mensajes de SignalR son pequeños). `POST api/chatroom/threads/{id}/images`
  (`ChatRoomController`, Staff+, cuerpo `ChatRoomImageDto` en base64) las guarda como recursos de la nota del hilo
  (`ChatRooms.AddImageAsync`) y devuelve su URL y su Markdown con la URL absoluta; el cliente envía ese Markdown
  como un mensaje. En la nota se guarda la ruta relativa a los recursos, como en las descripciones: el hub aplica
  `UtilUpdateResourceInDescriptionForWrite` al guardar y `ForRead` al dar los mensajes (`SendMessage`,
  `Subscribe`), y fija `ResourcesContainerRootPath`/`RootUrl` del host si están vacíos, como `NotesController` (la
  URL, de la petición de la conexión). Con `MountResourceContainerOnStartup` la URL de los recursos puede ser
  relativa (`/Ruta`): la Web la resuelve sola y ClientWin con un `<base href>` del servidor.

## Otros endpoints

- `SystemValuesController`: solo Admin. `WeatherForecastController`: restos de la plantilla.
- `Helpers/CheckUserPermissionsMiddleware`, `HttpContextExtensions` (cabeceras de paginación): sin uso
  actualmente.

## Configuración y secretos

`appsettings.json` solo contiene marcadores (`"... MyLongStringSecurityKeySecret ..."`). Los valores reales
(`AppSettings:Secret`, `RepositoryRef:ConnectionString`, `ai:providers:n:apiKey`...) van en
user-secrets (`UserSecretsId` en `KNote.Server.csproj`) o variables de entorno (`RepositoryRef__Orm`, ...).
No escribas secretos reales en `appsettings.json`.

## Compilar, ejecutar y depurar

- Visual Studio: proyecto de inicio `Server`, perfil `KNote.Server` (`https://localhost:44339/KNote`, abre el
  navegador; permite depurar también `Client`). Hay también un perfil `IIS Express`.
- VS Code: configuración `KNote.Web (Server + Client WASM)` de `.vscode/launch.json` para depurar `Server` y
  `Client` juntos (ver `Client/CLAUDE.md`), o `KNote.Server` (`coreclr`, tarea previa `build: Server`; abre
  `<url>/KNote`) para depurar solo `Server`. La tarea `watch: Server` para iterar sin depurador.- CLI: `dotnet run --project Server/KNote.Server.csproj`.
- `KNote.http`/`KNoteServer.http`: peticiones de prueba manuales.

## Tests

En `Tests/` (`KNoteTest.slnx`), ver `CLAUDE.md` raíz:

- `Tests/InProcessIntegrationTests` — levantan `Server` en proceso con `Tests/Helpers/KNoteWebApplicationFactory`
  (Sqlite temporal por instancia, configuración por variables de entorno `RepositoryRef__*`). Es donde se
  añaden los tests de un endpoint nuevo. Antes de crear el cliente, `KNoteWebApplicationFactory.AppConfiguration`
  sobrescribe configuración leída con `IOptions` (p. ej. `ai:providers`) y `TestServices` sustituye servicios
  (ver `InProcessTestHost.CreateAuthenticatedClientAsync(configure)`). `AiAssistantInProcessTests` usa
  `Helpers/ScriptedAiChatClientProvider`: un modelo con respuestas guionizadas (texto, uso, llamadas a tools)
  envuelto con las tools y la invocación de funciones reales, sin llamar a ningún proveedor.
  `ChatRoomHubInProcessTests` usa el cliente de SignalR contra el host (`HttpMessageHandlerFactory` del
  `TestServer`, LongPolling) con usuarios Staff y Guest creados por la API. La fábrica guarda los recursos de las
  notas en una carpeta temporal propia (`ResourcesFolder`, `ResourcesContainer`, `ResourcesRootUrl`): sin ella se
  escribirían en el `wwwroot` del proyecto `Server`.
- `Tests/WebApiIntegrationTests` — contra un `Server` real ya en ejecución.

## Añadir un endpoint

1. La operación existe en `Service` (si no, añádela allí, con su `[KntAuthorize]` y su fila en
   `Tests/ServiceTests/CommandAuthorizationMatrixTests`).
2. Acción en el controlador con la plantilla de arriba y su `[Authorize(Roles = ...)]` alineado con el comando.
3. Test en `Tests/InProcessIntegrationTests`.
4. Método en el `I*WebApiService` correspondiente de `Client` (ver `Client/CLAUDE.md`).
