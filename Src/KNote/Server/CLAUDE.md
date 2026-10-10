# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this project
(`Server`, el backend ASP.NET Core de la versión Web de KNote). Ver también el `CLAUDE.md` de la raíz del repo
para el contexto general (patrón Repository, capa Service, autorización por comando) y `Client/CLAUDE.md`
para el frontend Blazor que consume esta API.

## Papel de `Server`

Un único proceso ASP.NET Core que:

- expone la **API REST** (`Controllers/`, rutas `api/[controller]`) que usa `Client`;
- expone el **hub SignalR** del chat (`Hubs/ChatHub`, en `/chathub`);
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
- `KntAddAiAssistant` registra lo del asistente de IA: `AiConfig` enlazado a la sección `ai` **al usarse** (no
  leído al arrancar como `AppSettings`/`RepositoryRef`, así los tests en proceso pueden sobrescribirlo),
  `Ai/AiProvidersCatalog` e `Ai/IAiChatClientProvider` (singletons).
- `KntAddServices` registra, ambos `Scoped` (uno por petición):
  `IKntRepository` vía `KntRepositoryFactory.Create(repositoryRef)` (`Service/Core`; decide Dapper/EF según
  `RepositoryRef.Orm` y antes pasa por `KntSchemaUpdater.EnsureUpToDate` de EF) e `IKntService` → `KntService`.
  Una sola base de datos por instancia de `Server` (a diferencia de `ClientWin`, que puede tener varias).
- Autenticación JWT bearer (clave simétrica `AppSettings.Secret`, sin validar issuer/audience,
  `ClockSkew = 0`), `AddAuthorization`, CORS `KntPolicy` abierto (`*`), SignalR, compresión de respuesta,
  NLog (`nlog.config`, logs en `Log/`).
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
  `Register` y `NotesController.HomeNotes`.
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
  (`Service`) y se ajusta el tipo de contenido (`html` si empieza por `<BODY`, si no `markdown` con `\n` →
  `\r\n`). Está marcado como *hack* de compatibilidad con `ClientWin`: si lo cambias, comprueba que una nota
  editada en la Web se sigue viendo bien en el escritorio y viceversa.

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
- `Tests/WebApiIntegrationTests` — contra un `Server` real ya en ejecución.

## Añadir un endpoint

1. La operación existe en `Service` (si no, añádela allí, con su `[KntAuthorize]` y su fila en
   `Tests/ServiceTests/CommandAuthorizationMatrixTests`).
2. Acción en el controlador con la plantilla de arriba y su `[Authorize(Roles = ...)]` alineado con el comando.
3. Test en `Tests/InProcessIntegrationTests`.
4. Método en el `I*WebApiService` correspondiente de `Client` (ver `Client/CLAUDE.md`).
