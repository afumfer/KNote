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
  `MountResourceContainerOnStartup`), los repositorios (ver "Repositorios") y `ai` (proveedores del asistente de IA,
  ver "Asistente de IA").
- `KntAddAiAssistant` registra lo del asistente de IA: `AiConfig` enlazado a la sección `ai` **al usarse** (no
  leído al arrancar como `AppSettings`/`RepositoryRef`, así los tests en proceso pueden sobrescribirlo),
  `Ai/AiProvidersCatalog` e `Ai/IAiChatClientProvider` (singletons).
- `KntRepositories.OpenAsync` abre los repositorios al arrancar y `KntAddServices` registra el registro (singleton) y
  el `IKntService` de cada petición (`Scoped`), el del repositorio de la sesión (ver "Repositorios").
- Autenticación JWT bearer (clave simétrica `AppSettings.Secret`, sin validar issuer/audience,
  `ClockSkew = 0`), `AddAuthorization`, SignalR, compresión de respuesta, NLog (`nlog.config`, logs en `Log/`, rutas
  con `/` para que valgan en cualquier sistema).
- **CORS** (`Helpers/KntCors`, sección `Cors:AllowedOrigins`): vacía por defecto, sin orígenes cruzados (`Client` es del
  mismo origen; `ClientWin` y los tests no son navegadores, CORS no les aplica); `"*"` = cualquier origen. `UseKntCors`
  va entre `UseRouting` y `UseAuthentication`.
- **Tras un proxy inverso** (`Helpers/KntForwardedHeaders`, sección `ForwardedHeaders`: `Enabled`, `KnownProxies`,
  `KnownNetworks` en CIDR, `ForwardLimit`): desactivado por defecto (en IIS, ANCM ya da la dirección y el esquema del
  cliente). Activado, `UseForwardedHeaders` es el primer middleware y aplica `X-Forwarded-For/Proto/Host` de los
  proxies de confianza (siempre los de loopback, más los configurados): sin él, el límite de inicios de sesión por
  dirección (`KntRateLimits`) sería uno para todos y las URLs construidas desde la petición (`LocalFileStore`, `ChatHub`)
  llevarían el esquema y el host del proxy. Tests en `ProxyAndCorsInProcessTests`. Para el hub (`ChatRoomHubContract.Path`) el token también se lee del
  parámetro `access_token` (`JwtBearerEvents.OnMessageReceived`): WebSockets y SSE no envían la cabecera
  `Authorization`. `ChatRoomManager` se registra como singleton. `JwtBearerEvents.OnTokenValidated` rechaza los
  tokens sin repositorio (`knt_repo`) o de uno que no está en `KntRepositories`, y los de huella de contraseña que ya
  no es la del usuario en ese repositorio (ver "Autorización"). Lo comprueba con un servicio del repositorio del token
  creado por el registro, **nunca con el `IKntService` de la petición**: aún no hay `HttpContext.User` y quedaría el del
  repositorio por defecto para toda la petición.
- `KntAddPasswordReset` (secciones `PasswordReset` y `Email`, `IEmailSender`, `Helpers/KntPasswordResetMail`) y
  `KntAddRateLimits` (`Helpers/KntRateLimits`, sección `RateLimits`; `UseRateLimiter` tras `UseRouting`), ver
  "Cuentas de usuario".
- `KntAddAlarms` (`Helpers/KntAlarms`, sección `Alarms`): `AlarmsConfig` enlazado al usarse y `EmailAlarmsService`
  (singleton y hosted service), ver "Alarmas".
- Los estáticos de `Client` (`UseStaticFiles` y `MapFallbackToFile("index.html")`) se sirven con
  `Cache-Control: no-cache`: sus URLs no cambian entre versiones (`index.html`, `css/*.css`,
  `KNote.Client.styles.css`) y sin esa cabecera el navegador sigue usando los antiguos tras un despliegue (HTML
  nuevo con CSS viejo). Con `no-cache` revalida por ETag y recibe `304` si no han cambiado.
