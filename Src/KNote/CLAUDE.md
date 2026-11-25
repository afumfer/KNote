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
  Folder, KAttribute, NoteType, TraceNoteType, SystemValues, User, AiSession, ChatRoom, SemanticSearch, AtlasContext).
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
- **Fecha de modificación de una nota** (KNote Atlas, etapa 2, D6; la usan el filtro «Modified between» y los resúmenes
  de un periodo): solo la cambian los datos propios de la nota (tema, texto, etiquetas, tipo, carpeta, atributos), igual
  en la Web, `ClientWin` y el asistente. Moverla y cambiar sus etiquetas (`PatchFolderAsync`/`PatchChangeTagsAsync`) la
  cambian; sus recursos, tareas, alarmas, relaciones, su ventana de post-it y lo que escribe la IA en los recursos, no.
  Lo decide un solo punto, `KntNotesSaveAsyncCommand` (por donde guardan una nota existente las tres), **comparando con lo
  guardado** (`Model/Core/NoteChangeRules`: sin valor = vacío, CR LF = LF, sin los espacios del final, como `SameText` de
  la Web; el estado, `InternalTags`, no cuenta), no con las marcas `IsDirty` del editor, que se activan con valores que
  nadie cambió; si conserva la fecha, `UpdateAsync(..., keepModificationDate: true)`. El log de Trace dice qué campo la
  cambió. `IsDirty()` sigue decidiendo **si** se guarda. Crear una nota conserva las fechas que trae (las dos ORM). Los
  editores cargan la nota con `SetEmptyTextsForEditing` antes de `SetIsDirty(false)`; el editor de `ClientWin` no
  devuelve el estado al modelo. Tests en `Tests/NoteChangeRulesTests`, `Tests/RepositoryParityTests/NoteDatesParityTests`,
  `Tests/ServiceTests/NoteModificationDateServiceTests` y `ClientWin.Tests/NoteEditorsModificationDateTests`.
- **Historial de las notas** (especificación en `C:\Dev\KNote-Atlas\03-Historial.md`; entrega H.1: las notas, sin UI). Una sola
  tabla, `KLogs` (revisión 11 del esquema): una fila por cambio o por **sesión** (el mismo usuario, nota y origen en 10 minutos
  actualizan la última fila: el autoguardado deja una, `HistoryRules.CanGroup`), con quién (`UserName`), desde dónde (`Origin`,
  `EnumHistoryOrigin`), qué partes (`Fields`), el detalle (`ChangeDetails`, `NoteChangeDetailsDto`) y la línea legible
  (`RegistryMessage`, en inglés, `Model/Core/NoteChangeDescriber`, el texto comparado por líneas con `LineDiff`). Si el cambio tocó los
  datos propios de la nota (los mismos que deciden su fecha de modificación), la fila guarda **cómo estaba antes** (`PreviousContent`,
  `NoteVersionDto` en JSON: la versión), **fuera del modelo de EF** como `ExtractedText`: solo la leen `IKntHistoryRepository.GetPreviousContentAsync`
  y el escritor de cada ORM. **Un único punto**: `Service/Core/KntHistory`, que llaman los cinco comandos que escriben una nota (guardar,
  borrar, `PatchFolder`, `PatchChangeTags`); compara con lo guardado y da al repositorio un `KLogWriteDto`, que este escribe **en la misma
  transacción** que la nota y sus atributos (H10): si no se puede anotar, no se guarda nada. Esas transacciones son **explícitas de la
  conexión** (`BeginTransaction`): en Sqlite un `TransactionScope` no protege nada (sus conexiones no se enlistan; comprobado). Las fechas
  del historial van al milisegundo (`HistoryRules.Timestamp`; Dapper las envía como `datetime2`). El origen lo pone el host
  (`IKntService.HistoryOrigin`: `ServiceRef` Desktop, el `Server` Web, sus trabajos KNote) y lo cambia un ámbito (`KntHistoryOrigin.Begin`:
  el ejecutor de acciones y `create_task` del asistente, KntScript, el broker, los trabajos de `ClientWin`). Borrar guarda la última
  versión (la papelera, `DeletedNotesDays`); después del cambio, sin hacerlo fallar, la retención de esa nota (`HistoryRules.CopiesToEmpty`)
  y la papelera (`KntHistory.PurgeDeletedNotesAsync`, también al abrir el repositorio: `ServiceRef` y `KntRepositories.OpenAsync`).
  Retención: `SYSTEM/HISTORY_RETENTION` (`HistoryRetentionDto`, valores por defecto si no existe); `SYSTEM/HISTORY_START`, el día que
  empezó. Nada de las notas que escribe KNote (`KntConst.HistoryExcludedNoteTypeNames`). Tests en `Tests/LineDiffTests`,
  `NoteChangeDescriberTests`, `HistoryRulesTests`, `RepositoryParityTests/HistoryParityTests`, `SchemaUpdateTests/SchemaUpdaterRevision11Tests`
  y `ServiceTests/NoteHistoryServiceTests`.
  **Las partes de una nota** (H.2a: tareas, recursos, alarmas, relaciones) son filas **de su nota** (H3), en la de su sesión, sin versión
  (no son datos propios): `Model/Core/NotePartChangeDescriber` (qué cambió, con la descripción anterior de una tarea hasta 4 KB; cómo se
  suman en una sesión, `MergeList`; añadido y quitado en la misma sesión no queda) y `NoteChangeDetailsDto.Tasks/Resources/Alarms/Relations`.
  Los comandos de cada parte (guardar y borrar) comparan con lo guardado y pasan la fila al repositorio, que la escribe en la transacción de
  la parte (`ExecuteNotedAsync` en Dapper, `NotedAsync` en EF); una relación, una fila en cada nota. Un recurso «sustituido» es uno que llega
  con un contenido distinto del guardado (los editores lo reenvían en cada guardado). **No se anotan**: lo que escribe la IA en los recursos
  (otros métodos del repositorio), el orden de un recurso, las relaciones `@DerivedFrom`, las ventanas, el estado de la nota, y lo que hace
  KNote solo dentro de `KntHistory.NotNoted()` (el correo de una alarma que no se pudo enviar, en `KntAlarms` y `KNoteManagementCtrl`).
  Borrar una nota con sus partes (`KntHistory.DeletingNote`, en `KntNotesDeleteExtendedAsyncCommand`) no las anota sueltas: la fila
  `Deleted` dice cuántas tenía y cada nota relacionada anota que perdió la relación (H14). Un comando nuevo que cambie una parte tiene que
  pasar su fila. Tests en `Tests/NotePartChangeDescriberTests` y `ServiceTests/NotePartsHistoryServiceTests`.
  **La estructura del repositorio** (H.2b): carpetas, tipos de nota, atributos y sus valores de lista, tipos de relación, usuarios y
  ajustes (`EntityName` = la entidad; los ajustes, `SystemValue` con un id fijo por ajuste, `HistoryRules.SettingId`), una fila por sesión,
  sin versiones: `Model/Core/StructureChangeDescriber` y `StructureChangeDetailsDto`. Los atributos se anotan en su transacción (Dapper y
  EF, ya sin `TransactionScope`, que en Sqlite no guardaba juntos el atributo y sus valores); el resto **justo después**
  (`KntHistory.NoteStructureAsync`, `IKntHistoryRepository.WriteAsync`): si no se puede anotar, el cambio se queda y el error va al log
  (H10). Nunca una contraseña ni un hash (`StructureChangeDescriber.Password` solo dice cómo cambió); el correo de un usuario y el valor de
  un ajuste genérico, sin sus valores (H15). El registro y «Olvidé mi contraseña», sin usuario en el servicio, se anotan con el del propio
  usuario. Lo que crea `KntSystemNotes` lleva el origen `KNote` (H16). No se anotan el orden de las notas de una carpeta (H17) ni los
  estados que KNote escribe solo. Un comando nuevo que cambie la estructura tiene que anotarlo. Tests en `Tests/StructureChangeDescriberTests`
  y `ServiceTests/StructureHistoryServiceTests`.
