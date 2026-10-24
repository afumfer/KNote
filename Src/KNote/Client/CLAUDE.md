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
│     IStore.cs, Store.cs   – estado global + acceso a los servicios Web API + el chat room (ChatRoomHub)
│     AppState.cs           – estado de la app (usuario, carpeta seleccionada, árbol de carpetas) y eventos
│     ChatRoomHub.cs        – conexión SignalR del chat room, hilos seguidos y sus mensajes, no leídos
│     KntWindows.cs         – ventanas de la app (notas, asistente de IA, chat room) y aviso entre ventanas de las notas guardadas
│     KntSingleFlight.cs, KntLatestRequest.cs – doble envío y peticiones viejas (ver "Peticiones: doble envío y cancelación")
│     ClientDataServices/
│         Base/BaseService.cs   – manejo común de respuestas HTTP → Result<T> + notificaciones
│         Interfaces/, Services/ – un I*WebApiService + implementación por objeto de dominio
├── Auth/                   – AuthenticationProviderJWT (AuthenticationStateProvider + ILoginService), KntSignIn (entrar/registrarse)
├── Pages/                  – páginas por área: Notes/, Folders/, Types/, TraceNoteTypes/, Attributes/, Users/, Auth/,
│                             AIAssistant/, ChatRoom/
├── Shared/                 – layouts (MainLayout*, KntWindowLayout) y componentes reutilizables (EntityList, ToolingHeader,
│                             KntIndexHeader, KntSections, KntDialogs, KntLogo, KntFolderSelector,
│                             KntFoldersTreeView, KntNoteSelector, KntNotePicker, KntDialogFrame, KntFormSection,
│                             KntField, KntMarkdownEditor, ...)
├── Helpers/                – extensiones de IJSRuntime (localStorage, carga de módulos JS) y NavigationManager (query strings)
└── wwwroot/                – index.html, css/app.css, css/knt-theme.css, js/ (filePaste.js, chatRoom.js...), favicon.ico
```

## Acceso a datos: `IStore` y los `I*WebApiService`

- `IStore` (registrado `Scoped`; en WASM equivale a una instancia por pestaña) expone `Users`, `NoteTypes`,
  `KAttributes`, `Folders`, `Notes`, `AiAssistant`, `SystemValues` como propiedades perezosas, igual que
  `IKntService` en el backend. Las páginas inyectan `@inject IStore store` y no usan `HttpClient` directamente.
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
- **Todas las peticiones pasan por `BaseService.SendAsync`**, que nunca lanza por la petición: sin respuesta del
  servidor (sin red, servidor parado, timeout del `HttpClient`) devuelve un `Result` no válido con el error, ya
  notificado; cancelada por quien llama, un `Result` no válido **sin** notificación (no es un error para el usuario).
  Todos los métodos de los `I*WebApiService` aceptan un `CancellationToken cancellationToken = default` como último
  parámetro (ver "Peticiones: doble envío y cancelación"). Un `Result` no válido puede ser, por tanto, un fallo de
  red o una cancelación: usa `Entity` solo dentro del `if (result.IsValid)`. Login y registro devuelven un
  `UserTokenDto`, no un `Result`: sin respuesta, `success = false` con el motivo. En el streaming del asistente de
  IA, un fallo de red (al empezar o a mitad de la respuesta) acaba en su evento `error`. `SendAsync` acepta además un
  `timeout` opcional: sin respuesta en ese tiempo, un `Result` no válido notificado ("did not respond in N seconds").
  Lo usan las lecturas de las listas de notas (`GetFilter`, `GetSearch`, `GetHomeNotesAsync`), del árbol de carpetas
  (`GetTreeAsync`) y de las carpetas de Home (`GetHomeChildrenAsync`), con
  `BaseService.ListReadTimeout` (60 s; el del `HttpClient` es de 100 s), para que una lista sin respuesta muestre su
  error y se pueda reintentar.

## Peticiones: doble envío y cancelación

Blazor vuelve a ejecutar un manejador `async` si el usuario pulsa otra vez (o Enter, o Supr) mientras espera al
servidor, y no cancela nada al cambiar de selección o salir de una página. De ahí tres reglas, que siguen todas las
páginas:

- **Escrituras (guardar, borrar, subir, exportar): una a la vez, con `KntSingleFlight`** (`AppStoreService`). El
  manejador es `RunAsync` del cuerpo de siempre (`private Task OnSave() => saving.RunAsync(SaveAsync);`); otra
  llamada mientras dura se ignora. Una guarda por grupo de operaciones que no deben solaparse (`NoteForm`: una para
  guardar la nota, en `NoteEdit`, y otra, `tabsSaving`, para las pestañas). Su `IsBusy` llega a los botones:
  `IsBusy` en el que guarda, `Disabled` en los demás y en Cancel/cerrar; los formularios de presentación lo reciben
  como parámetro (`Saving` en los `XxxForm` y `NoteForm`, `Busy` en `NoteKResources`/`NoteKTasks`/`NoteTraceNotes`).
  **Las escrituras no se cancelan nunca** (el servidor puede haberlas aplicado ya). Casos especiales: el orden de una
  carpeta (`NotesTree.OnOrderChanged`) se guarda en el orden en que se elige y solo el último pendiente; el guardado
  de la sesión del asistente de IA (`AIAssistant.SaveSessionAsync`) se comparte: quien lo pide mientras se guarda
  espera a ese guardado.
- **Diálogos: `DialogService.Close` cierra el diálogo de arriba, sea cual sea.** Un `Close` tardío (tras el `await`
  de un guardado o una carga, si el usuario ya cerró con Esc) cerraría otro diálogo (el padre, o una confirmación).
  Los `XxxNew`/`XxxEdit` llevan `closed = true` en `Dispose` y cierran con `CloseDialog(...)`, que no hace nada si ya
  se cerró; `OnExit` no hace nada mientras se guarda. Los selectores que se abren desde otro diálogo
  (`KntFolderSelector`, `KntNoteSelector`) se cierran una sola vez. Referencia: `Types/TypeEdit`.
- **Lecturas: gana la última, y nada sigue al salir.**
  - Cuando una petición nueva deja vieja a la anterior (otra selección, otra búsqueda, otra página, recargar tras un
    cambio), `KntLatestRequest`: `var r = await request.RunAsync(ct => store.Xxx.GetAsync(id, ct)); if (r.Canceled)
    return;` (cancelada: no se toca nada) y `r.Value` es el `Result`. Su `Dispose` (en el del componente) cancela la
    que esté en curso. Ejemplos: `NotePreviewPane`, `ViewMarkdown`, `NoteView`, las notas de `Index`, las listas de
    Admin, `KntNoteSelector`, la ruta de la carpeta en `NoteForm`.
  - Un componente con varias lecturas que solo deben pararse al salir: un `CancellationTokenSource disposal` que se
    cancela en `Dispose` y cuyo token se pasa a cada `store.Xxx`; si tras el `await` está cancelado, no se toca el
    estado compartido (`AppState`; ver `Index.LoadFolders`). Ejemplos: `NoteForm`, `NoteKAttributes`, `NoteEdit`.
  - Si la respuesta modifica un DTO que se está editando, se lee aparte y se aplica de una vez cuando llega, para que
    dos respuestas no se mezclen (`NoteForm.OnChangeNoteTypeActions` → `LoadNoteTypeAttributesAsync`).
  - `NotesGrid.LoadPage` recibe un `CancellationToken`: se cancelan los bloques de una versión vieja de la lista
    (recargada, ordenada) y al salir de la página.
  - Un estado que se resuelve fuera de una petición HTTP sigue la misma idea: `ChatRoomHub.SelectThreadAsync` deja el
    último hilo elegido (y sigue una sola vez el que se elige dos veces mientras se suscribe).

Los **módulos JS de la app** se cargan con `js.ImportModuleAsync("./js/xxx.js", store.AppState)` (`Helpers`), no con
`InvokeAsync("import", ...)`: el navegador los pide al servidor la primera vez que se usan y, si no responde, avisa y
devuelve `null` en vez de romper la página; el componente sigue sin lo que hace el módulo (la lista, sin teclado; el
chat, sin pegar imágenes...) y lo guarda con `??=`, así que lo vuelve a pedir la próxima vez.

## Cuenta del usuario y sesión

- **Menú del usuario** (`Pages/Auth/AdminLogin`, en la cabecera): `RadzenProfileMenu` (`.knt-user-menu`; en un móvil
  solo el icono) con **My profile** (`Auth/UserProfile`), **Change password** (`Auth/ChangePassword`: al terminar
  entra con el token nuevo que devuelve el servidor) y **Sign out**. Un Admin pone la contraseña de otro desde el
  editor de usuarios (`Users/UserSetPassword`, botón del pie con `.knt-footer-start`). La política
  (`KntPasswordPolicy`) se comprueba antes de enviar, también en el registro.
- **Olvidé mi contraseña**: el diálogo `Auth/Login` abre `Auth/ForgotPassword` encima (el login sigue abierto); el
  enlace del correo lleva a la página anónima `Auth/ResetPassword` (`/reset-password?token=`), que entra con el token
  devuelto.
- **Entrar y registrarse**: `Auth/KntSignIn` (servicio `Scoped`, de uno en uno) abre el diálogo, llama a `IStore.Users` e
  inicia la sesión. Lo usan la cabecera (`AdminLogin`, que luego va a Home) y la tarjeta de acceso, que se queda en la página.
- **Página sin acceso**: `App.razor` (`AuthorizeRouteView`) muestra un indicador circular mientras comprueba la sesión y, si la
  página no se puede ver, `Shared/KntAccessDenied`: sin sesión, "Sign in"/"Register" (al entrar, el router vuelve a evaluar la
  página y la muestra); con sesión pero sin el rol, "Back to Home" o, en una ventana de la app, "Close window" (la ventana lo
  sabe por el `CascadingValue` `KntWindow` de `KntWindowLayout`). `AuthorizeRouteView` dibuja esos estados en su `DefaultLayout`,
  no en el `@layout` de la página: `App.razor` le pasa el de la página (`LayoutOf`), para que una ventana no salga con los menús.
- **Acciones según el rol** (como en `ClientWin`): los botones y menús de una acción (New/Edit/Delete note, Print/Export,
  Folder...) están **siempre habilitados** (solo dependen de que haya algo seleccionado), y la acción comprueba el rol al
  elegirla con `Auth/KntAccess` (`Check`/`CheckAsync`, rol mínimo y qué se intenta): si no llega, una notificación "Access
  denied" dice qué rol hace falta y no se hace nada. La comprobación es síncrona (el `AuthenticationState` de la página, ya
  resuelto): una acción que abre una ventana comprueba antes sin esperar. `KntAccess.HasRole` es la comprobación jerárquica
  (`Guest < Staff < ProjectManager < Admin`); úsala en vez de varios `IsInRole`. Lo que no es una acción elegida se adapta
  al rol sin avisar: el doble clic de una lista edita o, si no se puede, ve; el orden elegido en el árbol solo se guarda
  para Staff+. Las secciones del menú lateral y las páginas siguen con `AuthorizeView`/`[Authorize]`, y `Server` comprueba
  cada petición.
- **Sesión rechazada**: un 401 con sesión iniciada (`BaseService`) no se notifica como error: avisa a
  `AppState.OnSessionEnded`, y los dos layouts llaman a `ILoginService.SessionRefusedAsync`, que adopta el token de
  `localStorage` si otra ventana de la app ya guardó uno nuevo (tras cambiar la contraseña) o, si no, cierra la
  sesión. `Logout` vacía lo leído con la sesión (`AppState.ClearUserData`: árbol de carpetas, carpeta, filtro y
  posiciones de las listas), y **en `AppState` solo se cachea una lectura válida**: un árbol que no se pudo leer
  (401, sin red) quedaba vacío hasta recargar la página.

## SmartDTO en la UI

Los DTO de `Model/Dto` heredan de `SmartModelDtoBase` (`Model/Core`): no son POCO, llevan estado y
validación que ambas UIs usan directamente:

- `IsDirty()`/`SetIsDirty(...)`, `IsNew()`/`SetIsNew(...)` (recorren también los hijos). Patrón habitual:
  tras cargar o guardar, `SetIsDirty(false)`; antes de guardar, `if (!dto.IsDirty()) return;`; y al salir se
  pide confirmación si `IsDirty()` (en una ventana de nota, con `KntWindows.WatchUnsavedChangesAsync`, ver
  `NoteEdit.razor`).
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

- Servicios en `Program.cs` con `builder.Services.AddRadzenComponents()` (`DialogService`,
  `NotificationService`, `TooltipService`, `ContextMenuService`...).
- `<RadzenComponents />` en `Shared/MainLayout.razor` (anfitrión de diálogos, notificaciones, menús
  contextuales y tooltips). Sin él los servicios anteriores no muestran nada.
- Layout: `RadzenLayout` + `RadzenHeader`/`RadzenSidebar`(`RadzenPanelMenu`)/`RadzenBody`/`RadzenFooter` (el pie,
  `Shared/MainLayoutFooter`: la aplicación, su descripción y versión a la izquierda; el usuario con su rol más alto y
  el repositorio abierto a la derecha). Las ventanas de la app (notas, asistente de IA, chat room) usan
  `Shared/KntWindowLayout` (solo `RadzenBody`, con su propio `<RadzenComponents />`). El sidebar está siempre
  visible (`Expanded="true"`, `Responsive="false"`: plegado, Radzen lo deja con ancho 0) y su botón cambia el
  `DisplayStyle` del `RadzenPanelMenu` entre `IconAndText` e `Icon` (`MainLayoutNavMenu.Compact`; clase
  `knt-sidebar-compact` en `knt-theme.css`; solo con iconos, cada entrada muestra su texto como tooltip). La elección
  se guarda en `localStorage` (`KNoteMenuCompact`); sin ella, `RadzenMediaQuery` (máx. 768 px) empieza con iconos.
- Tema: `<RadzenTheme Theme="@KntThemes.Default" />` en el `<HeadContent>` de `App.razor` (no un `<link>` fijo
  en `index.html`), seguido de `css/knt-theme.css`. El usuario elige tema (los 5 gratuitos de Radzen,
  `Shared/KntThemes`), modo claro/oscuro y alto contraste (WCAG) en el panel `Shared/KntThemeSelector` (botón
  de paleta de `MainLayoutHeader`, panel lateral), y cambia de claro a oscuro con `RadzenAppearanceToggle`.
  Todo pasa por `ThemeService`. La elección se guarda en cookies (`KNoteTheme`, `KNoteThemeWcag`,
  `KNoteThemeVersion`): `App.razor` las lee de forma síncrona (`kntTheme.read`, interop de WebAssembly) antes de
  que se pinte `RadzenTheme` y las escribe en `ThemeService.ThemeChanged` (`kntTheme.save`). `wwwroot/js/
  knt-theme.js` (en el `<head>` de `index.html`) define esas funciones y carga la hoja del tema guardado antes de
  que arranque Blazor (sin destello del tema por defecto); la quita en cuanto carga la de `RadzenTheme`
  (`#radzen-theme-link`). No uses `CookieThemeService` de Radzen: no guarda el WCAG. El panel también elige el
  estilo de la cabecera de Home (`soft` por defecto o `ink`), que no es de Radzen: lo guarda `knt-theme.js`
  (cookie `KNoteHomeHeader`, `kntTheme.getHomeHeader`/`setHomeHeader`) como atributo `data-knt-home-header` de
  `<html>`, puesto antes de que arranque Blazor, y `Pages/Index.razor.css` lo usa en sus selectores. `index.html` carga
  además `css/app.css` (lo que hace falta antes de arrancar Blazor) y `_content/Radzen.Blazor/Radzen.Blazor.js`. Los iconos de Radzen (`Icon="save"`, `"edit"`, `"delete"`...)
  son nombres de Material Symbols.
