# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this project
(`Client`, el frontend Blazor WebAssembly de KNote). Ver también el `CLAUDE.md` de la raíz del repo para el
contexto general de la solución, y `Server/CLAUDE.md` para la API REST con la que habla este proyecto.

## Idea central de la arquitectura

La versión Web de KNote son dos proyectos: `Client` (Blazor WebAssembly, la UI, se ejecuta en el navegador)
y `Server` (ASP.NET Core: API REST + hub SignalR, y además hospeda los ficheros compilados de `Client`).

Sigue **el mismo diseño que `ClientWin`**: la interfaz de usuario está desacoplada de la lógica de negocio,
que vive en la capa `Service` y en el modelo de objetos **SmartDTO** (`Model/Dto`), ambos compartidos por los
dos desarrollos. La diferencia es dónde corre `Service`:

- `ClientWin` llama a `Service` **en el mismo proceso** (`Store` → `ServiceRef` → `IKntService`).
- `Client` **no referencia** `Service` ni `Repository` (solo `Model`): llega a la misma capa `Service` a
  través de HTTP, contra los controladores de `Server`, que son una capa fina sobre `IKntService`.

```
Página Blazor (Pages/Notes/NoteEdit.razor)
  → IStore.Notes (INoteWebApiService / NoteWebApiService)      ← proxy HTTP, devuelve Result<T>
    → HTTP  api/notes  (JWT bearer)
      → NotesController (Server)                              ← sin lógica de negocio
        → IKntService.Notes → comando de Service/ServicesCommands
          → IKntRepository (Dapper / EF)
```

Equivalencias con `ClientWin`, útiles para orientarse y para mantener el mismo diseño:

| `ClientWin`                                   | `Client`                                                        |
|-----------------------------------------------|-----------------------------------------------------------------|
| `Core/Store` (estado global + acceso a datos) | `AppStoreService/IStore` + `Store` (servicio `Scoped` de DI)     |
| `IKntService.Notes`, `.Folders`, ...          | `IStore.Notes`, `.Folders`, ... (`I*WebApiService`)             |
| `Store.Events` (bus de eventos)               | `AppState.OnChange` / `OnNotifyError` / `OnNotifySuccess`       |
| `Ctrl` del caso de uso                        | Página enrutable (`XxxEdit`, `XxxNew`, `XxxIndex`) y su `@code` |
| `IView*` + `Form`                             | Componente de presentación (`XxxForm`) con `[Parameter]` y `EventCallback` |
| SmartDTO (`Model/Dto`)                        | Los mismos SmartDTO, deserializados desde la API                |

Regla práctica: **las reglas de negocio no van en las páginas**. Si una página necesita decidir algo que no
es de presentación (qué se puede guardar, valores por defecto, numeración, permisos finos...), eso pertenece
a la capa `Service` (y se expone por `Server`) o a las validaciones del propio DTO, para que `ClientWin` y la
Web se comporten igual.

## Estructura de carpetas

```
Client/
├── Program.cs              – composition root: DI (HttpClient, IStore, auth JWT, servicios de Radzen)
├── App.razor               – Router + AuthorizeRouteView (layout por defecto: MainLayout)
├── _Imports.razor          – usings globales de los .razor (Radzen, Radzen.Blazor, KNote.Model.Dto, ...)
├── AppStoreService/
│     IStore.cs, Store.cs   – estado global + acceso a los servicios Web API + conexión SignalR (chat)
│     AppState.cs           – estado de la app (usuario, carpeta seleccionada, árbol de carpetas, chat) y eventos
│     ClientDataServices/
│         Base/BaseService.cs   – manejo común de respuestas HTTP → Result<T> + notificaciones
│         Interfaces/, Services/ – un I*WebApiService + implementación por objeto de dominio
├── Auth/                   – AuthenticationProviderJWT (AuthenticationStateProvider + ILoginService)
├── Pages/                  – páginas por área: Notes/, Folders/, Types/, Attributes/, Users/, Auth/, Lab/
├── Shared/                 – layout (MainLayout*) y componentes reutilizables (EntityList, ToolingHeader,
│                             KntIndexHeader, KntFolderSelector, KntFoldersTreeView, InputMarkdown, ...)
├── Helpers/                – extensiones de IJSRuntime (localStorage) y NavigationManager (query strings)
└── wwwroot/                – index.html, css (app.css, Bootstrap heredado), js/filePaste.js
```