- **Ruta de una carpeta** (`IKntFolderService.GetPathAsync`, p. ej. `Home\Proyectos\KNote`): la usan
  `ClientWin` (`Store.GetKNoteFolerPath`) y la Web (`api/folders/{id}/path`).
- **Nombre de un recurso**: único en su nota (índice único `NoteId + Name`). `KntNotesSaveResourceAsyncCommand`, por donde
  pasan todos los guardados de recursos, lo comprueba antes de escribir el fichero (`IKntNoteRepository.GetResourceIdByNameAsync`,
  Dapper y EF con paridad en `Tests/RepositoryParityTests/ResourceNameParityTests`) y devuelve un `Result` no válido con el texto
  de `ResourceFileRules.DuplicateNameError` (el nombre como lo ve el usuario, `NameOut`): sin eso, el fichero del otro recurso se
  sobrescribía y el usuario recibía el error de la BD. El índice sigue como última defensa. Tests en
  `Tests/ServiceTests/NoteResourceFilesServiceTests`.
- **Contenido de los recursos de una nota**: `GetResourcesAsync` (y con él `GetExtendedAsync`) da los recursos **sin el contenido de
  sus ficheros** (`UtilManageResourceContent(r, readContent: false)`); los guardados en la BD (`ContentInDB`) lo traen con su fila, como
  siempre. Antes leía cada fichero entero al cargar una nota, de forma síncrona: con la carpeta de recursos en una red lenta, `ClientWin`
  no respondía al arrancar (1,6 MB en 38 s, la primera nota de la última carpeta). Quien necesita el contenido lo lee: un recurso suelto
  (`GetResourceAsync`, también lo que usa la IA), el editor de recursos y «Save as» (el fichero), los informes (la ruta del fichero) y la
  fusión por el *message broker* (`KntNotesSaveExtendedAsyncCommand` lo lee antes de publicar). Guardar un recurso sin contenido conserva su
  fichero. Un uso nuevo del contenido de los recursos de una nota tiene que leerlo él. Tests en `Tests/ServiceTests/NoteResourceFilesServiceTests`.
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
    10 s, nada si hay otra ejecución en marcha) es Guest; no se hace si la búsqueda solo se vuelve a mostrar
    (`SemanticSearchRequestDto.BringIndexUpToDate = false`, ClientWin al volver a la pestaña *Search/Filter*). Una nota que falla
    se reintenta en la siguiente ejecución: las puestas al día rápidas la dejan tras `MaxFailuresPerProcess` (3) fallos en el
    proceso (y el aviso de la búsqueda la cuenta en `NotesForFullRun`); *crear / actualizar* y *rehacer* la intentan siempre
    (`Options.RetryFailedNotes`) y dan su error: antes «Update now» se las saltaba en silencio («Done: 0 notes» con pendientes).
    Ollama pide los vectores con un `HttpClient` de 10 minutos (`AiEmbeddingGeneratorFactory.OllamaTimeout`): con los 100 s por
    defecto, un lote de 32 fragmentos sin GPU fallaba y se perdían los ya calculados de la nota. Los vectores de una nota se piden en lotes de 32 fragmentos
    (`EmbeddingBatchSize`: una nota de cientos en una llamada superaba el tiempo máximo del proveedor) y sus filas se
    sustituyen una vez al final; las puestas al día rápidas (antes de una búsqueda y de las parecidas) dejan para la
    indexación completa las notas con más de 64 fragmentos por calcular (`QuickRefreshMaxChunks`). Estado en `AI_INDEX_STATE` (`KntSemanticIndexState`). El
    índice contiene **todas** las notas salvo las de los tipos del sistema (`KntConst.SystemNoteTypeNames`:
    sesiones, chat room, consultas de prueba, informes): los permisos se aplican al buscar. Los comandos que cambian una
    nota (guardar, carpeta, etiquetas, recursos, tareas) y los que cambian su fragmento de datos sin tocarla (renombrar
    o mover una carpeta, renombrar un tipo o un atributo) llaman a `MarkStaleAsync`; un fallo al marcar no hace fallar
    el guardado. **Un comando nuevo que cambie lo que se indexa de una nota tiene que marcarla.**
  - **Búsqueda** (`SearchAsync`): **Palabras y Significado son excluyentes**; la de palabras no cambia (código,
    paginación y API). Por significado, sin paginar: las 100 notas más parecidas desde 0,45, de más a menos parecida
    (`SemanticSearchRules`, `Model/Core`), entre todas o entre las del filtro (`NotesFilterDto.MeaningText`, «Talks
    about…»), cruzadas con las que da el filtro de siempre leído por el servicio. Los vectores se leen de
    `KntSemanticIndexCache` (uno por base de datos y clave del índice en el proceso, con delta por marca de agua y 5
    minutos de margen); `KntSemanticSearcher` es lo común de la búsqueda y de las parecidas. El vector del texto buscado se
    guarda por generador (`KntQueryVectorCache`, los últimos 32 textos): repetir la búsqueda no llama al modelo. El resultado
    (`NoteSearchHitDto : NoteMinimalDto`, con parecido y fragmento) lo muestran las listas de siempre; `NoteListSorter`
    ordena en memoria como el repositorio. Sin modelo, sin su generador o sin índice, un error claro: una búsqueda no
    crea el índice.
  - **Buscar fotos y documentos** (KNote Atlas, etapa 4, D14): `SemanticSearchRequestDto.ResourceKind` (`All`, `Photos`, `Documents`).
    Con `All`, las notas de siempre, y el fragmento de un recurso dice cuál y su página (`NoteSearchHitDto.FragmentResource`/`FragmentPage`,
    `FragmentShown()`). Con los otros, `SemanticSearchRules.RankResources` (el mejor fragmento propio de cada recurso: descripción o texto)
    y `SemanticSearchResultDto.Resources` (`ResourceSearchHitDto : NoteSearchHitDto`, con el `ResourceInfoDto` sin el fichero, para que las
    listas de notas lo muestren como una fila más). La caché de vectores lleva `SourceId`. Nunca los recursos de las notas del sistema.
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
  sin usuario, permitido. Los tres tipos son del sistema (`KntConst.SystemNoteTypeNames`). Tests en
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
- **Acciones del asistente (KNote Atlas, etapa 1)** (`Ai/KNoteAiActionTools.cs`, `Model/Core/AiActionRules`,
  `Service/Core/KntAiActionExecutor`). `create_note` se hace al momento; las demás (`append_to_note`, `replace_note_text`, `move_note`, `set_tags`, `add_task`,
  `add_alarm`, `add_email_alarm`, `set_attribute`, `relate_notes`, `create_folder`, `process_document`) solo se proponen: la
  herramienta añade un `AiActionDto` a `TurnActions` y le dice al modelo que está pendiente. El turno lleva sus acciones
  (`AiChatTurnDto.Actions`) y la app muestra la tarjeta: lo aceptado lo ejecuta `KNoteAiTools.ExecuteActionsAsync`, no el
  modelo, con los comandos de siempre y los roles del usuario (el `Server` pone `EnforceAuthorization` en `chat` y `actions`).
  El ejecutor vuelve a mirar la nota: no se fía de lo que llega. Un tipo aprobado para la conversación
  (`AutoApprovedActionKinds`) y un cambio en una nota creada en ella se hacen al momento. `BeginTurn` recibe eso en cada turno:
  el `Server` no guarda nada entre peticiones. **El historial que recibe el modelo lleva cada acción como lo que fue: la
  llamada a la herramienta y su resultado** (`BuildMessages`). Con solo el texto de las respuestas, `qwen3.5:9b` aprendía que
  proponer era escribirlo y dejaba de llamar a las herramientas (1 de 9; con las llamadas, 25 de 27). Una línea de estado en
  sus respuestas la copiaba tal cual. Que no pregunte lo que ya pregunta la tarjeta se le dice en el resultado de la herramienta,
  no en las reglas fijas: ahí se lee antes de llamar y hablaba de la tarjeta sin llamar a la herramienta (2 de 9) o daba lo
  propuesto por hecho. Lo mismo a la inversa: lo hecho sin tarjeta le dice en el resultado que nada espera, porque el historial
  le enseña a decir «pendiente en la tarjeta» tras un `Done:` (3 de 10). Y copia las llamadas que ve: el historial lleva las
  etiquetas de `set_tags`, que sin ellas lo llamaba sin etiquetas. Una acción repetida (`AiActionRules.IsRepeated`) no se hace: los modelos pequeños repiten llamadas, y
  con el tipo aprobado el texto se añadía varias veces sin preguntar. La casilla de la tarjeta solo aprueba los tipos aceptados
  en ella y lo dice (`DontAskAgainText`). Nivel C (`create_folder`, `add_email_alarm`, `replace_note_text`): tarjeta también en una nota
  nueva (`NeedsCard`). `replace_note_text` es lo único que pierde texto (las notas no guardan versiones): el texto nuevo va solo
  en la tarjeta (`CardText`; `Text` es una línea, porque se guarda y va al modelo en cada turno) y lleva un sello del texto al
  proponerlo (`TextStamp`): si la nota cambió mientras la tarjeta esperaba, no se sustituye. `open_note_editor` no es una acción
  (no está en `Kinds`, no cambia nada): `IKNoteAiToolsHost.OpenNoteEditor` abre el editor en `ClientWin` y, en la Web, el evento
  `openNote`. `create_note` no abre el editor (`OnNoteCreated(openEditor: false)`): con la nota abierta, lo siguiente que le pide
  el modelo fallaría. `create_task` sí lo abre. `MinRoleOf`: `create_folder` pide ProjectManager, pero la herramienta se ofrece desde Staff para que el modelo
  explique por qué no (sin ella, uno pequeño dice que la creó). Tras una tarea o una alarma el ejecutor guarda la nota: su
  estado se calcula al guardar. Las alarmas son del usuario que las ejecuta y su tipo lo fija la herramienta: nunca un
  script. Una acción nueva necesita su caso en el ejecutor, su `KindName` y entrar en `Kinds`. Se guarda en el comentario oculto de la conversación sin los datos para ejecutarla, y
  lo propuesto sin responder se lee como `NotConfirmed`. Tests en `Tests/AiActionRulesTests`,
  `Tests/ServiceTests/AiActionToolsTests` y `Tests/InProcessIntegrationTests/AiActionsInProcessTests`.
  **`process_document`** (KNote Atlas 2.1b2b, D22; `Ai/KNoteAiProcessDocumentTool.cs`, `Model/Core/AiDocumentProcessRules`): el resumen
  extendido o la traducción de un documento de una nota. **Siempre con tarjeta**, también con su tipo aprobado, y sin «Don't ask again»
  (`AiActionRules.CanBeApproved`/`CanDontAskAgain`/`ApprovedKinds`, que usan las dos apps): tarda y, con un modelo de pago, cuesta. La
  tarjeta lleva la estimación (`Content`, `CardText`) con el modelo que da el host (`IKNoteAiToolsHost.GetDocumentModelAsync`: el LLM
  recomendado del repositorio, como la ventana); solo documentos de notas (un adjunto de la conversación, primero `attach_file`); una
  traducción sin terminar a ese idioma sigue (`ContinueNoteId`). No pasa por `KntAiActionExecutor`: aceptada,
  `IKNoteAiToolsHost.StartDocumentProcessAsync` la lanza en segundo plano (el `Server`, en `KntAtlasJobs` para el usuario de la petición;
  `ClientWin`, `Store.DocumentProcessing`) y al modelo se le dice que solo empezó (`AiActionRules.Outcome`/`Line`). Solo se ofrece si el
  host puede (`CanProcessDocuments`). Tests en `Tests/ServiceTests/AiProcessDocumentToolTests`, `InProcessIntegrationTests/SummaryInProcessTests`
  y `ClientWin.Tests/KNoteAiToolsHostDocumentTests`.