- **Colores y temas**: la app debe verse bien con cualquier tema de Radzen, claro u oscuro. Usa solo variables
  del tema (`--rz-primary`, `--rz-primary-lighter`, `--rz-on-primary`, `--rz-text-color`,
  `--rz-base-background-color`...) y las de `knt-theme.css`; los únicos colores fijos son `--knt-brand`
  (el ámbar del logo), `--knt-ink` (cabecera de Home "ink") y `--knt-paper*` (ver abajo). No uses
  `rz-background-color-base-*` ni `rz-color-base-*` para fondos: la escala `base` no se invierte
  en los temas oscuros (fondo claro con texto claro); para un fondo suave usa `knt-background-subtle`. Los
  colores de estado también salen del tema: validación (`.invalid`, `.validation-message`, en `knt-theme.css`),
  resaltados de los calendarios (`--rz-warning-lighter`), citas marcadas (`--rz-danger`). El HTML de una nota
  (`Shared/ViewMarkdown`, envuelto en `.knt-markdown`) puede traer colores propios de ClientWin (`<font color>`,
  `style`, `bgcolor`), pensados para su editor blanco: en los temas oscuros esa nota se muestra sobre una hoja
  blanca (`--knt-paper`, regla con `:has` en `knt-theme.css`); las notas sin colores propios siguen el tema.
  `knt-theme.css` también declara la fuente Inter (incluida en el paquete de Radzen) como
  `--knt-heading-font-family`, para títulos y cabeceras, y ajusta el ancho del sidebar a su texto
  (`max-content`). El contenido de una nota (`.knt-markdown`) nunca ensancha la página: sus bloques anchos (`pre`,
  tablas) tienen su propio scroll horizontal, las palabras largas se parten y lo demás (imágenes, diagramas, HTML de
  `ClientWin` con anchos fijos) se desplaza dentro del contenido. El logo de la app es `Shared/KntLogo` (SVG, color
  `currentColor` → `--knt-brand`). Un ajuste solo para los temas oscuros va bajo `:root:has(link[href*="dark-base.css"])` (sus hojas son
  `<tema>-dark-base.css` o `dark-base.css`), como el de los diálogos, que en oscuro se confundían con la página.

