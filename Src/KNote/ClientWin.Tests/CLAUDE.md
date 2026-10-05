# CLAUDE.md

Guía para trabajar en `ClientWin.Tests` (MSTest, parte de `KNote.slnx`, referencia `ClientWin.csproj`
directamente). Ver también `ClientWin/CLAUDE.md` para la arquitectura del proyecto bajo test.

## Convención general: fakes hechos a mano, no mocking frameworks

No hay Moq/NSubstitute en este proyecto. Cada interfaz que hace falta test-doblar tiene una clase
`internal class FakeXxx : IXxx` en `Fakes/`, con:

- **Delegados `Func<...>` asignables** (`...Impl`) solo para los miembros que un test concreto necesita.
- **Todo lo demás lanza `NotSupportedException`** — así un test que toca sin querer un miembro no
  configurado falla alto y claro, en vez de devolver silenciosamente un valor por defecto.

Ejemplo (`Fakes/FakeKntNoteService.cs`):
```csharp
public Func<Guid, Task<Result<NoteExtendedDto>>>? GetExtendedAsyncImpl { get; set; }
public Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId) =>
    (GetExtendedAsyncImpl ?? throw new NotSupportedException($"{nameof(GetExtendedAsync)} not configured for this test"))(noteId);
```

`Fakes/FakeKntService.cs` compone los fakes por dominio (`NotesFake`, `UsersFake`, ...) tras las
propiedades de `IKntService`; `Fakes/TestFactoryViews.cs` es un `IFactoryViews` vacío que cada test rellena
con los `IView*` fake que necesite. Para instanciar un `*Ctrl` bajo test: `Store` real + `TestFactoryViews`
+ vista fake registrada + `IKntService` (normalmente `FakeKntService`) pasado explícitamente al método que
lo pida (`LoadModelById(service, id)`, `NewModel(service)`, ...) — nunca un `IKntService` ambiente.

Al añadir un miembro nuevo a un fake existente, sigue el mismo patrón (delegado opcional + throw por
defecto) en vez de sustituir el `throw new NotSupportedException()` por una implementación fija.

## Tests y autorización (roles)

Un `Store` recién creado no tiene ningún rol (`Store.Security` vacío: rol de aplicación Guest y ninguno en
ningún repositorio), así que una controladora con `[KntAuthorize]`, la ejecución de scripts
(`Store.CanRunScripts`) o el modo consulta de `NoteEditorCtrl` se comportan como para un Guest. En los tests
que **no** van de seguridad, entra como Admin con `TestStoreFactory.GrantAdmin(store, servicios...)` (Admin
en esos `IKntService`; sin argumentos, en uno ficticio, suficiente para el rol de aplicación). En los que sí,
fija el rol justo con `store.Security.SetRepositoryRole(service, rol)`.

`Store.AccessDeniedNotifier` es `null` fuera de la app (lo conecta `Program`), así que una denegación nunca
abre un `MessageBox` que bloquee la ejecución de los tests; para comprobar el aviso, captúralo:
`store.AccessDeniedNotifier = notified.Add`.

Las matrices de roles se comprueban por reflexión: `ControllerAuthorizationMatrixTests` (controladoras de
`ClientWin`) y, en `Tests/`, `CommandAuthorizationMatrixTests` (comandos del Service). Un caso de uso o un
comando nuevo las hace fallar hasta que se decide su rol en ellas.

## Tests de configuración (`AppConfigStorage`, migración, secretos)

- `Fixtures/KNoteData.v1.config` es un `KNoteData.config` del formato antiguo con **datos sintéticos** (nunca
  copies aquí el fichero real de un usuario: contiene secretos). Se copia al directorio de salida
  (`KNote.ClientWin.Tests.csproj`) y lo usan `AppConfigMigratorTests`, `AppConfigStorageTests` y
  `StoreConfigPersistenceTests`.
- Los tests que escriben ficheros de configuración usan siempre un directorio temporal propio; **nunca** el
  `%LocalAppData%\KNote` real (por eso los tests de `KntServerCOMCtrl`/`KNoteAIAssistantCtrl` no llaman a
  `SaveConfig()`).
- `Fakes/FakeSecretProtector` sustituye a DPAPI en `AppConfigStorageTests`/`AppUserSettingsSecretsTests`; los
  tests de `DpapiSecretProtector` y de `StoreConfigPersistenceTests` usan DPAPI real (solo Windows).

## Tests de IA (`KNoteAIAssistant`)

Dos capas con propósitos distintos — **capa 1 no requiere nada especial y corre siempre**; **capa 2
requiere ApiKeys reales y no corre por defecto** (ver más abajo cómo configurarlas).

### Capa 1 — unitarios, sin red, siempre corren

- `Fakes/FakeChatClient.cs` — fake de `IChatClient` (`Microsoft.Extensions.AI`), mismo patrón que el
  resto de `Fakes/` (`GetResponseImpl`/`GetStreamingResponseImpl` configurables).
- `AiChatClientFactoryTests.cs` — `ResolveApiKey` (precedencia `AiProviderRef.ApiKey` > variable de
  entorno), `Create` para los 3 proveedores (sin red real: la construcción del cliente es perezosa).