- `IFileStore` → `LocalFileStore` (ficheros en disco; solo lo usa ya la acción obsoleta `NotesController.SaveFile`).
  `KntAddResourcesStaticFiles`, solo si `MountResourceContainerOnStartup`, monta como estáticos la carpeta de recursos
  de cada repositorio cuya `ResourcesContainerRootUrl` es una ruta de este host (`/...`).
- `KntConfigureMessageBroker`: experimental, crea un `KntService` propio con RabbitMQ si `ActivateMessageBroker`, solo
  para el repositorio por defecto.

## Repositorios (`Helpers/KntRepositories`)

Un `Server` sirve uno o varios repositorios; **una sesión está en uno** (claim `knt_repo` del token,
`KntConst.RepositoryClaimType`, con su `RepositoryId`), y los controladores y las páginas no saben nada de ello.

- **Configuración**: la lista `Repositories` (cada uno, los campos de `RepositoryRef` más `IsDefault`;
  `KntRepositoryConfig`, solo en `Server`) o, si está vacía, la sección `RepositoryRef` (un solo repositorio: forma
  permanente, la del despliegue IIS y la de los tests). `appsettings.json` lleva `"Repositories": []` para no tapar el
  `RepositoryRef` de los user-secrets. Con `RepositoryRef`, `Name` vacío toma el antiguo `Alias`
  (`LEGACY(repository-alias)`). Ejemplo (en user-secrets o variables `Repositories__0__ConnectionString`...):

  ```json
  "Repositories": [
    { "Name": "Work", "Orm": "EntityFramework", "Provider": "Microsoft.Data.SqlClient", "ConnectionString": "...",
      "ResourcesContainer": "NotesResources", "ResourcesContainerRootPath": "", "ResourcesContainerRootUrl": "",
      "IsDefault": true },
    { "Name": "Personal", "Orm": "Dapper", "Provider": "Microsoft.Data.Sqlite", "ConnectionString": "Data Source=D:\\KNote\\Personal.db",
      "ResourcesContainer": "NotesResources", "ResourcesContainerRootPath": "D:\\KNote\\PersonalResources",
      "ResourcesContainerRootUrl": "/PersonalResources" }
  ]
  ```
- **Al arrancar** (`OpenAsync`), cada uno se abre (`KntRepositoryFactory` → `KntSchemaUpdater`) y se lee su
  `RepositoryId`. Se dejan fuera, con el error en el log y sin impedir que arranquen los demás: el que no se puede abrir
  (no se reintenta: hay que reiniciar), una copia de otro (mismo `RepositoryId`, como `ClientWin`) y uno cuyos recursos
  (carpeta `RootPath` + `ResourcesContainer`, o su URL) serían los de otro. Sin ninguno, el `Server` no arranca. El
  predeterminado es el `IsDefault` (el primero si hay varios) o, si no hay o no se abrió, el primero. Los nombres no se
  guardan: se leen de cada base de datos (un Admin los cambia).
- **Por petición** (`KntAddServices`): `CreateService(ForUser(HttpContext.User), ...)`: el repositorio del token o,
  sin usuario (Home pública, login, registro, olvidé/restablecer contraseña), el predeterminado. Cada petición recibe
  **una copia** del `RepositoryRef`, con la carpeta (`wwwroot`) y la URL (`{Scheme}://{Host}{PathBase}`) de los
  recursos de este host si la configuración no las da: antes cada controlador las escribía en un `RepositoryRef`
  compartido, y la primera URL se quedaba para siempre. Para varios repositorios, cada uno necesita su `RootPath` y su
  `RootUrl` (solo uno puede usar el `wwwroot`): `ResourcesContainer` es el guardado en esa base de datos (lo comparte
  con `ClientWin`). En IIS, cada repositorio con su directorio virtual (o el montaje de `MountResourceContainerOnStartup`)
  y la identidad del pool con permisos en su carpeta.
- Tests: `Tests/InProcessIntegrationTests/KntRepositoriesTests` (sin host) y `MultiRepositoryInProcessTests`.
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
  devuelven `UserTokenDto` (`success`, `token`, `error`); `ChangePassword` y `ResetPassword` devuelven un
  `Result<UserTokenDto>` con el token nuevo.