Patrones ya establecidos que hay que seguir:

- **Formularios** (los editores de entidades y sus diálogos): `EditForm` + `<DataAnnotationsValidator />` (las
  reglas de validación están en el SmartDTO) con controles de Radzen (`RadzenTextBox`, `RadzenTextArea`,
  `RadzenNumeric`, `RadzenDropDown`, `RadzenSwitch` para los booleanos, `RadzenDatePicker`, `RadzenPassword`,
  `RadzenSelectBar` para elegir entre pocas opciones) enlazados con `@bind-Value` y su
  `<ValidationMessage For="..." />` después. No se usan `InputText`/`InputNumber`/`InputSelect`... de Blazor ni
  `RadzenTemplateForm`. **La etiqueta va encima del campo**: `Shared/KntField` (`Label`, `Component` = `Name` del
  control, `Required` pone el asterisco, `Help` una línea de ayuda debajo; el control ocupa el ancho del campo, sin
  `Style="width: 100%"`). Los campos se agrupan en secciones, `Shared/KntFormSection` (icono, título y una línea
  de para qué sirve; dos seguidas se separan con una línea). Varios campos en una fila: `RadzenRow Gap="1rem"` con
  un `RadzenColumn Size="12" SizeSM="n"` por campo. Referencias: `Folders/FolderForm`, `Attributes/AttributeForm`,
  `Notes/NoteTaskEditor`. Un panel lateral de criterios junto a una lista (`Notes/NotesFilterCriteria`) usa en
  cambio `RadzenFormField` `Variant.Outlined` (etiqueta flotante e icono en `Start`) con `InputSize.Small`, del
  tamaño de la lista. Para elegir carpeta, `Shared/KntFolderPicker` (un campo con icono, el botón "Change" que abre
  `KntFolderSelector` y el de quitar, solo si se atiende `FolderReset`; muestra lo que se le pase, el nombre o la
  ruta). En los temas oscuros los botones `Variant.Outlined` del color principal parecen deshabilitados: usa los
  rellenos. La opción activa de un `RadzenSelectBar` usa el color principal (`knt-theme.css`).
- **Botones y espaciado**: grupos de botones en `<RadzenStack Orientation="Orientation.Horizontal"
  JustifyContent="JustifyContent.End" Gap="0.5rem">`, sin clases de margen. Para maquetar,
  `RadzenStack`/`RadzenRow`/`RadzenColumn`/`RadzenCard`; si hace falta una utilidad CSS, las de Radzen
  (`rz-p-*`, `rz-m-*`, `rz-mx-*`...), no las de Bootstrap.