- **Imágenes y archivos del asistente (KNote Atlas, etapa 1)** (`IKntAiSessionService.AddAttachmentAsync`/`GetAttachmentAsync`,
  `Model/Core/AiAttachmentRules`, `Ai/AiTurnAttachments`, `Ai/AiImageReducer`, `Ai/AiModelCapabilities`). Al enviar, cada archivo se
  guarda como recurso de la nota de la sesión (la crea si no existe) y `AiChatRequestDto` solo lleva su referencia; `AiTurnAttachments`
  lee las imágenes, las reduce (lado mayor 1.600 px, EXIF aplicado) y `AiChatTurnStreamer` las pone como `DataContent` **solo en ese
  turno**. Del historial el modelo solo recibe los nombres (`AiChatTurnDto.Attachments`, guardados en el comentario oculto). **Las
  imágenes no se muestran por la URL del recurso**: se sirven sin sesión y su fichero puede estar en otra instalación; cada app pide
  una vista reducida que comprueba al dueño (`GetAttachmentAsync`) y la dibuja como `data:`. `AiModelCapabilities`: Ollama lo dice en
  `/api/show`; OpenAI y Anthropic se dan por buenos. SkiaSharp va en `KNote.Ai`, y `Directory.Build.targets` quita sus símbolos y los
  sistemas que KNote no usa (sin eso, el `Server` publicado pesa casi el triple); una versión nueva hay que probarla también
  publicada. `attach_file` copia un adjunto a una nota, leyéndolo por la sesión (nivel B; solo se ofrece con adjuntos). `view_note_images`
  mira las imágenes de una nota o de la conversación, 3 por llamada y 9 por turno (`AiToolImages`): no van en el resultado de la herramienta,
  que cada proveedor trata a su manera (Ollama lo pasaría a texto), sino en un mensaje detrás de él que añade `AiToolImagesChatClient`,
  debajo de la invocación de funciones. Tests en `Tests/AiAttachmentRulesTests`, `AiImageReducerTests`, `ServiceTests/AiSessionsServiceTests`,
  `InProcessIntegrationTests/AiActionsInProcessTests` y `ClientWin.Tests/KNoteAIAssistantCtrlAttachmentsTests`.