## Acceso a datos: `IStore` y los `I*WebApiService`

- `IStore` (registrado `Scoped`; en WASM equivale a una instancia por pestaña) expone `Users`, `NoteTypes`,
  `KAttributes`, `Folders`, `Notes`, `ChatGPT` como propiedades perezosas, igual que `IKntService` en el
  backend. Las páginas inyectan `@inject IStore store` y no usan `HttpClient` directamente.
- Cada `XxxWebApiService` hereda de `BaseService` y sigue la nomenclatura de `Service`
  (`GetAllAsync`, `GetAsync`, `NewAsync`, `SaveAsync`, `DeleteAsync`...). `SaveAsync` decide POST/PUT según
  el id del DTO (`Guid.Empty` → POST).
- **Contrato de errores**: los controladores de `Server` devuelven siempre un cuerpo `Result<T>`, también con
  `400 BadRequest` cuando el servicio rechaza la operación. `BaseService.ProcessResultFromHttpResponse` lee ese
  cuerpo sea cual sea el código de estado, y si no es válido llama a `AppState.NotifyError`, que `MainLayout`
  muestra como notificación de Radzen. Por tanto **una página no debe volver a notificar el error** de una
  llamada a `store.Xxx`: basta con comprobar `result.IsValid`. Para operaciones de escritura se pasa
  `emitNotifySucess: true` y el aviso de éxito también sale solo. Los servicios nuevos heredan de
  `BaseService`.

## SmartDTO en la UI

Los DTO de `Model/Dto` heredan de `SmartModelDtoBase` (`Model/Core`): no son POCO, llevan estado y
validación que ambas UIs usan directamente:

- `IsDirty()`/`SetIsDirty(...)`, `IsNew()`/`SetIsNew(...)` (recorren también los hijos). Patrón habitual:
  tras cargar o guardar, `SetIsDirty(false)`; antes de guardar, `if (!dto.IsDirty()) return;`; y al salir,
  `<NavigationLock OnBeforeInternalNavigation=...>` pide confirmación si `IsDirty()` (ver `NoteEdit.razor`).
- Validación: DataAnnotations en las propiedades + `Validate(...)`/`IsValid()`/`GetErrorMessage()` de
  `ModelBase`. Los formularios usan `<DataAnnotationsValidator />`, así que las reglas viven en el DTO, no en
  la página.
- Cambiar `SmartModelDtoBase` o un DTO cambia el comportamiento de `ClientWin` también (y el JSON de la API).

## Interfaz de usuario: Radzen.Blazor

La librería de componentes de la UI es **Radzen.Blazor** (paquete `Radzen.Blazor`, ver versión en
`KNote.Client.csproj`), documentada en <https://blazor.radzen.com/>. **Úsala para toda UI nueva o que se
modifique**; consulta su documentación (y el changelog si se actualiza el paquete) antes de usar un componente
o parámetro que no aparezca ya en el código, en vez de suponer su API.

Cómo está integrada:

- Servicios en `Program.cs`: `DialogService`, `NotificationService`, `TooltipService`, `ContextMenuService`.
- Componentes anfitriones en `Shared/MainLayout.razor`: `<RadzenDialog/>`, `<RadzenNotification/>`,
  `<RadzenContextMenu/>`, `<RadzenTooltip/>`. Sin ellos los servicios anteriores no muestran nada.
- Layout: `RadzenLayout` + `RadzenHeader`/`RadzenSidebar`(`RadzenPanelMenu`)/`RadzenBody`/`RadzenFooter`.
- Tema y script en `wwwroot/index.html`: `_content/Radzen.Blazor/css/software.css` y
  `_content/Radzen.Blazor/Radzen.Blazor.js`. Los iconos de Radzen (`Icon="save"`, `"edit"`, `"delete"`...)
  son nombres de Material Symbols.