- **Listas**: `RadzenDataGrid` (normalmente `Density.Compact`, `AllowColumnResize`) dentro de
  `Shared/EntityList` (que pinta "cargando"/"sin registros" y, si la página no pudo leer la lista, el error con Retry:
  `LoadFailed`, `Retry`; sin él, una lectura fallida dejaba "cargando" para siempre; `Loading` y `NotRecordsFound`
  sustituyen sus textos: las notas de Home, `Notes/NotesList`, muestran tarjetas fantasma con la forma de `NoteItem` mientras
  se leen, solo si tarda más de 300 ms, y sin notas un `.knt-empty-state` centrado que dice de qué carpeta salen). Las acciones de una fila van en la cabecera de
  la página (Edit/Delete junto a New, sobre la fila seleccionada) y en un menú con clic derecho
  (`CellContextMenu` → `KntActionsMenu.Open(...)` con una lista de `KntAction`: texto, icono, acción, `Danger` para
  borrar; el clic derecho también selecciona la fila); el doble clic hace la acción principal (editar). Sin
  columna de acciones ni botones por fila. Ver `Users/UsersIndex`. Fuera de una lista (las tarjetas de Home) el
  menú es el botón ⋮ de `Shared/KntActionsMenu`. En un árbol, el menú va con el clic derecho
  (`KntFoldersTreeView.FolderActions`, `ItemContextMenu` de `RadzenTree`) y los botones de la cabecera actúan sobre
  el elemento seleccionado (`Folders/FoldersIndex`). El árbol de carpetas (`Shared/KntFoldersTreeView`, en el Tree panel,
  `FoldersIndex` y los selectores) guarda qué ramas están abiertas en `FolderDto.Expanded` (del árbol cacheado en
  `AppState.FoldersTree`, así que se recuerdan al volver a la página), que cambian los eventos `Expand`/`Collapse`; al
  cambiar la selección (o leerse de nuevo el árbol) abre una vez los ancestros de la carpeta seleccionada. No calcules
  `Expanded` de la selección: Radzen lo evalúa en cada dibujado y cierra la rama cuyo valor pasa de `true` a `false`. La
  selección se compara por `FolderId`. Las listas de administración se ordenan con
  `AllowSorting` (todos los datos en memoria, sin paginar) y, dentro de `Admin`, la clase `knt-admin-list` les da
  la altura de la ventana con scroll propio (`Admin.razor.css`). Tienen selección simple (`SelectionMode.Single`,
  el clic derecho también selecciona) y la clase `knt-list` (`knt-theme.css`: filas como las de las listas de notas,
  sin rayado, `AllowAlternatingRows="false"`, y la fila seleccionada con el color de su fila actual); la cabecera
  lleva Edit/Delete del elemento seleccionado junto a New, como en `Folders/FoldersIndex`.