- `KNoteAiToolsTests.cs` — `search_notes`/`get_note_details`/`create_task` contra
  `FakeKntService`/`FakeKntNoteService` (sin base de datos real). Como esos métodos son `private` en
  `KNoteAiTools` (solo pensados para llegar a través del `AITool` que construye `AIFunctionFactory.Create`
  en `GetTools()`), los tests los invocan por reflexión en vez de ensanchar su visibilidad solo para
  testear. `create_task` persiste vía `Service.Notes.NewExtendedAsync()`/`SaveExtendedAsync(...)` — capa
  Service pura, igual que las otras dos tools — así que su lógica de negocio (Topic/Description/FolderId
  correctos, propagación de errores de `NewExtendedAsync`/`SaveExtendedAsync`) está totalmente cubierta
  con fakes; solo su cola final (abrir la nota ya guardada con `NoteEditorCtrl.LoadModelById`+`.Run()`,
  que muestra una `Form` real) queda sin cubrir aquí — no hay forma de automatizar esa UI desde este
  proyecto de test; verifícalo a mano si tocas esa parte.
  Para simular `Store.DefaultFolderWithServiceRef.ServiceRef.Service` como un fake sin abrir una base de
  datos real, usa `TestServiceRefFactory.CreateWithFakeService(fakeService)` — explota que
  `ServiceRef._service` es un campo público (no solo la propiedad `Service`), así que se puede sustituir
  el `KntService` real que el propio constructor de `ServiceRef` ya construyó de forma perezosa.
- `KNoteAIAssistantCtrlTests.cs` — `RestartAIAssistant`, y sobre todo el **rollback de turno huérfano**: si
  `IChatClient` lanza una excepción, `GetCompletionAsync`/`StreamCompletionAsync` no deben dejar un mensaje
  de usuario sin respuesta en `ChatMessages`/`ChatTextMessages` (bug real que se coló y arregló en
  `KNoteAIAssistantCtrl` — este test evita que vuelva). Usa
  `KNoteAIAssistantCtrl.SetChatClientForTesting(chatClient, providerRef)` — un seam `internal` que
  bypassa `AiChatClientFactory` — habilitado por
  `[assembly: InternalsVisibleTo("KNote.ClientWin.Tests")]` en `ClientWin/Properties/AssemblyInfo.cs`.

Corren con cualquier `dotnet test`, sin configuración adicional:
```powershell
dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj --filter "TestCategory!=RequiresRealAiProvider"
```

### Capa 2 — smoke tests con proveedores reales (`[TestCategory("RequiresRealAiProvider")]`)

`OpenAiProviderSmokeTests.cs`, `AnthropicProviderSmokeTests.cs`, `OllamaProviderSmokeTests.cs`: hacen
**llamadas HTTP reales** a través del mismo camino que usa producción (`AiChatClientFactory.Create`), para
detectar roturas de comportamiento en tiempo de ejecución que un `dotnet build` no puede ver — por ejemplo,
un paquete NuGet (`OpenAI`, `Anthropic`, `OllamaSharp`, `Microsoft.Extensions.AI*`) que cambia de versión y
rompe la llamada real, aunque el código siga compilando sin problema. Por proveedor, 3 tests: completion,
streaming, y un round-trip de function-calling contra `search_notes` (la tool que ya lleva incluida
`AiChatClientFactory.Create`).

**Por qué existen — precedente real**: esta suite ya encontró y ayudó a corregir dos bugs reales de
producción en `AiChatClientFactory` el mismo día en que se escribió, cuando OpenAI se llamaba por Chat
Completions (`/v1/chat/completions`):
1. Los modelos de razonamiento de OpenAI (`o1`/`o3`/`o4`/familias `gpt-5.x`, `gpt-6`...) rechazan
   function tools en `/v1/chat/completions` salvo que `reasoning_effort` sea explícitamente `"none"`
   (HTTP 400 `invalid_request_error`). Omitirlo no basta: estos modelos usan `medium` por defecto.
2. Los modelos NO razonadores (`gpt-4o`, `gpt-4o-mini`, ...) rechazan el propio parámetro
   `reasoning_effort` como argumento no reconocido (un HTTP 400 *distinto*) — forzarlo siempre para
   `OpenAI`, como se hizo al arreglar (1), rompía estos modelos.

Se resolvió primero con una heurística por nombre de modelo que enviaba `"none"` solo a los modelos de
razonamiento, pero `gpt-6-astra` rechaza también `"none"` (solo admite `low`/`medium`/`high`/`xhigh`), así
que con tools no tiene ningún valor válido en Chat Completions. La solución definitiva fue pasar OpenAI a
la **Responses API** (`/v1/responses`, `OpenAI.Responses.ResponsesClient`), que acepta tools con cualquier
esfuerzo de razonamiento: ya no se envía `reasoning_effort` (cada modelo usa su valor por defecto) y no
hace falta heurística. Se envía `store=false` (`StoredOutputEnabled = false`) para que OpenAI no guarde
en su servidor las respuestas, que llevan contenido de las notas — igual que en Chat Completions.
Verificado con llamadas reales (`gpt-6-astra`, `gpt-5.6-terra`, `gpt-4o-mini`): varios turnos con tools,
con y sin streaming. Al actualizar `OpenAI`/`Microsoft.Extensions.AI.OpenAI`, pasa esta suite con un
modelo de razonamiento y con uno que no lo sea (`AiProviderSmokeTests:OpenAI:Model`) — la API de
Responses del SDK aún está marcada como experimental (`OPENAI001`) y puede cambiar.