Patrones ya establecidos que hay que seguir:

- **Listas**: `RadzenDataGrid` (normalmente `Density.Compact`, `AllowColumnResize`) dentro de
  `Shared/EntityList` (que pinta "cargando"/"sin registros"); acciones por fila con `RadzenButton` +
  `tooltipService.Open(...)`. Cabecera de página con `KntIndexHeader` + `RadzenMenu`.
- **Diálogos** (alta/edición de entidades pequeñas, selectores, login): `dialogService.OpenAsync<Componente>(
  título, parámetros, new DialogOptions {...})`; el componente devuelve su resultado con
  `dialogService.Close(resultado)` (`null` = cancelado). Confirmaciones con `dialogService.Confirm(...)`.
  Ejemplos: `TypesIndex` → `TypeEdit`/`TypeNew`, `NoteForm` → `KntFolderSelector`/`NoteTaskEditor`.
- **Notificaciones**: para errores de la API ya salen solas (ver arriba); para validaciones de la propia
  página, `notificationService.Notify(new NotificationMessage { Severity = ..., ... })`.
- Otros en uso: `RadzenTabs`, `RadzenSplitter`, `RadzenTree`, `RadzenScheduler` (calendarios de tareas y
  alarmas), `RadzenDatePicker`, `RadzenCheckBoxList`, `RadzenPager`, `RadzenCard`.

**Código heredado (Bootstrap)**: parte de la UI es anterior a Radzen y usa Bootstrap 4
(`wwwroot/css/bootstrap`, clases `form-group`, `col-sm-*`, `float-right`, `btn`...), Font Awesome 4.7 por CDN y
open-iconic (`oi oi-*`), con `EditForm` + `InputText`/`InputNumber`/`InputSelect` y botones HTML (p. ej.
`NoteForm`, `Login`, `InputMarkdown`, `ToolingHeader`). Al modificar uno de esos componentes, pasa la parte
tocada a sus equivalentes de Radzen (`RadzenTextBox`, `RadzenNumeric`, `RadzenDropDown`, `RadzenFormField`,
`RadzenStack`/`RadzenRow`/`RadzenColumn`, `RadzenButton`...), manteniendo la validación por DataAnnotations
del DTO. No hagas migraciones masivas que no se hayan pedido.

## Páginas: organización y convenciones

- Por área, un trío de componentes: `XxxIndex` (lista), `XxxEdit`/`XxxNew` (caso de uso: cargan el DTO,
  guardan, navegan; son las equivalentes al `Ctrl`) y `XxxForm` (presentación: recibe el DTO por
  `[Parameter]` y avisa con `EventCallback` `OnValidSubmit`/`OnExit`...). `XxxEdit`/`XxxNew` pueden abrirse
  como página (`@page`) o como diálogo de Radzen (las de tipos, carpetas, atributos y usuarios se abren como
  diálogo desde su `Index`).
- **Rutas relativas**: la app se sirve bajo `/KNote/` (`<base href="/KNote/" />` en `index.html`, a juego con
  `app.UsePathBase("/KNote")` en `Server`). Navega con rutas sin `/` inicial (`store.NavigateTo("notes/tree")`,
  `Path="index"`), o se saldrá de la base.
- Estado compartido entre páginas en `store.AppState` (p. ej. `SelectedFolder`, que usan `NoteNew`/`NoteEdit`
  para preasignar carpeta). Un componente que se suscriba a `AppState.OnChange` implementa `IDisposable` y se
  desuscribe en `Dispose()`.
- `Nullable` está activado en este proyecto: `[Parameter] [EditorRequired] public NoteDto Note { get; set; }
  = null!;` para parámetros obligatorios.
- `Pages/Lab/` (`TestPage`, `FileUpload`) son páginas de pruebas, solo visibles para Admin.

## Autenticación y autorización