- **Fotos y documentos (KNote Atlas, etapa 4)** (especificación en `C:\Dev\KNote-Atlas\03-Etapa-4.md`). **Lo que escribe una persona
  nunca lo pisa la IA, y describir o leer un recurso nunca cambia la fecha de modificación de la nota.**
  - **Esquema**: revisión 9, `Resources.DescriptionByUser` (bloqueada) y `DescriptionModel` (el modelo que la escribió); revisión 10,
    `Resources.ExtractedText`/`ExtractedState` y `NoteChunks.Page`. `ExtractedText` **no está en el modelo de EF ni en las consultas de
    recursos de siempre** (se lee aparte: `GetResourceTextAsync`), para que no viaje en las listas.
  - **Reglas puras** (`Model/Core`): `ResourceDescriptionRules` (D12: la casilla manda; `ResourceInfoDto.LoadedDescription` en toda lectura
    del `Service`, así que guardar un recurso sin tocar su texto no pisa lo que la IA escribió mientras), `AiPhotoRules`/`AiDocumentRules`
    (qué se describe o resume y qué se hace con las descripciones de personas sin bloquear), `DocumentTextRules` (formatos y el formato de
    `ExtractedText`: páginas separadas por `\f`, `\u0001` pendiente de transcribir, `\u0002` transcrita por la IA; sin letras ni números,
    para que buscar no los encuentre), `AiTranscriptionRules`, `ResourceProcessingRules` (el lote y su estimación), `ResourceFileRules`
    (`KntConst.MaxLenNoteResourceFile`, 200 MB; la Web sube en streaming) y los textos comunes de las dos apps (`DocumentTexts`,
    `ResourceProcessingTexts`).
  - **`KNote.Ai`**: `AiDocumentReader` (`IKntDocumentReader`: PdfPig para el texto, PDFtoImage/PDFium para dibujar páginas, un lector propio
    de DOCX y ODT; sin IA, solo marca las páginas escaneadas) y las tareas sin herramientas (`AiResourceTask`, D13): `AiPhotoDescriber`,
    `AiDocumentSummarizer`, `AiPageTranscriber`. `AiResourceProcessor` es el lote («Process pending resources»: fotos, documentos por leer,
    páginas escaneadas y resúmenes), el mismo en el `Server` (`KntAtlasJobs`, un trabajo a la vez con el índice) y en `ClientWin`
    (`ResourceProcessingJob`); al terminar pone al día el índice.
  - **`Service`**: `IKntService.ResourceContent` (`KntResourceContentCommands.cs`; leer y dibujar, Guest; guardar descripciones, textos y
    transcripciones, Staff; preparar el lote, Admin), con escrituras condicionales (si el recurso cambió, no se guarda) y `MarkStaleAsync`
    de su nota. El host pone `IKntService.DocumentReader` como `EmbeddingGenerators`. Los recursos de las notas del sistema quedan fuera
    de todo. El indexador lee los documentos sin leer (un fichero que no está en esta instalación se cuenta, no es un error) y
    `NoteChunkBuilder` hace un fragmento `ResourceText` por página o grupo de páginas, con su `Page`.
  - **Búsqueda por palabras** (D18): `SearchInResources`/`SearchInNoteTasks` en `NotesSearchDto` y `NotesFilterDto` (`LIKE` sobre la
    descripción y el texto de los recursos, también al contar; `ResourcesExcludedNoteTypeIds` lo pone el `Service`), en las dos apps.
  - Tests: reglas en `Tests/*Rules*Tests`, servicio en `Tests/ServiceTests/Resource*`, `AiPhotoDescriber*`, `AiDocument*`, `AiPage*`,
    API en `Tests/InProcessIntegrationTests/ResourceContentInProcessTests` y `ResourceUploadInProcessTests`; con modelos reales y medidas, ver `ClientWin.Tests/CLAUDE.md`.