- **Listas de notas** (árbol, filtro, búsqueda): `Pages/Notes/NotesGrid`, que llena la altura de su contenedor (la
  página fija la suya, `calc(100vh - ...)`, contando con el padding de la página, `--rz-layout-body-padding`
  de `knt-theme.css`, y la página no hace scroll). Carga sus datos con `LoadPage(page,
  order)`: virtualización de Radzen (`AllowVirtualization` + `LoadData`) sobre bloques de 50 notas cacheados, con
  el orden en el servidor (`OrderColumn`/`OrderAscending` de `NotesFilterDto`/`NotesSearchDto`, columnas de
  `NoteOrderCriteria.SortableColumns`, el número de nota desempata); el `SortOrder` de las columnas solo da el orden
  inicial (cambiarlo mientras el grid recarga deja las filas viejas). Las mismas columnas que ClientWin; vista compacta (#, Topic, Priority, Tags) o extendida con
  `NotesListViewToggle` (opción de "View options", `AppState.NotesListCompact` + `localStorage`), y Topic con
  `MinWidth`. Clic
  (también el derecho) selecciona; doble clic/Enter edita (ve, quien no puede editar); menú de acciones con clic
  derecho, tecla de menú o Mayús+F10; la cabecera de la página tiene Edit/Delete y, tras un separador (clase
  `knt-menu-separator` de `knt-theme.css`), el desplegable "View options" (`NotesViewOptionsMenu`: View de la nota
  seleccionada, la vista previa y la vista compacta o extendida; agrupadas para dejar sitio en la barra a otros
  casos de uso) (`NotesGrid.Current`, `HasCurrent`, `CurrentChanged`,
  `ViewCurrentAsync`/`EditCurrentAsync`/`DeleteCurrentAsync`; editar y borrar comprueban el rol en `NotesGrid`, ver
  "Acciones según el rol"). Ver y editar abren la nota en su ventana (ver "Ventanas de la app"). En el árbol, el
  desplegable "Folder" de la cabecera crea una carpeta dentro de la seleccionada, edita o borra la seleccionada y
  vuelve a leer el árbol ("Refresh tree"), y el clic derecho en una carpeta da "Edit folder"/"Delete folder"
  (ProjectManager+, como `FolderEditorCtrl` de `ClientWin`); los casos de uso de carpeta son los de `Admin`
  (`Folders/FolderDialogs`, que también usa `FoldersIndex`), y tras un cambio el árbol se vuelve a leer con la carpeta
  seleccionada. **Vista previa**: `NotesPreviewToggle` (opción de "View options", `AppState.NotesPreviewVisible`
  + `localStorage`, común a las tres páginas) muestra a la derecha de la lista `NotePreviewPane`, que carga la nota
  seleccionada cuando la selección se para 250 ms (con las flechas no carga cada nota del camino) y la recarga si
  se guarda en su ventana. Desde el clic hasta que se ve la nota seleccionada, la anterior se atenúa y corre una barra
  fina arriba (tras un rato sin uso el servidor tarda y parecía la seleccionada); la nota y el HTML de su descripción se
  leen juntos y cambian a la vez (`NoteViewer.DescriptionHtml`), para que no se vea el tema de una con la descripción de
  otra. Su `RadzenSplitterPane` está siempre, con el tamaño de todos los paneles fijado desde el
  principio: Radzen reparte los tamaños al registrar los paneles y uno añadido después no cabe (el anterior pasa a
  `flex: 0 0` con su tamaño). Oculto, lo esconde la clase `knt-preview-hidden` del splitter (`knt-theme.css`) y la
  lista (`knt-notes-list-pane`) ocupa su sitio; así `NotesGrid` no se recrea y conserva su estado. Los tamaños
  que deja el usuario en los paneles de cada página se recuerdan en el navegador con `Shared/KntPaneSizes`
  (`Size="@paneSizes[i]"` en cada panel y `Resize="paneSizes.Resized"` en el splitter; Radzen solo informa del
  panel arrastrado, y el siguiente se queda con el resto del espacio de los dos). La fila actual y el teclado los lleva
  `wwwroot/js/notesGrid.js`, no Radzen: su navegación con flechas cuenta las filas dibujadas y con virtualización
  se pierde; la posición de cada fila va en `data-knt-index` (el `aria-rowindex` de Radzen se estropea al
  redibujar). `NotesGrid` no se redibuja por su página salvo si cambia `Compact` (`ShouldRender`): redibujarlo
  mientras Radzen carga filas lo deja vacío o con filas viejas; la marca de ese redibujado se limpia también en
  `OnAfterRender` (si el grid ya tenía uno suyo en cola, Blazor no llama a `ShouldRender` y se perdería el siguiente
  propio, p. ej. el fin de una carga). **Carga y errores**: el indicador de Radzen (`IsLoading`) cuenta las cargas en
  curso (`loadsRunning`, desde el principio de `OnLoadData` y con `finally`: varias a la vez, una que acaba por
  recargarse o con excepción); un bloque que no se pudo leer deja `loadFailed`: sin filas, la lista dice "The notes
  could not be loaded" con **Retry** (`EmptyTemplate`); con filas de esa misma lista, lo dice el resumen de arriba. Retry
  es `RefreshAsync` (`RefreshDataAsync` hace que Radzen vuelva a pedir filas que ya pidió). El árbol de `NotesTree` hace
  lo mismo si no se puede leer. Las dos usan `.knt-load-error`/`.knt-load-error-text` de `knt-theme.css`. El velo de
  carga de los grids (`IsLoading`) es del color de fondo del tema y solo aparece si la carga dura más de 300 ms
  (`knt-theme.css`): el gris de Radzen destellaba en cada carga rápida. Al dejar la página (p. ej. por el menú lateral)
  `NotesGrid` guarda en `AppState` (`NotesListPosition`, en su `DisposeAsync`; el desplazamiento lo va anotando
  `notesGrid.js`, porque entonces sus filas ya pueden no estar) el desplazamiento, el orden y la nota
  seleccionada, y al volver se restauran (con el filtro o el texto buscado). Una nota guardada en otra ventana recarga las filas a la vista sin perder la
  selección (`RefreshAsync`; la virtualización de Radzen recarga sin redibujar, de ahí su `StateHasChanged`). Una
  lista nueva (otra carpeta, filtro o búsqueda) es un `NotesGrid` nuevo (`@key`). **Imprimir y exportar** la
  lista: `NotesPrintExportMenu`, el último elemento de la botonera de las tres páginas (Staff+, como en
  `ClientWin`, comprobado al elegir cada opción), un desplegable "Print/Export" con la lista, el libro (pide confirmación con más de
  `LargeNotesBook.Notes` notas), el detalle de la nota seleccionada y el CSV. Cada página le da de dónde salen sus
  notas (`Request`: carpeta, texto buscado o `AppState.NotesFilter`) y, en el árbol, el orden de la carpeta
  (`DefaultOrder`); el componente añade el orden y la vista de la lista (`NotesGrid.CurrentOrder`, `Compact`) y
  `Server` vuelve a cargar la lista entera (`TotalCount` solo decide si hay notas y el aviso del libro).
- **Cabeceras**: la de una página de sección es `KntIndexHeader` con `Section="KntSections.Xxx"` (título e
  icono de su entrada del menú; `TextHead`/`Icon` los sustituyen, como en `NotesTree`, que muestra la carpeta, y
  `Subtitle` añade una línea en letra pequeña debajo: en `NotesTree`, la ruta de la carpeta, sacada del árbol ya cargado,
  sin pedirla al servidor, como la cabecera de `ClientWin`) y
  la botonera en `IndexMenu` (normalmente un `RadzenMenu`, que la cabecera deja sin fondo). `Shared/KntSections`
  es la única fuente de texto, ruta e icono de cada sección: la usan el menú lateral, las pestañas de `Admin` y
  las cabeceras. Dentro de otra sección (las pestañas de `Admin` pasan el `CascadingValue`
  `KntCompactHeader = true`) la cabecera sale compacta. La de un formulario de página completa es
  `ToolingHeader` (`Title`, `Icon` opcional y `Buttons`), una versión más ligera de la misma familia. Un botón de
  la botonera puede tener opciones (`RadzenMenuItem` con hijos, como Print/Export): `KntIndexHeader` les da el
  fondo del tema (la barra es transparente), las muestra como un menú (icono y texto en fila; solo los botones de
  la barra llevan el icono encima) y pone su flecha junto al icono; el último de la barra lleva la clase
  `knt-menu-end` para que el desplegable se abra hacia la izquierda, dentro de la ventana.
- **Tooltips**: siempre con `tooltipService.Open(...)` (en `MouseEnter`), no con el atributo `title`, que
  muestra el tooltip del navegador con otro estilo.
- **Diálogos** (alta/edición de entidades, selectores, login): `dialogService.OpenAsync<Componente>(título,
  parámetros, KntDialogs.Framed(KntDialogs.Small()/Medium()/Large()))` y el componente se dibuja dentro de
  `Shared/KntDialogFrame` (`Icon`, `Title`, `Subtitle`, el contenido y los botones en `Footer`): cabecera con el
  icono de la entidad (el de su `KntSection`) y su botón de cerrar, cuerpo con scroll y pie siempre visible.
  `Framed` quita la barra de título de Radzen (el diálogo no se arrastra). Botones del pie: "Cancel"
  (`ButtonStyle.Base`, `Variant.Text`) y la acción principal con icono (`ButtonStyle.Primary`). El botón de cerrar
  de la cabecera está fuera del orden de tabulación, para que Radzen enfoque el primer campo; Esc también cierra.
  Si Enter debe enviar (login, registro), `EditForm OnSubmit` + `ButtonType.Submit`. El componente devuelve su
  resultado con `dialogService.Close(resultado)` (`null` = cancelado); confirmaciones con
  `dialogService.Confirm(...)`. Ejemplos: `TypesIndex` → `TypeEdit`/`TypeNew`, `NoteForm` →
  `NoteTaskEditor`/`NoteResourceEditor`, `KntFolderPicker` → `KntFolderSelector`. `KntDialogs` no fija la altura:
  el diálogo mide lo que su contenido; si tiene una lista o un árbol que puede crecer, limita esa zona
  (`max-height`), no el diálogo. Un contenido que se desplaza solo (la vista previa de un informe) usa
  `KntDialogFrame Fill="true"`: el diálogo ocupa el alto de la ventana y el cuerpo, sin scroll propio, el espacio
  que le dejan la cabecera y el pie (el contenido, `height: 100%`).
- **Listas dentro de un editor** (los valores de un atributo, los recursos y las tareas de una nota): una cabecera
  con cuántos hay y el botón de añadir (`.knt-list-header` de `knt-theme.css`), filas o tarjetas con sus botones
  (`Variant.Text`) y un estado vacío que dice cómo llenarla (`.knt-empty-state`). Son pequeñas y no siguen la
  convención de las listas de página (acciones en la cabecera y clic derecho). Borrar pide confirmación. Iconos:
  `Shared/KntFileTypes` (tipo de fichero) y `Shared/KntDataTypes` (tipo de dato de un atributo); ninguna imagen de
  otros sitios.
- **Editor de notas** (`Notes/NoteForm`, en su ventana): cabecera compacta con la nota (el número, o "New", bajo el
  icono; el tema y, debajo, la ruta de la carpeta, el tipo y las etiquetas) y sus botones; pestañas con contadores (`RadzenTabsItem` `Template`); Overview en dos
  columnas (tema y descripción; panel "Details" y "Record"). La ruta de la carpeta la da `Server`
  (`IStore.Folders.GetPathAsync` → `api/folders/{id}/path`, `Folders.GetPathAsync` de `Service`, la misma de
  `ClientWin`). "Record" tiene el interruptor que bloquea el contenido de la nota (`ContentType` → JSON con
  `DescriptionBlocked`): bloqueado, la descripción solo se ve (sin editor). La Web no menciona `ClientWin` en la
  interfaz. Las pestañas de recursos y tareas se cargan al abrir la nota y otra vez si cambia (`OnParametersSetAsync`
  por `NoteId`, p. ej. tras "Save and new").
- **Recursos de la nota** (`NoteKResources`, galería; `NoteResourceEditor`, diálogo de alta y edición): al editar
  un recurso solo cambian su descripción y su orden; el fichero no se cambia ni se reenvía (`Server` conserva el
  fichero y el contenido guardado en la BD, ver `Server/CLAUDE.md`). Al pulsar un recurso se ve en la misma ventana
  de la nota (`NoteResourceViewer`, casi a ventana completa): imagen (ajustada o a su tamaño), PDF (visor del
  navegador), vídeo, audio o texto (en un `iframe` con `sandbox`: un fichero guardado como texto puede ser HTML). Los
  demás se abren en otra pestaña: la ventana de la nota es emergente y no tiene pestañas, así que se abren en la
  ventana principal. El tipo se deduce del MIME o, si no lo hay, de la extensión (`KntFileTypes.KindOf`: `ClientWin`
  solo guarda el tipo de unos pocos ficheros).
- **Tareas de la nota** (`NoteKTasks`), como en `ClientWin`: la lista (por prioridad y tema) y, al lado, la tarea
  seleccionada con sus datos y su descripción en HTML (`ViewMarkdown`: Markdown largo, con imágenes), en un
  `RadzenSplitter` con tamaños guardados (`KntPaneSizes`, clave `KNoteNoteTasksPanes`); en una ventana estrecha la
  tarea va debajo (`.knt-tasks-split` en `knt-theme.css`). El título de una tarea es su `Tags` ("Topic / Tags",
  como en `ClientWin`) o, sin él, la primera línea de su descripción (`NoteTaskDto.GetTopic`, el mismo en la
  lista de tareas de `ClientWin`). Una tarea resuelta solo se distingue por su
  marca y su estado (sin tachar). `NoteTaskEditor` edita la descripción con `KntMarkdownEditor` (su modo se guarda
  aparte: `StorageKey`), y como `ClientWin` una tarea nueva es del usuario conectado, con prioridad n+1 y empezada
  ahora, y al resolverla sin fecha de fin se le pone la actual.
- **Notas trazadas** (`NoteTraceNotes`, presentación; `NoteTraceNoteEditor`, diálogo), como en `ClientWin`: dos paneles,
  "Trace notes from" (la nota es `ToId`) y "Trace notes to" (es `FromId`), cada relación descrita por la otra nota y
  su tipo, orden y peso (`IStore.Notes.GetTraceNotesAsync` → `NoteTraceNotesDto`, resuelto en `Server`). A diferencia
  de `ClientWin`, una relación **se guarda al momento** (como las tareas; la nota tiene que estar guardada): el
  diálogo recibe `Save` y solo se cierra si `Server` la acepta, así que una relación repetida (la rechaza `Service`)
  no pierde lo escrito. La pestaña se vuelve a leer cada vez que se abre (una relación también cambia desde la otra
  nota). La otra nota se elige con `Shared/KntNotePicker` → `Shared/KntNoteSelector` (diálogo `Large`: búsqueda de
  texto o árbol de carpetas, un `RadzenDataGrid` paginado en el servidor con `LoadData`, no `NotesGrid`, cuya
  virtualización no va bien en un diálogo; `ExcludedNoteIds` atenúa las notas que no se pueden elegir) y se abre en
  su ventana de edición (`KntWindows.OpenEditAsync`, sin esperas delante). Los tipos se mantienen en la pestaña
  "Trace note types" de `Admin` (`Pages/TraceNoteTypes`, como `Types`; `IStore.TraceNoteTypes`); su icono,
  `KntSections.TraceNoteTypes`, es también el de la pestaña del editor.
- **Notificaciones**: para errores de la API ya salen solas (ver arriba); para validaciones de la propia
  página, `notificationService.Notify(new NotificationMessage { Severity = ..., ... })`.
- Otros en uso: `RadzenTabs`, `RadzenSplitter`, `RadzenTree`, `RadzenScheduler` (calendarios de tareas y
  alarmas, `Notes/NotesTasksCalendar` y `NotesAlarmsCalendar`: aún son una prueba de concepto con datos de
  ejemplo, sin entrada en el menú hasta que se desarrollen), `RadzenDatePicker`, `RadzenCheckBoxList`, `RadzenPager`, `RadzenCard`.

**No hay Bootstrap ni otras librerías de iconos o CSS**: la UI usa solo Radzen (tema `software` por defecto)
más `wwwroot/css/app.css` (base `box-sizing`, tipografía, error de Blazor),
`wwwroot/css/knt-theme.css` (ajustes sobre el tema, ver arriba) y el CSS aislado de algún componente
(`*.razor.css`; para estilar un componente hijo de Radzen hace falta `::deep`). No añadas clases de Bootstrap (`row`, `col-*`, `form-control`, `btn`,
`mr-1`...) ni iconos de Font Awesome: no hay hoja que los defina. Los parámetros de un diálogo se pasan como
`new Dictionary<string, object?>()` (firma de `DialogService.OpenAsync` en Radzen 12).

## Páginas: organización y convenciones

- Por área, un trío de componentes: `XxxIndex` (lista), `XxxEdit`/`XxxNew` (caso de uso: cargan el DTO,
  guardan, navegan; son las equivalentes al `Ctrl`) y `XxxForm` (presentación: recibe el DTO por
  `[Parameter]` y avisa con `EventCallback` `OnValidSubmit`/`OnExit`...). `XxxEdit`/`XxxNew` pueden abrirse
  como página (`@page`) o como diálogo de Radzen (las de tipos, carpetas, atributos y usuarios se abren como
  diálogo desde su `Index`).
- **Rutas relativas**: la app se sirve bajo `/KNote/` (`<base href="/KNote/" />` en `index.html`, a juego con
  `app.UsePathBase("/KNote")` en `Server`). Navega con rutas sin `/` inicial (`store.NavigateTo("notes/tree")`,
  `Path="index"`), o se saldrá de la base.
- **Ventanas de la app** (`AppStoreService/KntWindows`). **De nota**: ver una nota (`notes/view/{id}`, `NoteView` → `NoteViewer`), editarla
  (`notes/edit/{id}`, `NoteEdit`) y crearla (`notes/new?folderId=...`, la misma `NoteEdit`) se hace en una
  ventana propia sin los menús (`@layout KntWindowLayout`), que abre `AppStoreService/KntWindows`
  (`OpenViewAsync`/`OpenEditAsync`/`OpenNewAsync`, `wwwroot/js/kntWindows.js`): una por nota y uso, que se trae al
  frente si ya está abierta (sin recargarla ni moverla: no pierde una edición en curso); las nuevas se abren en
  cascada desde la ventana que las abre (`openWindow`, dentro de la pantalla). Cada "New note" abre una ventana
  nueva, en la carpeta de `AppState.SelectedFolder`; al guardarla por primera vez pasa a ser la ventana de edición
  de esa nota (`BecomeEditWindowAsync`: cambia su nombre y su URL). El navegador
  solo deja abrirla justo tras un clic o una tecla: no pongas otra espera delante (el script se carga de antemano
  en `KntWindows.StartAsync`, que llaman los dos layouts); si la bloquea, se avisa. Cada ventana es otra
  instancia de la app (su propio `AppState`; la sesión sí se comparte, el JWT está en `localStorage`). Al guardar,
  `NoteEdit` avisa a las demás ventanas (`NotifyNoteSavedAsync`, `BroadcastChannel`) y quien muestre notas se
  suscribe a `KntWindows.NoteSaved` para refrescarse (las listas, Home, `NoteView`). La salida de `NoteEdit`
  cierra la ventana (`CloseAsync`; si no la abrió un script, va a Home) y cerrarla con cambios sin guardar
  pregunta (`WatchUnsavedChangesAsync`, `beforeunload`), sin `NavigationLock`, que preguntaría siempre.
  **De caso de uso**: el asistente de IA y el chat room también se abren en su ventana (`OpenAiAssistantAsync`,
  `OpenChatRoomAsync`), con el botón `open_in_new` ("Open in a new window") de su cabecera. Hay una de cada una (se
  trae al frente si ya está abierta) y la ventana principal vuelve a Home al abrirla: el caso de uso "se traslada", lo
  que evita dos instancias escribiendo la misma sesión de IA. La página de la ventana (`AIAssistantWindow`,
  `ChatRoomWindow`) es una página fina con `KntWindowLayout` que dibuja la página de siempre con `InWindow="true"`
  (sin el botón, con la altura de una ventana sin cabecera ni pie) y lo que se le pase en la URL: la sesión o el hilo
  mostrados (`AppState` no llega a la otra instancia). Un componente enrutable usado así no aplica su `[Authorize]`:
  la página de la ventana lleva el suyo.
- Estado compartido entre páginas en `store.AppState` (p. ej. `SelectedFolder`, la carpeta de una nota nueva,
  que `KntWindows.OpenNewAsync` pasa en la URL: `AppState` no llega a las ventanas de nota, que son otra
  instancia de la app). Un componente que se suscriba a `AppState.OnChange` implementa `IDisposable` y se
  desuscribe en `Dispose()`.
- `Nullable` está activado en este proyecto: `[Parameter] [EditorRequired] public NoteDto Note { get; set; }
  = null!;` para parámetros obligatorios.

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

- **Home y nombre del repositorio**: Home (`Pages/Index`) es pública (sin `[Authorize]`), igual que los endpoints
  anónimos que usa (ver `Server/CLAUDE.md`, "Autorización"). `MainLayout` lee una vez el nombre del repositorio
  (`IStore.SystemValues.GetRepositoryNameAsync`, `DB_REPOSITORYNAME`) en `AppState.RepositoryName` (`null`
  mientras se lee, vacío si no se pudo): Home lo muestra como título de su cabecera y las demás páginas en la
  barra superior, "KNote - nombre" (`.knt-app-repository`, oculto en un móvil; el layout se redibuja en cada
  navegación, así que el título sigue la página sin suscribirse a `LocationChanged`). A la derecha de las notas,
  `Pages/HomeFolders` (presentación) lista Home y sus carpetas hijas directas (`Folders.GetHomeChildrenAsync`) y
  al elegir una Home pide sus notas (`Notes.GetHomeNotesAsync(folderId)`, el mismo criterio que Home: prioridad y
  tema, 25 como mucho). La elegida se recuerda en `AppState.HomeFolderId` mientras la app está abierta; sin
  carpetas no hay panel, y en un móvil es una fila encima de las notas (`grid-template-areas` de
  `Index.razor.css`, así el orden del código coincide con el visual en escritorio).
- **Chat room** (`Pages/ChatRoom`, ruta `chat`, Staff+): `ChatRoom` es el caso de uso y `ChatRoomThreadsList`/
  `ChatRoomThreadNew` la presentación; el estado vive en `store.ChatRoom` (`AppStoreService/ChatRoomHub`), no en la
  página. `MainLayout` lo conecta al iniciar sesión un usuario Staff+ y lo desconecta al cerrarla
  (`AuthenticationStateChanged`); el token es el de `HttpClient` (`AccessTokenProvider`), con reconexión automática
  que vuelve a suscribir los hilos seguidos (los grupos de SignalR son de la conexión). Sigue los hilos en los que
  participa el usuario y los que abre; los mensajes de un hilo que no está en pantalla se notifican y cuentan como
  no leídos. Contrato del hub: `Model/Dto/ChatRoomHubContract` (ver `Server/CLAUDE.md`). La conversación es un
  `RadzenChat` cuyos `Messages` son los de la página (sin `MessagesChanged`: lo que escribe el usuario solo aparece
  cuando lo devuelve el servidor) y con `MessageTemplate`, para pintar el Markdown con `ViewMarkdown
  DisableHtml="true"` (no con el `RadzenMarkdown` de su plantilla); su `pre-wrap` se anula en
  `ChatRoom.razor.css`. Tras cada mensaje nuevo baja al final (`ScrollToBottom` en `OnAfterRenderAsync`) y otra vez
  al cargarse sus imágenes (`js/chatRoom.js`). Las imágenes se pegan o se sueltan (`js/chatRoom.js` →
  `[JSInvokable] OnImage`), se suben con `ChatRoomHub.SendImageAsync` (REST, `ChatRoomHubContract.ImagesPath`) y se
  envían como un mensaje con su Markdown. **En su ventana** (`ChatRoomWindow`, `chat/window?threadId=`, ver "Ventanas
  de la app"): `KntWindowLayout` no conecta el hub (las ventanas de nota no lo usan), así que lo conecta la propia
  página antes de dibujar `ChatRoom` (`ChatRoomHub.StartForUserAsync`, la misma comprobación de rol que `MainLayout`)
  y lo desconecta al cerrarse. Cada ventana tiene su conexión: la principal sigue siguiendo los hilos y avisando de
  sus mensajes, y `Server` cuenta una sola vez a un usuario conectado varias veces.
- **Asistente de IA** (`Pages/AIAssistant`, ruta `aiassistant`, Staff+): `AIAssistant` es el caso de uso y
  `AiChatView`/`AiSessionsList` la presentación. La conversación y la caja de la pregunta van en un `RadzenSplitter`
  vertical (el panel de la pregunta empieza con la altura que tenía la caja, `Rows="3"`, y no se recuerda; el del chat lleva
  `Size="calc(100% - 90px)"`: sin tamaño, Radzen le da el 100 % y la pregunta queda fuera de la vista); los tokens
  de la conversación se ven en la entrada de la sesión abierta en `AiSessionsList` (la lista no trae los de las demás:
  `GetFilterMinimalAsync` no lee la descripción). Usa `IStore.AiAssistant` (`api/aiassistant`, ver
  `Server/CLAUDE.md`). La respuesta llega en streaming: `ChatAsync` lee los server-sent events con
  `SetBrowserResponseStreamingEnabled(true)` + `ResponseHeadersRead` y devuelve un `IAsyncEnumerable` de
  `AiChatStreamEventDto`; un error (antes o durante la respuesta) acaba en un evento `error` ya notificado. El
  servidor no guarda la conversación: la página manda los turnos anteriores en cada pregunta y repinta como
  mucho cada 100 ms mientras llega la respuesta. La sesión se guarda (`SaveSessionAsync`) tras cada respuesta, y
  otra vez antes de dejarla (nueva conversación, otra sesión, salir de la página) si aquel guardado falló.
  Cambiar de proveedor empieza una conversación nueva; al retomar una sesión se usa su proveedor y modelo
  (`AiProviderSelection.ForSession`, en `Model`) o, si ya no están configurados, el predefinido. Las notas que
  crea la tool `create_task` se enlazan en el chat (se abren en su ventana de edición) mientras la página está
  abierta. **En su ventana** (`AIAssistantWindow`, `aiassistant/window?sessionId=`, ver "Ventanas de la app") retoma
  la sesión de la URL (`OpenSession`). El botón que la abre está desactivado mientras responde o queda un guardado
  pendiente, para que la sesión que se pasa sea la guardada. En la ventana no hay `NavigationLock` que valga para
  salir: cerrarla mientras responde o sin haber podido guardar pregunta (`WatchUnsavedChangesAsync`,
  `HasUnsavedChanges`).
- **Markdown**: las descripciones de nota se editan con `Shared/KntMarkdownEditor` y se muestran con
  `Shared/ViewMarkdown` (Markdig). `KntMarkdownEditor` es un `textarea` con una barra de herramientas (con las clases
  y el aspecto del editor de Radzen) más una vista "Preview" (`ViewMarkdown`); el modo se recuerda en el navegador.
  **No se usa `RadzenMarkdownEditor`**: en cada tecla vuelve a analizar todo el Markdown en .NET, en el hilo del
  navegador (~6 ms por KB y tecla: con una nota de 130 KB, ~770 ms por tecla y el texto decenas de segundos por
  detrás al teclear seguido; medido con 12.1.2 y 12.1.4). El texto lo edita el navegador (`wwwroot/js/
  kntMarkdownEditor.js`) y .NET lo recibe pocas veces (`OnTextChanged`): en el primer cambio (la nota queda
  modificada al momento: el aviso de cerrar la ventana pregunta a .NET de forma síncrona), tras 300 ms sin teclear,
  al perder el foco (`OnLeave`, con la posición del cursor) y cuando se pide con `FlushAsync()`. **Quien guarde o lea
  la descripción llama antes a `FlushAsync()`** (`NoteForm.Save`, `NoteTaskEditor` al aceptar, el paso a Preview). El
  `textarea` no se enlaza con Blazor: su texto lo pone el script (al arrancar o con un texto nuevo de fuera, como
  tras "Save and new"), porque un render movería el cursor y perdería el deshacer. Las herramientas y `Ctrl+B`/`I`/`K`
  (`command` del script) cambian la selección con `execCommand('insertText')`, para que el deshacer del navegador
  (`Ctrl+Z`, Undo) las incluya; sin el script (no se pudo cargar) queda el `textarea` con `@onchange` y sin barra. En
  `NoteForm` las pestañas se dibujan solo la elegida (`TabRenderMode.Server`): el editor desaparece al ir a otra
  pestaña, así que guarda dónde estaba el cursor en un `KntMarkdownEditorState` de `NoteForm` (al volver se restaura,
  y "insertar en la descripción" un recurso lo pone ahí, en su propio párrafo: `InsertBlock`). El modo Design
  (WYSIWYG) de Radzen no está: Enter crea un párrafo y Markdown lo escribe como una línea en blanco más, y `ClientWin`
  muestra el Markdown como texto, línea a línea. El navegador devuelve el texto con saltos LF y las notas los guardan
  CR LF: esa diferencia (y los espacios o líneas en blanco del final) no es un cambio de la nota (`SameText`), y el
  componente no se dibuja hasta saber el modo guardado. `Server` reescribe las URLs de recursos de la descripción al leer/guardar
  para compatibilizarla con `ClientWin` (ver `Server/CLAUDE.md`). **El HTML de una nota no se genera en el
  navegador**: `ViewMarkdown` lo pide ya saneado a `Server` (`IStore.Notes.HtmlAsync` → `POST api/notes/html`,
  Markdig + `KntHtmlSanitizer` de `Service`), con las mismas reglas que `ClientWin`; las descripciones de Home ya
  llegan así de `HomeNotes` (anónimo) y se pasan con `SafeHtml`. Sanear en WebAssembly (HtmlSanitizer/AngleSharp)
  se descartó: +680 KB de descarga y 1-2 s con una nota de 130 KB. No insertes HTML de usuario de otra forma
  (`MarkupString`), tampoco el tema de una nota. Los **diagramas Mermaid** (`<pre class="mermaid">`) los dibuja
  `ViewMarkdown` en `OnAfterRenderAsync`, solo si el HTML ha cambiado y los tiene (no con `DisableHtml`), con
  `kntMermaid.render` (`wwwroot/lib/mermaid/kntMermaid.js`, cargado en `index.html`), que descarga
  `mermaid.min.js` (~3,6 MB) la primera vez que hace falta y redibuja al cambiar el tema (ver el `CLAUDE.md` raíz). Para contenido sin HTML que cambia a menudo (el chat del
  asistente, que se repinta mientras llega la respuesta) `DisableHtml="true"` lo genera en el navegador, con su
  HTML como texto y solo enlaces `http`, `https`, `mailto` o relativos, igual que `AiChatHtml` en `ClientWin`.
- **Informes imprimibles**: los genera `Server` (`IStore.Reports` → `api/reports`, ver `Server/CLAUDE.md`) como una
  página HTML completa (A4, con sus estilos), con la cultura del navegador. `Shared/KntReportPreview`
  (`KntReportPreview.OpenAsync(dialogService, título, subtítulo, load)`, `KntDialogs.Document()`) la muestra mientras
  se genera y después en un `iframe` con `sandbox="allow-same-origin allow-modals allow-popups
  allow-popups-to-escape-sandbox"`: sin scripts (el HTML viene de las notas), `allow-same-origin` para que
  `js/kntReports.js` la imprima y abra sus enlaces externos en otra pestaña, y `allow-modals` para `print()` (sin
  él el navegador lo ignora). Se carga desde una URL `blob:`, no con `srcdoc`: en `srcdoc` los enlaces `#id` (el
  índice de un libro) se resuelven contra la URL de la app y el iframe saldría del informe. Cerrar el diálogo antes
  de que llegue cancela la petición (un libro largo). `kntReports.downloadFile` guarda un fichero exportado (CSV).

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
3. Método en el `I*WebApiService` + implementación (heredando de `BaseService`, con `SendAsync` y un
   `CancellationToken cancellationToken = default` como último parámetro); si es un objeto de dominio nuevo, su
   propiedad en `IStore`/`Store`.
4. Página(s) y componentes con Radzen en `Pages/<Área>/`, con su `[Authorize(Roles = ...)]`, siguiendo las reglas de
   "Peticiones: doble envío y cancelación" (escrituras con `KntSingleFlight`, lecturas con `KntLatestRequest` o
   cancelación al salir, diálogos sin `Close` tardío). Si es una sección nueva del menú: su `KntSection` en
   `Shared/KntSections.cs`, su entrada en `Shared/MainLayoutNavMenu.razor` dentro del `<AuthorizeView>`
   correspondiente y `<KntIndexHeader Section="KntSections.Xxx">` en la página.
5. Si cambia el comportamiento de cara al usuario, `Docs/Manual.md` y `Docs/Manual_es.md`.