- Login/registro: `Shared/MainLayoutHeader` → `Pages/Auth/AdminLogin` abre `Login`/`Register` como diálogos,
  llama a `store.Users.LoginAsync`/`RegisterAsync` (`api/users/login|register`) y entrega el JWT a
  `ILoginService.Login(token)`.
- `Auth/AuthenticationProviderJWT` guarda el token en `localStorage` (clave `TOKENKEY`), lo pone como cabecera
  `Authorization: bearer` del `HttpClient` compartido y construye el `ClaimsPrincipal` leyendo los claims del
  JWT (nombre y roles; el rol del usuario lo decide `Service` al registrar, ver `CLAUDE.md` raíz).
- Las páginas declaran `@attribute [Authorize(Roles = "...")]` y el menú usa `<AuthorizeView Roles="...">`.
  Los roles de ASP.NET **no son jerárquicos**: enumera todos los que tienen acceso
  (`"Staff, ProjectManager, Admin"`), con los nombres de `EnumRoles` (`Guest`, `Staff`, `ProjectManager`,
  `Admin`).
- Esto solo decide qué se muestra: **la autorización real está en `Server`** (`[Authorize(Roles = ...)]` de
  cada acción). Mantén alineados los roles de la página con los de los endpoints que usa.

## Otros

- **Chat**: `Store` crea un `HubConnection` contra `chathub` (`Server/Hubs/ChatHub`); `MainLayout` lo arranca
  (`ChatStartAsync`) y los mensajes recibidos llegan a `AppState.ChatMessages`. `NotesChatGPT` usa
  `api/chatgpt` (OpenAI, en `Server`).
- **Markdown**: las descripciones de nota se editan con `Shared/InputMarkdown` y se muestran con
  `Shared/ViewMarkdown` (Markdig). `Server` reescribe las URLs de recursos de la descripción al leer/guardar
  para compatibilizarla con `ClientWin` (ver `Server/CLAUDE.md`).

## Compilar, ejecutar y depurar

`Client` no se ejecuta solo: lo hospeda `Server` (sin él no hay API). Arranca siempre `Server`:

- Visual Studio: `Server` como proyecto de inicio, perfil `KNote.Server` → `https://localhost:44339/KNote`.
  La depuración de los `.razor`/C# de `Client` funciona desde VS (el `inspectUri` está en
  `Server/Properties/launchSettings.json`).
- VS Code: configuración `KNote.Web (Server + Client WASM)` de `.vscode/launch.json` (tipo `dotnet`, requiere
  C# Dev Kit): arranca `Server` con su perfil de `launchSettings.json`, abre el navegador y depura a la vez
  `Server` y el código WASM de `Client`. La configuración `KNote.Server` (`coreclr`) solo depura `Server`.
- Limitación de Blazor WASM (también en VS): no se detienen los breakpoints en `Program.cs` ni en el
  `OnInitialized{Async}` de la primera página cargada, porque el proxy de depuración aún no está conectado.
- `dotnet watch run --project Server/KNote.Server.csproj` (tarea `watch: Server`) para iterar en la UI.

No hay tests de `Client`. La API que consume se prueba en `Tests/` (ver `Server/CLAUDE.md`).

## Añadir una funcionalidad a la Web

1. Si hace falta, DTO en `Model/Dto` y operación en `Service` (ver "Notas para editar" del `CLAUDE.md` raíz).
2. Endpoint en `Server/Controllers` con su `[Authorize(Roles = ...)]`.
3. Método en el `I*WebApiService` + implementación (heredando de `BaseService`); si es un objeto de dominio
   nuevo, su propiedad en `IStore`/`Store`.
4. Página(s) y componentes con Radzen en `Pages/<Área>/`, con su `[Authorize(Roles = ...)]`, y entrada en
   `Shared/MainLayoutNavMenu.razor` dentro del `<AuthorizeView>` correspondiente.
5. Si cambia el comportamiento de cara al usuario, `Docs/Manual.md` y `Docs/Manual_es.md`.