- **El asistente lee documentos (KNote Atlas, etapa 4, D14)** (`Ai/KNoteAiDocumentTools.cs`, `Model/Core/AiDocumentToolRules`): solo
  lectura. `read_document` da el texto de un documento de una nota (`IKntResourceContentService.GetDocumentToReadAsync`: el guardado
  o, sin leer, leído al vuelo **sin guardarlo**) o de la conversación (leído del adjunto con `DocumentReader`), por páginas con su
  marca (transcrita, escaneada sin transcribir) y cortado en una página a los 40.000 caracteres, con `fromPage`/`toPage` para seguir.
  `view_document_pages` dibuja páginas de un PDF (`DrawDocumentPageAsync`) como imágenes del turno, como `view_note_images` (y,
  como ella, solo con un modelo que ve imágenes). `get_note_items` dice de cada documento si se leyó, sus páginas y las escaneadas
  sin transcribir (`GetNoteDocumentsAsync`). Las notas del sistema, fuera. Tests en `Tests/AiDocumentToolRulesTests`,
  `ServiceTests/AiDocumentToolsTests` y el smoke `ServiceTests/AiDocumentToolsSmokeTests` (`RequiresRealAiProvider`, con
  `KNOTE_RESOURCE_SMOKE_MODEL`: responde leyendo un PDF y no obedece la orden escrita en otro).
- **Razonamiento del modelo (KNote Atlas, etapa 1)**: `AiChatTurnStreamer` separa `TextReasoningContent` en eventos
  `reasoning` y da el texto, el tiempo y los tokens (si el proveedor los distingue; Ollama no, y no se estiman) en el turno.
  `AiChatTurnDto.Reasoning` va con `[JsonIgnore]`: no se guarda ni viaja con el historial, y la Web lo monta con los eventos.
  Solo `ReasoningTokens` queda en el comentario oculto de la conversación. Ollama no necesita `think`: los modelos que
  razonan lo envían aparte por su cuenta. `AiChatClientFactory` no pide nada a ningún proveedor: OpenAI responde 400 a
  `reasoning.summary` con los modelos que no razonan (`gpt-4o-mini`, comprobado con una llamada real), así que con OpenAI
  no hay texto de razonamiento, solo sus tokens.
  Tests en `ClientWin.Tests/AiReasoningTests`.
- **Búsqueda en Internet (KNote Atlas, etapa 1)** (`Ai/AiWebSearch.cs`, `Model/Core/AiWebSearchRules`). Solo en un turno con
  «Search the Internet» encendido (`AiChatRequestDto.WebSearch`, `KNoteAIAssistantCtrl.WebSearch`; empieza apagado en cada
  conversación y no se guarda) y con un proveedor que puede (`AiProviderRef.SupportsWebSearch`: OpenAI, Anthropic, u Ollama con
  `WebSearchApiKey`, un secreto como `ApiKey`). `AiWebSearch.ToolsFor` da `HostedWebSearchTool` a OpenAI y Anthropic (sus adaptadores
  la convierten en la búsqueda del proveedor) y a Ollama `web_search`/`web_fetch` sobre la API de ollama.com (`OllamaWebSearch`, que
  nunca lanza: dice al modelo qué falló); el host las añade a las del turno y `AiWebSearch.SystemPrompt` añade
  al mensaje de sistema `AiWebSearchRules.SystemRules` o, apagada o sin poder, `SystemRulesOff`/`SystemRulesUnavailable` (sin ellas,
  el modelo respondía de memoria como si hubiera buscado). `AiChatTurnStreamer` da un evento `tool` `web_search` también con la búsqueda
  del proveedor (`WebSearchToolCallContent`) y añade al final de la respuesta, como «Sources:», las citas (`CitationAnnotation`) que
  no están ya en el texto (`SourcesSection`). Tests en `Tests/AiWebSearchTests`, `InProcessIntegrationTests/WebSearchInProcessTests` y
  los `WebSearch_*` de `RequiresRealAiProvider`.