- Errores en el `catch`: `ex.ToApiErrorMessage()` (`Helpers/ExceptionExtensions`; la capa `Service` envuelve
  las excepciones en `KntServiceException`, así que se devuelve el mensaje de la causa raíz).
- `_service.UserIdentityName` se fija en cada controlador con el usuario del JWT: es la identidad del usuario
  para la capa `Service` (`GetCurrentUserAsync`, rol en el repositorio). No lo olvides en un controlador nuevo.
- Si un DTO nuevo necesita valores por defecto de negocio, que los ponga `Service` (`_service.Xxx.NewAsync()`)
  y se expongan con un `GET api/xxx/new`, como `NotesController.New`, en lugar de construirlos en `Client`.

## Autorización

- Se hace **en los controladores** con `[Authorize]` a nivel de clase + `[Authorize(Roles = "...")]` por acción
  (lectura abierta a cualquier usuario autenticado; escritura con rol). `[AllowAnonymous]` solo en `Login`,
  `Register`, `ForgotPassword`, `ResetPassword` y lo que muestra la Home pública de `Client`:
  `NotesController.HomeNotes` (las notas de Home o, con `?folderId=`, de una carpeta hija directa de Home; cualquier
  otra la rechaza `Service`,
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
- El token lo emite `UsersController.BuildTokenAsync` para un repositorio: claims `Name` (UserName), `UniqueName`
  (email), `Jti`, `knt_repo` (el `RepositoryId`) y un `Role` por cada valor de `User.RoleDefinition` en **ese
  repositorio** (separado por comas); HS256 con `AppSettings.Secret`; caduca en un año. El rol de un usuario que se
  registra lo decide `Service` (`Users.RegisterAsync`), no el cuerpo de la petición. Un token sin `knt_repo` (de antes
  del multirepositorio) se rechaza: tras actualizar, todos vuelven a entrar una vez (también el chat de `ClientWin`).
- **Sesiones cerradas al cambiar la contraseña**: el token lleva también `knt_pwd`, la huella de la contraseña del
  usuario en su repositorio (`Users.GetPasswordStampAsync`, `KntPasswordHasher.Stamp`), y `OnTokenValidated` rechaza el
  que no la lleva o no coincide (API y hub). `Helpers/KntPasswordStamps` las cachea un minuto por repositorio y
  usuario: lo que cambia la contraseña aquí la olvida al momento (`BuildTokenAsync` la relee, `SetPassword` y
  `ChangePassword` la olvidan en cada repositorio donde cambia); un cambio desde `ClientWin` sobre la misma BD cuenta en
  un minuto como mucho.

## Cuentas de usuario (`UsersController`)

- **Iniciar sesión** (`POST api/users/login`, `UserCredentialsDto`), en dos pasos: sin `RepositoryId`, las credenciales
  se comprueban en cada repositorio; válidas en uno, su token; en varios, `success = false`, sin token, con
  `UserTokenDto.Repositories` (solo donde valen: a un anónimo no se le dice qué repositorios hay) y un `error` que
  entiende un cliente antiguo; el cliente vuelve a enviar las credenciales con el `RepositoryId` elegido (no hay
  ticket: no aporta nada frente a quien ya tiene la contraseña). Con `RepositoryId`, solo ahí ("User not
  authenticated" si no valen o no existe). El límite de intentos es el mismo: el paso 1 verifica un hash por
  repositorio, y elegir cuesta una petición más.
- **Registro** (`Register`, anónimo): en el repositorio por defecto. El alta por un Admin (`POST api/users`) es en el
  de su sesión.
- `GET/PUT api/users/me` (perfil, **solo en el repositorio de la sesión**: el mismo nombre de usuario en otro
  repositorio puede ser otra persona, y guardar el perfil no pide la contraseña) y `POST api/users/me/password`
  (cambio, devuelve el token nuevo): cualquier usuario autenticado, siempre su propia cuenta
  (`Users.GetProfileAsync`...). El cambio de contraseña, como en `ClientWin`, primero en el repositorio de la sesión
  (si falla, nada más) y después en los demás donde la contraseña actual abre ese usuario (las mismas credenciales, la
  misma persona); `UserTokenDto.Notice` dice dónde cambió y dónde no pudo (solo entre esos). `PUT api/users/{id}/password`:
  un Admin pone la contraseña de otro (`UserSetPasswordDto`), en el repositorio de su sesión.
- **Olvidé mi contraseña**: `POST api/users/forgot-password` (`UserForgotPasswordDto`, usuario o email) responde
  siempre lo mismo y, por cada repositorio donde hay ese usuario con correo, un token de
  `Users.CreatePasswordResetAsync` (firmado con `AppSettings.Secret`; lleva el `RepositoryId` y solo vale en él,
  `KntPasswordResetToken` v2 de `Service`). Se agrupan por **usuario y correo** (lo que prueba quién pide es el buzón:
  la cuenta de otro repositorio con el mismo nombre y otro correo puede ser otra persona y recibe su propio enlace en su
  dirección) y `KntPasswordResetMail` envía en segundo plano (la respuesta tarda lo mismo) **un correo con un solo
  enlace** por grupo, `{PasswordReset:PublicBaseUrl}reset-password?token=...`, que dice en qué repositorios se pondrá la
  contraseña. El token del enlace (`KntPasswordResetLinkToken`) es la lista de los tokens de cada repositorio, sin firma
  propia (cada uno la lleva). `PublicBaseUrl` se configura, nunca sale de la petición (su `Host` se puede falsear). Sin
  `PublicBaseUrl`, `Email:Host` o `Email:FromAddress` responde que no está disponible.
  `POST api/users/reset-password` (`UserResetPasswordDto`) pone la contraseña en cada repositorio del enlace (olvida sus
  huellas): en uno, devuelve su token; en varios, `Repositories` y `UserName` sin token (`Client` entra como en el segundo
  paso del login); `Notice` dice dónde no se pudo (p. ej. la contraseña cambió allí mientras tanto). Si no se pudo en
  ninguno, un 400 con el motivo (la política de contraseñas es la misma en todos: el mismo usuario).
- **Límites** (`[EnableRateLimiting]`, por dirección IP, ventana fija): `Login` (`RateLimits:SignInPerMinute`, 10) y
  forgot/reset (`RateLimits:PasswordResetPerHour`, 10). El 429 lleva un cuerpo que se lee como `Result<T>` y como
  `UserTokenDto`. `Register` no está limitado (los tests en proceso registran muchos usuarios).

## Recursos de notas y descripciones

- `ResourcesContainerRootPath`/`RootUrl` vacíos en la configuración son los del host (`wwwroot` y la URL de la
  petición), puestos por `KntRepositories.CreateService` en la copia del `RepositoryRef` de cada petición (ver
  "Repositorios").
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
- **Alarmas de una nota** (sus `KMessage`, pestaña "Alarms" del editor de la Web): se guardan al momento, como las
  tareas. `GET api/notes/{id}/alarms` (la lista, con el nombre del usuario) y `GET api/notes/alarms/{id}`: cualquier
  usuario autenticado; `GET api/notes/{id}/alarms/new` (una alarma nueva con los valores de `Notes.NewMessageAsync`:
  usuario actual, activada, ahora al minuto, Application info), `POST/PUT api/notes/alarms` y `DELETE
  api/notes/alarms/{id}`: Staff+, como los comandos. Las reglas, en `KMessageDto.Validate` (fecha obligatoria; "cada N
  minutos" con minutos). Guardar una alarma no recalcula el estado de la nota ("Alarms pending", `InternalTags`): lo hace
  el guardado de la nota, como con las tareas. Tests en `NoteAlarmsInProcessTests`.
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

## Alarmas (`AlarmsController`, `Helpers/KntAlarms`)

Las alarmas que vencen se **consumen** con `Notes.ConsumeDueAlarmsAsync` (las del usuario actual) o
`ConsumeAllUsersDueAlarmsAsync` (las de todos, Admin): cada una se reprograma o se desactiva con una actualización
condicional y la recibe un solo consumidor, aunque consulten a la vez `ClientWin`, el `Server` y varias pestañas (ver
`CLAUDE.md` raíz). Las fechas son la hora local del servidor, como en `ClientWin`.

- **Avisos de la Web** (`api/alarms`, Staff+): `POST check` consume las alarmas *Application info* vencidas del usuario
  y devuelve `AlarmCheckDto`: los avisos ya rellenos (`AlarmNoticeDto`: alarma, nota, número, tema, comentario y fecha)
  y cada cuántos segundos volver a consultar (`Alarms:AppInfoCheckSeconds`, 60; entre 15 y 3600). `POST notices`
  recibe los `KMessageId` que guarda el panel (en el navegador) y devuelve los datos actuales de los que siguen siendo
  avisos del usuario (`Notes.GetAlarmNoticesAsync`): se omiten los borrados o cambiados a otro usuario o tipo. El
  servidor no guarda qué se ha avisado: lo guarda el navegador, como `ClientWin` en la carpeta del usuario.
- **Alarmas de correo** (`EmailAlarmsService`, `BackgroundService` con `PeriodicTimer`): **apagado por defecto**
  (`Alarms:Email:Enabled`; cada `Alarms:Email:IntervalSeconds`, 60). Encendido, en cada vuelta y en cada repositorio de
  `KntRepositories`, consume las alarmas *Email* de todos los usuarios (un servicio sin usuario: el `Server` no exige
  autorización) y envía cada una al correo de su usuario con la cuenta SMTP de la sección `Email` y el mismo correo que
  `ClientWin` (`Service/Core/KntAlarmEmail`: el tema de la nota, el comentario y la descripción). Sin `Email:Host` o
  `Email:FromAddress` no consume nada. Si el SMTP falla, la alarma se guarda como estaba al vencer, para reintentarla
  en la siguiente vuelta; un usuario sin correo es un fallo definitivo que se escribe en el comentario de la alarma
  (`KntAlarmEmail.FailedComment`, como `ClientWin`). Un repositorio o un correo que falla se registra en el log y los
  demás siguen. Si lo enciendes, desactiva "Process Email alarms" en las Opciones de `ClientWin` (si no, los dos
  envían, aunque cada alarma una sola vez: la envía el primero que la consume). `SendDueAlarmsAsync` es una vuelta,
  pública para los tests.
- Tests en `AlarmsInProcessTests` (dos repositorios; las vueltas del servicio las lanza el test) y
  `Tests/ServiceTests/AlarmNoticesServiceTests`.

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
  **Cada repositorio tiene su chat room**: una conexión está en el repositorio de su token, y el hub crea su servicio
  con `KntRepositories` y `Context.User` (no con el `IKntService` de DI ni con `IHttpContextAccessor`).
  Eventos: `MessageReceived` (al grupo del hilo), `ThreadCreated` y `ThreadActivity` (al grupo del repositorio,
  `repository:{id}`, al que se une cada conexión en `OnConnectedAsync`) y `OnlineUsersChanged` (al grupo del hilo).
  **Un grupo de SignalR por hilo** (`chatroom:{repositorio}:{hilo}`): un cliente recibe los mensajes de los hilos a los
  que se suscribe en su conexión, y se suscribe de nuevo al reconectar (los grupos son de la conexión). Escribir no
  exige suscripción.
- **`ChatRoomManager`** (singleton; un `Hub` vive una llamada): qué hilos sigue cada conexión (los usuarios
  conectados de cada hilo, `OnDisconnectedAsync` los limpia), **un bloqueo por hilo** para guardar y difundir los
  mensajes en orden (el servicio no serializa: añadir un mensaje reescribe la nota) y otro por repositorio para no
  crear dos veces el hilo `General`. Las claves llevan el repositorio (`ChatThreadKey`).
- **Imágenes**: no van por el hub (los mensajes de SignalR son pequeños). `POST api/chatroom/threads/{id}/images`
  (`ChatRoomController`, Staff+, cuerpo `ChatRoomImageDto` en base64) las guarda como recursos de la nota del hilo
  (`ChatRooms.AddImageAsync`) y devuelve su URL y su Markdown con la URL absoluta; el cliente envía ese Markdown
  como un mensaje. En la nota se guarda la ruta relativa a los recursos, como en las descripciones: el hub aplica
  `UtilUpdateResourceInDescriptionForWrite` al guardar y `ForRead` al dar los mensajes (`SendMessage`,
  `Subscribe`), con los `ResourcesContainerRootPath`/`RootUrl` del host si están vacíos, como en las demás peticiones
  (la URL, de la petición de la conexión). Con `MountResourceContainerOnStartup` la URL de los recursos puede ser
  relativa (`/Ruta`): la Web la resuelve sola y ClientWin con un `<base href>` del servidor.

## Otros endpoints

- `SystemValuesController`: solo Admin. `WeatherForecastController`: restos de la plantilla.
- `Helpers/CheckUserPermissionsMiddleware`, `HttpContextExtensions` (cabeceras de paginación): sin uso
  actualmente.

## Configuración y secretos

`appsettings.json` solo contiene marcadores (`"... MyLongStringSecurityKeySecret ..."`). Los valores reales
(`AppSettings:Secret`, `RepositoryRef:ConnectionString` o `Repositories:n:ConnectionString`, `ai:providers:n:apiKey`,
`Email:Password`...) van en user-secrets (`UserSecretsId` en `KNote.Server.csproj`) o variables de entorno
(`RepositoryRef__Orm`, `Repositories__0__Orm`, ...).
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
  (Sqlite temporal por instancia, configuración por variables de entorno `RepositoryRef__*`; con
  `repositoryCount: 2`, `Repositories__0__*`/`Repositories__1__*`, cada variante borra las de la otra, y
  `CreateServiceForRepository(i)` da un servicio directo sobre la base de datos de cada uno, p. ej. para crear los
  usuarios del segundo). Es donde se añaden los tests de un endpoint nuevo. Antes de crear el cliente, `KNoteWebApplicationFactory.AppConfiguration`
  sobrescribe configuración leída con `IOptions` (p. ej. `ai:providers`) y `TestServices` sustituye servicios
  (ver `InProcessTestHost.CreateAuthenticatedClientAsync(configure)`). `AiAssistantInProcessTests` usa
  `Helpers/ScriptedAiChatClientProvider`: un modelo con respuestas guionizadas (texto, uso, llamadas a tools)
  envuelto con las tools y la invocación de funciones reales, sin llamar a ningún proveedor.
  `ChatRoomHubInProcessTests` usa el cliente de SignalR contra el host (`HttpMessageHandlerFactory` del
  `TestServer`, LongPolling) con usuarios Staff y Guest creados por la API. `PasswordResetInProcessTests` cambia
  `IEmailSender` por `Helpers/FakeEmailSender` (guarda los correos; `WaitForAsync`, porque se envían en segundo plano);
  los límites se prueban con un host por test (`PasswordResetRestrictionsInProcessTests`), ya que cuentan todas las
  peticiones de un host. La fábrica guarda los recursos de las
  notas en una carpeta temporal propia (`ResourcesFolder`, `ResourcesContainer`, `ResourcesRootUrl`): sin ella se
  escribirían en el `wwwroot` del proyecto `Server`.
- `Tests/WebApiIntegrationTests` — contra un `Server` real ya en ejecución.

## Añadir un endpoint

1. La operación existe en `Service` (si no, añádela allí, con su `[KntAuthorize]` y su fila en
   `Tests/ServiceTests/CommandAuthorizationMatrixTests`).
2. Acción en el controlador con la plantilla de arriba y su `[Authorize(Roles = ...)]` alineado con el comando.
3. Test en `Tests/InProcessIntegrationTests`.
4. Método en el `I*WebApiService` correspondiente de `Client` (ver `Client/CLAUDE.md`).