**No corren por defecto** (cuestan tokens/dinero y dependen de red):
```powershell
dotnet test ClientWin.Tests/KNote.ClientWin.Tests.csproj --filter "TestCategory=RequiresRealAiProvider"
```
Ejecútalos explícitamente **después de subir de versión** `OpenAI`, `Anthropic`, `OllamaSharp` o
`Microsoft.Extensions.AI`/`Microsoft.Extensions.AI.OpenAI` en `ClientWin/KNote.ClientWin.csproj`, antes de
dar el bump por bueno.

## Cómo configurar las ApiKeys para correr la Capa 2

`Helpers/AiTestConfig.cs` resuelve cada proveedor con la misma precedencia que usa producción
(`AiChatClientFactory.ResolveApiKey`): **primero el fichero de configuración/user-secrets, luego la
variable de entorno**. Si un proveedor no tiene ni una cosa ni la otra, ese test se marca
**Inconclusive** ("Omitido" en la salida de `dotnet test`) con un mensaje explicando qué falta — nunca
falla ni pasa en falso por ausencia de credenciales.

**Opción A — variables de entorno** (más simple; si ya las usas para probar `KNoteAIAssistant` a mano en
`ClientWin`, los tests las reutilizan sin nada más que hacer):
```powershell
$env:OPENAI_API_KEY = "sk-..."
$env:ANTHROPIC_API_KEY = "sk-ant-..."
```
Solo para la sesión de PowerShell actual. Para dejarlas permanentes en tu usuario de Windows:
```powershell
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "sk-...", "User")
[Environment]::SetEnvironmentVariable("ANTHROPIC_API_KEY", "sk-ant-...", "User")
```
(Ollama no usa ApiKey — ver más abajo.)

**Opción B — user-secrets de este proyecto** (persistente, nunca se escribe en el repo — mismo mecanismo
que ya usa `Tests/`, ver `CLAUDE.md` raíz):
```powershell
dotnet user-secrets set "AiProviderSmokeTests:OpenAI:ApiKey" "sk-..." --project ClientWin.Tests/KNote.ClientWin.Tests.csproj
dotnet user-secrets set "AiProviderSmokeTests:Anthropic:ApiKey" "sk-ant-..." --project ClientWin.Tests/KNote.ClientWin.Tests.csproj
```
Opcionalmente, también el modelo a usar en las pruebas (si no se indica, `OpenAI` usa `gpt-4o-mini` y
`Anthropic` usa `claude-haiku-4-5`):
```powershell
dotnet user-secrets set "AiProviderSmokeTests:OpenAI:Model" "gpt-4o-mini" --project ClientWin.Tests/KNote.ClientWin.Tests.csproj
```
Si configuras ambas opciones a la vez, gana el user-secret/`appsettings.json` sobre la variable de entorno
(mismo orden que en producción).

**Ollama** no usa ApiKey — usa un `Host` (servidor local o remoto). Por defecto
`http://localhost:11434`; para cambiarlo:
```powershell
$env:OLLAMA_HOST = "http://mi-servidor:11434"
# o, vía user-secrets:
dotnet user-secrets set "AiProviderSmokeTests:Ollama:Host" "http://mi-servidor:11434" --project ClientWin.Tests/KNote.ClientWin.Tests.csproj
```
Antes de llamar de verdad, el test comprueba que el host responde (`GET {host}/api/tags`, timeout 2s); si
no responde, se marca Inconclusive en vez de fallar — distingue "Ollama no instalado/parado" de una
regresión real. El test de function-calling además depende de que el modelo configurado soporte tools
(no todos los modelos de Ollama lo hacen — `llama3.1`/`qwen2.5`/`gemma4` sí, por ejemplo); si solo ese test
falla, prueba con otro modelo antes de asumir que la integración está rota.

Observado en la práctica: la primera llamada a un modelo que Ollama aún no tiene cargado en memoria puede
tardar mucho y, ocasionalmente, devolver un 500 puntual mientras carga — si `Completion_ReturnsNonEmptyResponse`
falla en solitario pero `Streaming_ReturnsNonEmptyResponse`/`ToolCalling_...` (que usan el mismo cliente)
pasan, es casi seguro ese arranque en frío, no una regresión; relánzalo antes de investigar más.

**Verificar la configuración**: `dotnet user-secrets list --project ClientWin.Tests/KNote.ClientWin.Tests.csproj`
lista lo que hay guardado (nunca se muestra en claro en la salida de los tests ni se comitea — `appsettings.json`
del proyecto solo tiene placeholders vacíos, igual que `Tests/appsettings.json`).