- **Resumir o traducir un documento largo (KNote Atlas, etapa 2, CU-43)** (`IKntService.Summaries`, comandos en `KntSummaryCommands.cs`;
  especificación en `C:\Dev\KNote-Atlas\03-Etapa-2.md`, D11, D17, D18, D21, § 5.1 y § 5.3). **Nombres en la interfaz** (2.1b2): el
  «resumen corto» (*Short summary*) es la descripción de un documento que escribe la IA (etapa 4, tarea `SummarizeDocument`); el
  «resumen extendido» (*Extended summary*), el de todo el documento en una nota nueva (esta). Solo cambian los textos: el código y
  las claves siguen igual, y una tarea renombrada guarda su nombre anterior en `AiTask.FormerNames` (el atributo «Task» de las notas
  de instrucciones guarda el nombre, y `AiTaskCatalog.Find` lo encuentra). **El motor** es `Ai/AiSummaryEngine`,
  genérico sobre piezas (`Model/Core/SummaryPieces`: las páginas de un PDF con su etiqueta «p. N», o trozos por párrafos sin
  ella): lotes que caben en el contexto (`SummaryBatches`, unos 4 caracteres por token), apuntes de cada lote, «resúmenes de
  resúmenes» mientras no caben juntos y la redacción final, con las tareas sin herramientas de la etapa 4 (`AiResourceTask`) y la
  tarea `SummarizeLongDocument`; la 2.5 lo usará con notas sin cambiarlo. **KNote fija el contexto** de esas llamadas
  (`Ai/AiSummaryContext`: el «Context size» del proveedor o min(`/api/show`, 16.384) en Ollama; 32.000 en OpenAI y Anthropic).
  `Ai/AiDocumentProcessor` estima (`SummaryEstimate`, con el ritmo por modelo de `AI_SUMMARY_STATE`) y resume: el resultado va a
  una **nota nueva** `@KNoteSummary` (no del sistema, D5) en la carpeta de la nota del documento, relacionada con ella
  (`@DerivedFrom`, de la nota nueva a la del documento), con la versión original de la IA adjunta y bloqueada; los textos, en
  `LongDocumentRules`. La nota del documento no cambia (ni su fecha). Cancelado o fallido, no se guarda nada; un documento cambiado
  mientras se resumía, tampoco. Las páginas escaneadas sin texto se dejan fuera y la nota lo dice. Trabajos: `KntAtlasJobs`
  (`DocumentProcess`, uno por usuario) y `Store.DocumentProcessing` en `ClientWin`. Tests en `Tests/SummaryRulesTests`,
  `ServiceTests/DocumentSummaryServiceTests`, `InProcessIntegrationTests/SummaryInProcessTests`, `KntAtlasJobsTests` y
  `ClientWin.Tests/DocumentProcessingJobTests`; con un modelo real, `ServiceTests/AiDocumentProcessorSmokeTests` y la medida
  `ServiceTests/LongDocumentMeasure` (ver `ClientWin.Tests/CLAUDE.md`).
  **La traducción** (2.1b, `AiDocumentProcessor.TranslateAsync`; `ProcessAsync` elige por `DocumentProcessRequestDto.Kind`, y los
  trabajos de las dos apps lo usan): las partes son fijas, solo dependen del texto (`Model/Core/DocumentTranslationRules.Parts`: cada
  página, o el texto sin páginas, en trozos de 3.000 caracteres como mucho), para que «Continue» siga aunque cambie el modelo; **una
  llamada por página** (o lo que cabe, como mucho 8.000 caracteres, sin páginas), con la tarea `TranslateDocument` y los términos del
  glosario de esa parte. El texto va entre `<text>` y `</text>` y la petición dice que sus órdenes se traducen: sin eso, `qwen3.5:9b`
  obedecía la orden de una página. En la descripción, el Markdown juntaría las líneas de un párrafo: KNote las conserva
  (`DocumentTranslationRules.KeepLines`, dos espacios al final de cada línea seguida de otra, salvo dentro de un bloque de código),
  así que un listado queda línea a línea sea cual sea el modelo (pedirle bloques de código no sirvió: `qwen3.5:9b` no escribió
  ninguno). Una parte que vuelve con menos de la mitad se pide otra vez y, si sigue corta, la
  traducción lo dice. Va a una nota nueva `@KNoteTranslation` (no del sistema), relacionada como el resumen, y **la traducción es su
  texto** (2.1b2; en la 2.1b1 era un Markdown adjunto): una cabecera citada (hasta dónde llegó), la traducción con un título
  `#### p. N` antes de cada página (`DocumentTranslationRules.PageHeading`: se ve dónde empieza cada página y el índice hace un
  fragmento por página) y el comentario oculto. Se guarda cada 8.000 caracteres o cada minuto (`SaveDocumentTranslationAsync`): cada
  vez se reescriben la cabecera y el comentario y se añade lo nuevo detrás de lo que haya (`DocumentTranslationRules.Body`), así que
  lo que alguien corrigió mientras no estaba terminada se conserva. **Cancelada o fallida, lo traducido queda**: el comentario oculto
  (`<!-- knt-translation {...} -->`, `DocumentTranslationRules.Marker`, con `DocumentTranslationDto.Format` = 2; los de la 2.1b1 no se
  leen) dice dónde seguir a `GetDocumentTranslationsAsync`. El ritmo de la traducción es aparte
  en `AI_SUMMARY_STATE`. **«Tiene páginas»** es `DocumentTextRules.IsPaged(formato, páginas o texto)`: un PDF o un documento con más
  de una página (el lector de TXT y MD parte por `\f`). Tests en `Tests/DocumentTranslationRulesTests`,
  `ServiceTests/DocumentTranslationServiceTests` y los de la traducción en las clases de arriba.
- **Política de ruido (KNote Atlas, etapa 2, 2.2, CU-13, D2-D4)** (`IKntSummaryService.GetNoisePolicyAsync`, Guest;
  `SaveNoisePolicyAsync`, ProjectManager; `CountNoiseAsync`, Guest; comandos en `KntSummaryCommands.cs`): los tipos de nota y las
  carpetas cuyas notas no entran nunca en los resúmenes de notas. Se guarda en `SystemValues` (ámbito `AI`, clave `AI_NOISE_POLICY`,
  protegida como las demás de ese ámbito) como un JSON con los ids (`NoisePolicyDto`); al leerla y al guardarla se quitan los que ya
  no existen y los de KNote (`Service/Core/KntNoisePolicy`). **Cada carpeta se excluye sola**, sin sus subcarpetas, y Home es una más
  (no hay bandeja de entrada). La regla es pura, `Model/Core/NoiseRules.Classify`, en este orden: las notas de KNote (tipos de
  `KntConst.SystemNoteTypeNames` más `@KNoteSummary` y `@KNoteTranslation`, `NoiseRules.KNoteNoteTypeNames`, y las carpetas de
  `SystemFolderNames`) → la etiqueta `#NoKnowledge` (siempre fuera) → `#Knowledge` (siempre dentro, gana a la política) → tipo
  excluido → carpeta excluida. Las etiquetas (`KntConst.NoKnowledgeTag`/`KnowledgeTag`) se buscan enteras, sin mayúsculas, entre
  espacios, comas o puntos y comas (`NoiseRules.HasTag`). El recuento de una política sin guardar (`NoiseRules.Count`, por motivo)
  lee todas las notas con una consulta ligera, `IKntNoteRepository.GetClassificationsAsync` (id, número, carpeta, tipo y etiquetas;
  Dapper y EF, con paridad en `Tests/RepositoryParityTests/NoteClassificationsParityTests`). Guardarla no cambia ninguna nota. Tests en
  `Tests/NoiseRulesTests`, `ServiceTests/NoisePolicyServiceTests` y `InProcessIntegrationTests/SummaryInProcessTests`.
- **Selección de un resumen (KNote Atlas, etapa 2, 2.3, CU-15, D6-D10, S1-S7)** (`IKntSummaryService.PrepareSelectionAsync` y
  `SaveRelevanceCorrectionsAsync`, Staff; `Service/Core/KntNoteSelection`), sin IA y sin escribir nada: las notas **modificadas en el
  periodo** (días enteros) **o con una tarea que empezó** (`StartDate` o, sin ella, su creación) **o terminó en él**
  (`IKntNoteRepository.GetSelectionSourcesAsync`, con su texto y cuántas tareas, con actividad, recursos y relaciones en los dos sentidos;
  Dapper y EF con paridad en `Tests/RepositoryParityTests/NoteSelectionSourcesParityTests`) y, con «y», de una carpeta y sus
  subcarpetas, con palabras clave (la búsqueda por palabras en el tema y el texto) y «Talks about…» (por significado entre las del
  periodo, desde 0,45, sin el tope de 100). Cada nota la decide `Model/Core/NoteRelevanceRules` (In / Doubtful / Out, con su motivo
  en palabras y una puntuación que ordena las listas), la primera que decide: las notas de KNote (solo se cuentan, `KNoteNotes`) →
  la corrección de una persona → la política de ruido y las etiquetas (`NoiseRules`) → las señales (tareas con actividad, citas `#n`
  desde otras notas de la selección, relaciones, recursos y el tamaño del texto limpio con `Model/NoteReadingCleaner`: menos de 10
  palabras y creada y dejada el mismo día, fuera; menos de 40, dudosa). `ComputedDecision` es lo que decide KNote sin la corrección:
  `NoteRelevanceRules.CorrectionFor` da lo que guarda «Review selection» al cambiar una casilla (o quitar la corrección).
  **Las correcciones van en la nota del sistema «Note relevance»** (`@KNoteRelevance`, en «KNote Atlas», `Model/Core/RelevanceNote`: dos
  tablas legibles y una marca oculta por fila, `<!-- knt-relevance {...} -->`, de la que se leen; `Service/Core/KntRelevanceNotes`):
  una por repositorio, compartida, la última vale; escrituras una a una por base de datos y releyendo la nota; al escribirla se quitan
  las filas de notas borradas. La selección da su referencia (`NoteSelectionResultDto.RelevanceNote`, el enlace de la sección; nula
  hasta que existe: leer no la crea). Es una nota protegida (`KntAtlasNotes.ProtectedNoteTypeNames`): a mano, solo ProjectManager y Admin;
  el comando la escribe para un Staff dentro de `KntAuthorizationBypass`, **que también salta esa regla de guardado**
  (`KntAtlasNotes.CanWriteAsync`). Las personas nunca tocan sus notas al corregir. Tests en `Tests/NoteReadingCleanerTests`,
  `NoteRelevanceRulesTests`, `RelevanceNoteTests`, `ServiceTests/NoteSelectionServiceTests` y `InProcessIntegrationTests/SummaryInProcessTests`.
- **Las dudosas con IA (KNote Atlas, etapa 2, 2.4, D9, C1-C6)**: a petición, nunca al preparar la selección. `Model/Core/NoteClassificationRules`
  elige las dudosas por sus señales (las de más puntuación, 100 como mucho por vez; las demás siguen dudosas, y las dudosas entran), las
  parte en lotes de 10, arma la petición (cada nota entre `<note>` y `</note>`, con número, tema, carpeta, tipo, etiquetas y el comienzo
  de su texto limpio, 1.500 caracteres) y lee la respuesta línea a línea (`#123 | In | motivo`; una nota sin línea legible sigue dudosa).
  `Ai/AiNoteClassifier.ClassifyAsync` hace un lote con la tarea `ClassifyNote` (`AiTaskCatalog`, sin herramientas, `AiResourceTask`):
  pide las notas a `IKntSummaryService.GetNotesToClassifyAsync` y guarda las respuestas con `SaveRelevanceAiAnswersAsync` (los dos Staff;
  las respuestas van a la tabla «AI classification» de «Note relevance», con la huella y el modelo, dentro de `KntAuthorizationBypass`).
  **La huella** (`NoteRelevanceRules.ContentHash`: el tema y el texto limpio, sin los saltos de línea ni los espacios de los extremos) la
  calcula igual la selección: una respuesta con otra huella no vale y la nota vuelve a ser dudosa; la siguiente escritura de «Note
  relevance» la quita (`KntRelevanceNotes`, como las filas de notas borradas), así la tabla solo tiene respuestas válidas. La respuesta decide solo una dudosa
  por sus señales, por debajo de una corrección y de la política (`NoteRelevanceRules.Decide`, `EnumSelectionSource.Ai`). Tests en
  `Tests/NoteClassificationRulesTests`, `ServiceTests/NoteClassificationServiceTests`, `InProcessIntegrationTests/SummaryInProcessTests`
  y el smoke `ServiceTests/AiNoteClassifierSmokeTests` (`RequiresRealAiProvider`, `KNOTE_RESOURCE_SMOKE_MODEL`).
- **El resumen de un periodo (KNote Atlas, etapa 2, 2.5, CU-16, CU-17, P1-P7)**: **un tipo más del trabajo de los resúmenes**
  (`EnumDocumentProcessKind.PeriodSummary`, `DocumentProcessRequestDto.Period`): la misma cola del `Server` (`KntAtlasJobs`,
  `DocumentProcess`, uno por usuario: un resumen de un documento o de un periodo), el mismo trabajo de `ClientWin`, el mismo progreso
  y la misma cancelación, y las mismas rutas (`api/summaries/documents/...`). `Ai/AiPeriodSummarizer` (desde `AiDocumentProcessor`):
  `IKntSummaryService.PreparePeriodAsync` (Staff: la selección y, de cada nota que entra, su ruta, su texto limpio, sus tareas con
  actividad en el periodo y las descripciones de sus recursos, sin el texto de sus documentos), las dudosas que queden preguntadas antes
  a la IA (fase `Classifying`, con el tope de 100; sus respuestas se guardan aunque luego se cancele), las notas como piezas `#123`
  ordenadas por carpeta y fecha (`Model/Core/PeriodSummaryRules`) leídas por `AiSummaryEngine` con **una tarea**, `SummarizePeriod`
  (como el documento largo: lotes, uniones y redacción), de 100 a 10.000 palabras, y `SavePeriodSummaryAsync` (Staff): una nota
  `@KNoteSummary` en la carpeta «KNote Summary» (`KntConst.SummaryFolderName`, no del sistema); el ritmo por modelo en
  `PeriodSecondsPer1000Characters`. Las notas del periodo no cambian. Sin notas que entren, un error claro (`PeriodSummaryRules.NoNotes`).
  **Por apartados, siempre** (2.6, D13; `Model/Core/PeriodSummarySections`): las notas por carpeta hasta el segundo nivel de su ruta, como
  mucho 8 (los más pequeños, en «Other folders»), con las palabras repartidas por sus notas (el ejecutivo, el 10 %; los pendientes, el 15 %)
  y uno de más de 2.500 palabras partido por fecha; cada apartado es una ejecución de `AiSummaryEngine.RunAsync` con sus piezas y el
  resumen ejecutivo y los pendientes, dos llamadas con los apuntes de todos (`AiSummaryEngine.JoinAsync`/`AskAsync`, separados de
  `RunAsync` sin cambiar lo que hace). **El documento** lo monta `Model/Core/PeriodSummaryDocument` en el orden del capítulo 35, con
  títulos en inglés: datos (con llamadas, tiempo, citas no reconocidas y los atributos de «By concept»), ejecutivo (IA), índice por
  carpetas y por conceptos, lo ocurrido (IA), pendientes (IA), estadísticas por carpeta (tareas con actividad:
  empezadas, terminadas, abiertas al final, tiempos; `PeriodSummaryRules.SetTaskCounts`), incluidas y omitidas con su motivo. **«By concept»**
  (`Model/Core/PeriodConceptRules`): los atributos que elige el usuario (`PeriodSummaryRequestDto.ConceptAttributeIds`; el selector los muestra
  como «Tipo de nota · Atributo», `PeriodConceptRules.Choice.Label`, con `-` si no son de un tipo; la columna es el nombre, `Columns`: dos
  elegidos del mismo nombre en tipos distintos son una columna; solo los que agrupan, `IsGrouper`: texto, valor tabulado, etiquetas, sí/no y entero,
  nunca fechas, decimales ni textos largos; los deshabilitados no), **una tabla por atributo** (`Tables`) con una fila por valor, sus notas,
  sus tareas con actividad (empezadas / terminadas / abiertas) y sus citas, de más a menos notas, 30 como mucho (el resto, «other values»),
  y debajo las notas sin valor de ese atributo. En cada tabla, cada nota está en una fila (un `TagsValue` cuenta como el conjunto ordenado
  de sus valores), así que sus filas suman las notas con valor. Sin atributos, «No attributes chosen.». Los valores los lee `PreparePeriodAsync` de una vez
  (`IKntNoteRepository.GetAttributeValuesAsync`, Dapper y EF con paridad en `Tests/RepositoryParityTests/NoteAttributeValuesParityTests`) en
  `PeriodNoteDto.Attributes`, y la IA también los lee (una línea `Attributes:` en `PeriodSummaryRules.PieceText`; la tarea le dice que no
  cuente notas ni tareas). El glosario no está en el índice: la IA lo recibe en el mensaje de sistema, como siempre. **Todos los campos de
  «Period summary»** (la selección y el resumen) se guardan en cada cambio por usuario y repositorio (`PeriodSummaryParametersDto`: ClientWin
  en `AppUserState.Session.PeriodSummaries`, la Web en el navegador) y vuelven al abrir la sección. El comando
  lo calcula con `PeriodSummarySaveDto.Prepared` (las partes de la IA van aparte: `Executive`, `Sections`, `Pending`). **Citas**
  (`Model/Core/SummaryCitationRules`, D16): la de una nota no leída queda sin enlace (un `⁠` entre `#` y el número) y se cuenta.
  **Relaciones** `@DerivedFrom` hacia cada nota leída que sigue existiendo y no es de KNote (el comando no se fía de quien llama), con
  `Order` (el de su primera cita, las demás detrás) y `Weight` (su puntuación). El original adjunto, las partes tal como las escribió la
  IA. Tests en `Tests/PeriodSummaryRulesTests`, `PeriodSummaryDocumentTests`, `PeriodConceptRulesTests`, `ServiceTests/PeriodSummaryServiceTests`,
  `InProcessIntegrationTests/SummaryInProcessTests`, el smoke `ServiceTests/PeriodSummarySmokeTests` y la medida
  `ServiceTests/PeriodSummaryMeasure` (ver `ClientWin.Tests/CLAUDE.md`).
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
  room), `AlarmsController` (avisos de alarmas de la Web), `ReportsController` (informes imprimibles y CSV de las listas de notas, con `KNote.Reports`), `SemanticSearchController` (índice y búsqueda por significado), `AtlasController` (contexto de la IA), `ServerController` (capacidades del servidor) y `LocalController` (API del equipo de KNote Local), además del scaffold
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
- **Saltos de línea del texto de la IA**: lo que KNote guarda escrito por la IA (resumen extendido, traducción, resumen de un
  periodo y sus `.md` originales, `create_note`/`append_to_note`/`replace_note_text`/`create_task`, tareas y alarmas del asistente)
  va con CR LF, como las notas escritas en los editores de Windows: `Model/Core/TextLineEndings.ToCrLf`, en el punto de guardado del
  `Service`. No usa `ReplaceLineEndings`, que también cambiaría el `\f` que separa las páginas de un documento. Un caso de uso
  nuevo que guarde texto de la IA tiene que pasarlo por ahí.
- Cadenas localizadas: `Docs/Manual.md` (inglés) y `Docs/Manual_es.md` (español) son el manual de usuario —
  actualiza ambos si cambia el comportamiento de cara al usuario, siguiendo la convención bilingüe ya
  existente en este repo.
