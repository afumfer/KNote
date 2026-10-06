# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this project
(`ClientWin`, la app de escritorio WinForms de KNote). Ver también el `CLAUDE.md` de la raíz del repo para
el contexto general de la solución y el patrón Repository intercambiable Dapper/EF.

## Idea central de la arquitectura

`ClientWin` organiza cada caso de uso (editar una nota, seleccionar una carpeta, gestionar mensajes...) en
una clase **controladora** con sufijo `Ctrl` (p. ej. `NoteEditorCtrl`, `FolderEditorCtrl`,
`NotesSelectorCtrl`). Estas clases:

- Implementan el flujo del caso de uso (cargar, guardar, cancelar, seleccionar, etc.).
- Heredan de una jerarquía de clases base abstractas (`Ctrl*Base`) que aportan comportamiento genérico
  compartido por familias de casos de uso (editar entidad, seleccionar entidad, editar nota...).
- No conocen WinForms directamente: hablan con su UI a través de una interfaz `IView*`, cuya
  implementación concreta les entrega una **factory** (`IFactoryViews`/`FactoryViewsWinForms`).
- Reciben una referencia a `Store`, que mantiene el estado global de la app, actúa de mediador entre
  controladores, y da acceso a la capa `Service` (persistencia vía `IKntRepository`, más lógica de negocio).

No existe contenedor de inversión de control: todo el grafo de objetos (`Store`, la factory, cada `Ctrl`,
cada `Form`) se construye a mano con `new`, empezando en `Program.Main()`.

## Estructura de carpetas

```
ClientWin/
├── Program.cs              – composition root: crea Store, FactoryViewsWinForms y el Ctrl raíz
├── Controllers/            – UNA clase concreta "*Ctrl" por caso de uso, un archivo por clase
├── Core/
│     CtrlBase.cs           – raíz de la jerarquía de controladores
│     CtrlViewBase.cs       – resto de la jerarquía (editor/selector/embeddable/nota)
│     IViews.cs             – interfaces IView* que implementan las vistas
│     IFactoryViews.cs      – contrato de la factory de vistas
│     FactoryViewsWinForms.cs – implementación WinForms de la factory
│     Store.cs              – estado global, mediador entre controladores, acceso a Service
│     KNoteScriptLibrary.cs – funciones expuestas al motor de scripting KntScript
│     Reports/              – informes imprimibles y exportación CSV (ver "Informes imprimibles")
├── Views/                  – Forms de WinForms (uno o varios `IView*` implementados por Form)
├── Utils/, Resources/, AutoKntScripts/, Properties/, Log/
```

No hay carpetas separadas `Interfaces/`/`Factory/` ni organización por feature: es organización por capa
(`Controllers`, `Core`, `Views`), con namespace por carpeta (`KNote.ClientWin.Controllers`,
`KNote.ClientWin.Core`, `KNote.ClientWin.Views`).

**Nota de nomenclatura**: el sufijo `Ctrl` va al final del nombre (`NoteEditorCtrl`, no `CtrlNote`). El
prefijo `Ctrl` se reserva para las clases base abstractas (`CtrlBase`, `CtrlEditorBase`, `CtrlSelectorBase`,
...).

## Jerarquía de clases base de controladores

Todas son `abstract` y genéricas sobre el tipo de vista (y, desde `CtrlEditorBase` hacia abajo, también
sobre el tipo de entidad/DTO). Viven en `Core/CtrlBase.cs` y `Core/CtrlViewBase.cs`:

```
CtrlBase
 └─ CtrlViewBase<TView>
     ├─ CtrlViewEmbeddableBase<TView>                    (TView : IViewEmbeddable)
     │    ├─ CtrlSelectorBase<TView, TEntity>
     │    │    └─ CtrlSyncableSelectorBase<TView, TEntity>     (TView : IViewSelector<TEntity>)
     │    └─ CtrlManageListBase<TView, TEntity>
     └─ CtrlEditorBase<TView, TEntity>                   (TEntity : SmartModelDtoBase, new())
         └─ CtrlNoteEditorBase<TView, TEntity>
             └─ CtrlNoteEditorEmbeddableBase<TView, TEntity>   (TView : IViewEmbeddable)
```

- **`CtrlBase`**: `Store`, `ControllerId`/`ControllerName`, `EControllerState`, `EmbededMode`. Ciclo de
  vida `Run()` → `CheckPreconditions()` → `OnInitialized()` (virtuales, para sobrescribir en cada Ctrl
  concreto) → `Finalize()`/`Dispose()`. El constructor `CtrlBase(Store store)` se auto-registra con
  `Store.AddController(this)`. `FinalizeViewsController()` usa reflexión para encontrar **campos** (no
  propiedades) de tipo `CtrlBase`/`IViewBase` en la clase derivada, finalizarlos/cerrarlos y anularlos —
  si un controlador guarda sub-controladores o sub-vistas para que se limpien solos, deben ser campos de
  esos tipos exactos (o marcarse con `[ResetControllerField]`).
- **`CtrlViewBase<TView>`**: posee la vista de forma perezosa vía el método abstracto `CreateView()` — este
  es el punto donde cada Ctrl concreto resuelve su vista contra `Store.FactoryViews.Registry`. `Run()` llama a
  `View.ShowView()`; `RunModal()` a `View.ShowModalView()`.
- **`CtrlViewEmbeddableBase<TView>`**: para vistas que pueden mostrarse como ventana flotante o embebidas en
  un panel (`ConfigureWindowMode()`/`ConfigureEmbededMode()` según `EmbededMode`). Úsala directamente (con
  `TView = IViewEmbeddable`) para paneles que solo capturan datos y notifican al padre con un evento propio
  — no repliques la semántica de "seleccionar un elemento de una colección" de `CtrlSelectorBase` si el
  caso de uso no es eso. Ejemplos: `NotesSearchParamCtrl` (búsqueda rápida), `NotesFilterParamCtrl`
  (filtro estructurado) — ambos con evento propio (`SearchApplied`/`FilterApplied`) en vez de
  `EntitySelection`.
- **`CtrlSelectorBase<TView, TEntity>`** — familia "seleccionar entidad": `SelectedEntity`,
  `ListEntities`, abstracto `LoadEntities`, eventos
  `EntitySelection`/`EntitySelectionDoubleClick`/`EntitySelectionCanceled`. Por sí sola sirve para un
  selector de lista fija cargada una vez (`TView` puede ser el propio `IViewEmbeddable`, sin más).
  Ejemplo: `NoteTypesSelectorCtrl`.
- **`CtrlSyncableSelectorBase<TView, TEntity>`** (`TView : IViewSelector<TEntity>`) — añade los
  abstractos `SelectItem`/`RefreshItem`/`AddItem`/`DeleteItem`, para selectores cuya lista se mantiene
  sincronizada en caliente con altas/bajas que ocurren en otra ventana mientras el selector sigue
  abierto. Ejemplos: `NotesSelectorCtrl`, `FoldersSelectorCtrl`. Mantenla separada de
  `CtrlSelectorBase`: un selector de lista fija (`NoteTypesSelectorCtrl`) no tiene forma de implementar
  estos cuatro miembros con sentido — antes de este split los tenía como `throw new
  NotImplementedException()`, un contrato que no encajaba con el caso de uso real.
- **`CtrlEditorBase<TView, TEntity>`** — familia "editar entidad": `Model` (perezoso, `new()`),
  `IKntService Service`, `ServiceRef` (resuelto vía `Store.GetServiceRef(...)`), abstractos
  `LoadModelById`/`NewModel`/`SaveModel`/`DeleteModel`, eventos
  `SavedEntity`/`AddedEntity`/`DeletedEntity`/`EditionCanceled`. Ejemplos: `AttributeEditorCtrl`,
  `MessageEditorCtrl`, `TaskEditorCtrl`, `ResourceEditorCtrl`, `RepositoryEditorCtrl`,
  `NoteAttributeEditorCtrl`, `OptionsEditorCtrl`, `PostItPropertiesCtrl`.
- **`CtrlNoteEditorBase<TView, TEntity>`**: añade `GetFolder()` (abre un `FoldersSelectorCtrl` modal para
  elegir carpeta). Ejemplos: `FolderEditorCtrl`, `PostItEditorCtrl`.
- **`CtrlNoteEditorEmbeddableBase<TView, TEntity>`**: combina editor de nota + embebible. Único
  descendiente: **`NoteEditorCtrl`**, el caso de uso principal de la app.

Algunos controladores no encajan en editor/selector y heredan directamente de `CtrlBase`
(`HeavyProcessCtrl`, `KntChatCtrl`, `KNoteAIAssistantCtrl`, `KntHttpClientCtrl`, `KntLabCtrl`,
`KntServerCOMCtrl`, `MessagesManagementCtrl`), gestionando su vista manualmente si la necesitan.

Al crear un nuevo caso de uso: elige la clase base según la familia (editor/selector/nota) — no repliques
lógica de guardado/selección genérica dentro del Ctrl concreto, eso vive en la base.

## Vistas: interfaces `IView*` + Factory

Todas las interfaces viven en `Core/IViews.cs` (el propio archivo tiene un `//TODO: refactor view
hierarchy`, considérala provisional):

```csharp
public interface IViewBase
{
    void ShowView();
    Result<EControllerResult> ShowModalView();
    void RefreshView();
    void OnClosingView();
    DialogResult ShowInfo(string info, string caption = "KNote", ...);
}

public interface IViewEmbeddable : IViewBase
{
    Control PanelView();
    void ConfigureEmbededMode();
    void ConfigureWindowMode();
}

public interface IViewEditor<T> : IViewBase { ... }
public interface IViewEditorEmbeddable<T> : IViewEmbeddable { ... }
public interface IViewSelector<TItem> : IViewEmbeddable { ... }
```

Más interfaces específicas de un caso de uso concreto: `IViewKNoteManagement`, `IViewPostIt<T>`,
`IViewChat`, `IViewServerCOM`, `IViewHeavyProcess`, `IViewNotesSelector` (`IViewSelector<NoteMinimalDto>` +
`INotesListSnapshotProvider`, ver "Informes imprimibles").

`IFactoryViews` (`Core/IFactoryViews.cs`) históricamente declaraba **una sobrecarga de `View(...)` por cada
Ctrl concreto** (resolución por el tipo estático del controlador), más un par de vistas auxiliares de
`KNoteManagementCtrl` (`NotifyView`, `AboutView`) — con el inconveniente de que cada caso de uso nuevo
obligaba a tocar esa interfaz. Tras el refactor (Fases 4 y 4b), **`IFactoryViews` ya no declara ninguna
sobrecarga**: se ha quedado reducida a un único miembro, `ViewFactoryRegistry Registry { get; }`
(`Core/ViewFactoryRegistry.cs`), un mapa genérico `(tipo de Ctrl, key opcional) → Func<Ctrl, View>` (la
`key` distingue los tres registros de `KNoteManagementCtrl`: vista principal, `Notify`, `About`).
`FactoryViewsWinForms` (`Core/FactoryViewsWinForms.cs`), su única implementación, registra las 25
fábricas existentes en su constructor y no expone ya ningún método `View(...)`:

```csharp
// Constructor de FactoryViewsWinForms — todo lo que queda de la fábrica
Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => new NoteEditorForm(c));
Registry.Register<KNoteManagementCtrl, IViewBase>(c => new NotifyForm(c), key: "Notify");
...
```

Los 25 `Ctrl` existentes resuelven su vista directamente contra el registro desde `CreateView()` (o desde
su propiedad de vista perezosa, en los pocos controladores como `KntChatCtrl`/`HeavyProcessCtrl` que no
heredan de `CtrlViewBase<TView>`):

```csharp
protected override IViewNoteEditorEmbeddable<NoteExtendedDto> CreateView()
    => Store.FactoryViews.Registry.Resolve<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(this);
```

`IViewNoteEditorEmbeddable<T>`/`IViewPostItEditor<T>` (`Core/IViews.cs`) son así: `IViewEditorEmbeddable<T>`/
`IViewPostIt<T>` combinadas con `IFolderAndRepositoryDisplay` (un único método,
`RefreshFolderAndRepositoryDisplayAsync()`, para repintar solo la ruta de carpeta/alias de repositorio sin
disparar un `RefreshView()` completo). Esa capacidad se mantiene fuera de `IViewEditorEmbeddable<T>`/
`IViewPostIt<T>` a propósito — son contratos genéricos con más implementaciones (p. ej.
`PostItPropertiesForm : IViewPostIt<WindowDto>`, que edita el estilo de la ventana, no una nota) que no
tienen carpeta/repositorio que mostrar y no deberían verse obligadas a implementar ese miembro.

**Un `Ctrl` nuevo no toca `IFactoryViews`** — nunca ha hecho falta desde que se retiraron todas las
sobrecargas: solo hay que registrar su fábrica (típicamente en el constructor de `FactoryViewsWinForms`)
y resolverla desde `CreateView()` con el mismo patrón de arriba.

El `Form` concreto (en `Views/`) recibe el **controlador concreto** en su constructor y lo llama
directamente (`_ctrl.MetodoX()`), mientras que el Ctrl solo ve al `Form` a través de la interfaz `IView*`.
Es un acoplamiento asimétrico a propósito: View → Ctrl concreto (referencia directa), Ctrl → View (solo
interfaz), que es lo que permite sustituir WinForms por otro framework sin tocar `Controllers/`.

**Al añadir un caso de uso nuevo hay que tocar, en este orden**: interfaz `IView*` (si no hay una genérica
que sirva) → registro en `ViewFactoryRegistry` (vía el constructor de `FactoryViewsWinForms`) → clase
`Ctrl` en `Controllers/` heredando de la base adecuada, resolviendo su vista contra el registro, con su
`[KntAuthorize]` si es un caso de uso de nivel superior (ver "Autenticación y autorización") → `Form` en
`Views/` implementando la interfaz.

## `Store` (`Core/Store.cs`)

`Store` es el estado global y el mediador entre controladores; **no** guarda un único servicio, sino una
colección de `ServiceRef` (la app puede tener varias bases de datos de notas abiertas a la vez). Desde el
refactor incremental documentado en el plan de mejora de este archivo (ver más abajo), parte de lo que
antes eran campos/lógica directos de `Store` vive en clases auxiliares propias, con `Store` delegando en
ellas y manteniendo su API pública sin cambios para el resto del código:

- `ServiceRefRegistry` (`Core/ServiceRefRegistry.cs`) — posee la colección de `ServiceRef` y las consultas
  básicas (`GetById`/`GetByAlias`/`GetFirst`/`GetAll`); `Store.AddServiceRef`/`RemoveServiceRef`/
  `GetServiceRef(Guid|alias)`/`GetFirstServiceRef()`/`GetActiveOrDefaultService()` delegan en ella. Cada
  `ServiceRef` (en `Service/Core/ServiceRef.cs`) construye de forma perezosa su `IKntService`, que a su vez
  usa un `IKntRepository` (Dapper o EF, según `RepositoryRef.Orm`) — así es como `Store`/los controladores
  llegan finalmente a la capa de persistencia.
- `ControllerRegistry` (`Core/ControllerRegistry.cs`) — posee la colección de `CtrlBase` vivos;
  `Store.AddController`/`RemoveController` delegan el almacenamiento ahí, y operaciones transversales como
  `SaveActiveNotes()`, `SaveAndCloseActiveNotes()`, `HidePostIts()`/`ActivatePostIts()` iteran sobre
  `_controllerRegistry.All`.
- `KntTextUtils` (`Core/KntTextUtils.cs`) — utilidades puras de texto/fichero (`TextToDateTime/Int/Double`,
  `ExtractUrlFromText`, `ExtensionFileToFileType`, `IsSupportedFileTypeForPreview`), expuestas como
  `Store.KntTextUtils` (propiedad *lazy*, una única instancia por `Store`: `_kntTextUtils ??= new
  KntTextUtils();`). `Store` **no** tiene métodos envoltorio para esto — los que había (`Store.TextToInt(...)`,
  etc.) se retiraron a propósito; los ~20 puntos de uso en `Controllers/`/`Views/` llaman a
  `Store.KntTextUtils.TextToInt(...)` directamente. Se eligió esta vía (propiedad instanciable) en vez de
  dejar `KntTextUtils` como `static class` invocada sin pasar por `Store`, porque para el dominio de KNote
  conviene que estas utilidades sean descubribles como parte de `Store` (autocompletado `Store.` →
  `KntTextUtils`) en vez de exigir conocer y referenciar una clase estática suelta.
- `DomainEventBus` (`Core/DomainEventBus.cs`), expuesto como `Store.Events` — bus de publicación/suscripción
  genérico (`Subscribe<TMessage>`/`Unsubscribe<TMessage>`/`Publish<TMessage>`). `CtrlEditorBase<TView,
  TEntity>.OnSavedEntity`/`OnAddedEntity`/`OnDeletedEntity` publican `EntitySaved<TEntity>`/
  `EntityAdded<TEntity>`/`EntityDeleted<TEntity>` en él, además de disparar los eventos CLR
  `SavedEntity`/`AddedEntity`/`DeletedEntity` de siempre — por vivir en la base genérica, esto aplica a los
  8 editores (`NoteEditorCtrl`, `PostItEditorCtrl`, `TaskEditorCtrl`, etc.), no solo a Note/PostIt.
  `NoteEditorCtrl`/`PostItEditorCtrl` también publican `PostItEditRequested`/`ExtendedEditRequested`
  (mensajes específicos de la transición nota↔post-it) desde sus propios `OnPostItEdit`/`OnExtendedEdit`.
  **`Store.AddController`/`RemoveController` ya no conocen ningún tipo concreto de controlador** — el
  antiguo relé especial-caseado (`if (controller is NoteEditorCtrl) ...` más los eventos
  `Store.SavedNote`/`DeletedNote`/`AddedPostIt`/etc.) se ha retirado; `KNoteManagementCtrl` y los propios
  `NoteEditorCtrl`/`PostItEditorCtrl` (que necesitan enterarse de que una nota se borró en otra ventana)
  ahora se suscriben directamente a `Store.Events`. Un controlador nuevo que quiera difundir sus cambios
  no necesita tocar `Store`: le basta con heredar de `CtrlEditorBase` (para Saved/Added/Deleted) o publicar
  sus propios mensajes en `Store.Events` (para eventos específicos de su dominio).
  `IKntService.CommandExecuting`/`CommandExecuted` (`Service/Core/IKntService.cs` — disparados por
  `KntServiceBase.ExecuteCommand` alrededor de cada uno de los ~80 comandos de
  `Service/ServicesCommands/`, sirve por igual a `Server`) se republican también en `Store.Events` como
  `ServiceCommandExecuting`/`ServiceCommandExecuted`: `Store.AddServiceRef`/`RemoveServiceRef` se
  suscriben/desuscriben por cada `ServiceRef` que gestionan. Ver `MonitorCtrl` como suscriptor de
  referencia de ambos tipos de evento.
  Los propios eventos de coordinación de `Store` (ciclo de vida de controladores/`ServiceRef`, el canal
  de "toast") también pasan por aquí en vez de por `event EventHandler<T>` propios de `Store`:
  `ControllerAdded`/`ControllerRemoved`/`ControllerStateChanged` (`Store.AddController`/`RemoveController`
  más el relé `Controller_StateCtrlChanged` de cada `CtrlBase.StateControllerChanged`),
  `ServiceRefAdded`/`ServiceRefRemoved` (`Store.AddServiceRef`/`RemoveServiceRef`) y
  `ControllerNotification` (canal genérico de "toast" que cualquier Ctrl puede disparar vía
  `CtrlBase.NotifyMessage` → `Store.OnControllerNotification`).
- `FolderWithServiceRef ActiveFolderWithServiceRef` / `SelectedNotesInServiceRef
  ActiveFilterWithServiceRef` — selección activa compartida (carpeta/filtro actuales), cambiada vía
  `ChangeActiveFolderWithServiceRef(...)` con sus eventos `ChangedActiveFolderWithServiceRef`. Estos dos
  siguen siendo `event EventHandler<T>` propios de `Store` (no migrados a `Store.Events`).
- `Settings` (`AppUserSettings`, lo que configura el usuario) y `State` (`AppUserState`, lo que recuerda la
  app), persistidos en dos ficheros (ver "Configuración persistida" más abajo), `Logger` (NLog), helpers de
  scripting (`RunKntSCode`,
  `RunCSCode`, `ExecuteCommand`) para el motor KntScript.
- `Security` (`KntSecurityContext`): cómo ha entrado el usuario de la sesión (`AppUserName`) y su rol en
  cada repositorio vinculado; `AuthenticateRepositoryAsync`, `NotifyAccessDenied`/`AccessDeniedNotifier` y
  `CanRunScripts` (ver "Autenticación y autorización").
- Constructor: `Store(IFactoryViews factoryViews)` — la factory se inyecta aquí, no vía DI.

## Configuración persistida (`Settings` / `State`)

La configuración vive en `%LocalAppData%\KNote` (`AppUserDataPath`), en **dos ficheros XML** que `Store`
carga/guarda con `LoadConfig`/`SaveConfig` delegando en `Core/AppConfigStorage.cs`:

- `KNoteData.config` ← `Store.Settings` (`AppUserSettings`, en `Model/Config`): lo que **configura el usuario**
  (`General`, `Security`, `Repositories`, `Ai`, `Notifications/Email`, `Connectivity` con `ChatHub`/`MessageBroker`/
  `ServerCOM`). Solo se reescribe cuando los ajustes cambian de verdad (`AppConfigStorage` compara con lo
  último leído/escrito), así que llamar a `SaveConfig()` por un cambio de estado no lo toca.
- `KNoteState.config` ← `Store.State` (`AppUserState`): lo que **la app recuerda sola** (`Session`,
  `ManagementWindow` con `Bounds`/`NotesList`/`Panels`, `AppInfoAlarmsWindow`). Se reescribe en cada
  `SaveConfig()`.

Ambos se leen/escriben con `Core/XmlConfigFile` (guardado atómico vía fichero temporal, deja el anterior como
`.bak` y, si el fichero no se puede leer, carga ese `.bak`).

**Secretos**: `SmtpPassword` (`Email.Password`), `AiProviderRef.ApiKey` y las `ConnectionString` que llevan
`Password=`/`Pwd=` se guardan cifrados con DPAPI (usuario actual) como `dpapi:<base64>`. El cifrado ocurre solo
en la capa de fichero (`AppUserSettingsSecrets` + `ISecretProtector`/`DpapiSecretProtector`); en memoria y en la
UI son texto plano. `Model` no conoce el cifrado (lo comparten `Server` y `Client`). Un valor sin prefijo se lee
como texto plano y se cifra en el siguiente guardado; uno que no se puede descifrar (otro usuario/equipo) se
vacía y se avisa al usuario.

**Añadir un ajuste nuevo**: decide si lo escribe el usuario (→ una sección de `AppUserSettings`) o la app
(→ `AppUserState`), y ponle un valor por defecto en la propia clase: un fichero guardado antes de que existiera
el ajuste debe cargar sin él. Si es un secreto, añádelo a `AppUserSettingsSecrets.Secrets(...)`. Si el diálogo
de Opciones lo edita, añádelo también a `Core/OptionsModel` (`From`/`ApplyTo`). Tests en
`AppConfigStorageTests`/`ConfigSerializationTests`.

**Formato antiguo (V1)**: hasta esta reestructuración todo iba en un único `KNoteData.config` plano (raíz
`<AppConfig>`). `AppConfigStorage.Load` lo detecta por el elemento raíz y lo migra: escribe los dos ficheros
nuevos y deja `KNoteData.config.v1.bak` (copia **sin secretos**). `Model/Config/Legacy/AppConfigV1` es el lector
**congelado** de ese formato (conserva sus nombres con erratas: `Respository`, `Managment`, `Ascendig`) y
`AppConfigMigrator` la conversión: no los renombres ni los elimines mientras haya instalaciones por migrar.
El fixture `ClientWin.Tests/Fixtures/KNoteData.v1.config` (datos sintéticos) cubre esa migración.

## Ejemplo de flujo completo: `NoteEditorCtrl`

```csharp
// Program.cs — composition root
Store appStore = new Store(new FactoryViewsWinForms());
var knoteManagement = new KNoteManagementCtrl(appStore);
knoteManagement.Run();
Application.Run(new ApplicationContext { MainForm = (Form)knoteManagement.View });
```

```csharp
// Controllers/KNoteManagementCtrl.cs — un caso de uso lanza otro
public async Task AddNote(IKntService service)
{
    var noteEditorCtrl = new NoteEditorCtrl(Store);   // construcción manual, sin DI
    await noteEditorCtrl.NewModel(service);
    noteEditorCtrl.Run();                     // → CreateView() → Store.FactoryViews.Registry.Resolve<...>(this)
}                                              //   → new NoteEditorForm(this) → View.ShowView()
```

Dentro de `NoteEditorCtrl.LoadModelById`, el acceso a datos baja por:

```
NoteEditorCtrl (ClientWin)
  → Service.Notes (IKntNoteService)
    → KntNoteService — construye un objeto Command (Service/ServicesCommands)
      → KntNotesGetExtendedAsyncCommand — agregación/lógica de negocio
        → IKntRepository.Notes — contrato de persistencia
          → Repository.Dapper / Repository.EntityFramework
```

`NoteEditorCtrl` también orquesta sub-casos de uso ejecutando otros `Ctrl` como diálogos modales, p. ej.
`EditAttribute()` crea un `NoteAttributeEditorCtrl(Store)`, hace `LoadModel(...)` y `RunModal()`, y lee
`.Model` al volver. Es el patrón habitual para "un caso de uso abre otro": `new SubCtrl(Store)` →
configurar → `RunModal()`/`Run()` → leer resultado por evento o por `.Model`.

## Otras convenciones a tener en cuenta

- **Sin contenedor DI**: todo se construye con `new`, de arriba abajo desde `Program.Main()`. Los nuevos
  controladores siguen el mismo patrón (`new MiCtrl(Store)`).
- **Menús → controlador**: el handler de clic de un menú en el `Form` llama directamente a un método del
  Ctrl propietario (`_ctrl.AddNote()`), sin registro de comandos ni mapeo por atributos.
- **Errores**: la capa de servicio devuelve `Result`/`Result<T>` (`IsValid`, `ErrorMessage`). Los métodos de
  los `Ctrl` normalmente capturan la excepción internamente y llaman a `View.ShowInfo(ex.Message)` en vez
  de propagarla (existe el flag `ThrowKntException` en `CtrlBase` para el caso contrario). **El valor de
  retorno `bool` de `SaveModel`/`DeleteModel`/`LoadModelById` debe reflejar honestamente el resultado**:
  `false` si `Result.IsValid` es `false` o si se capturó una excepción, `true` solo si la operación
  realmente tuvo éxito (Fase 5 del refactor corrigió varios `Ctrl` — `NoteEditorCtrl`, `PostItEditorCtrl`,
  `TaskEditorCtrl`, `MessageEditorCtrl`, `ResourceEditorCtrl`, `FolderEditorCtrl`,
  `NoteAttributeEditorCtrl` — que devolvían `true` incluso tras un fallo mostrado al usuario; el caso más
  grave era `NoteEditorCtrl.DeleteModel`, que ni siquiera mostraba el mensaje de error). Al escribir un
  `Ctrl` nuevo, sigue el patrón ya usado en `NotesSelectorCtrl`/`NoteTypesSelectorCtrl`: `if
  (response.IsValid) { ...; return true; } else { View.ShowInfo(response.ErrorMessage); return false; }`.
- **Async**: todo lo que toca `IKntService` es `async Task`/`async Task<T>`; los handlers de UI son
  `async void`. Para trabajo largo se usan hilos/tasks explícitos (`Store.RunKntSCodeInNewThread`) o
  E/S asíncrona basada en eventos (`InteractiveScriptSession`, para los motores cs/py/js) en vez de
  un patrón `async`/`await` puro — no hay disciplina de `ConfigureAwait`, se depende del
  `SynchronizationContext` de WinForms para volver al hilo de UI.
- **Múltiples servicios/BDs simultáneas**: no asumas un único servicio ambiente — los métodos de
  editor/selector (`NewModel`, `LoadModelById`, `LoadEntities`) reciben explícitamente el `IKntService` a
  usar.
- **Modo embebido**: si el nuevo caso de uso necesita mostrarse tanto en ventana flotante como embebido en
  un panel, hereda de `CtrlViewEmbeddableBase`/`CtrlNoteEditorEmbeddableBase` e implementa
  `IViewEmbeddable` en el `Form` (`PanelView()`, `ConfigureEmbededMode()`, `ConfigureWindowMode()`).
- **Escalado (DPI)**: la app es `SystemAware` (`ApplicationHighDpiMode` en el csproj): se maqueta una vez a
  la escala del sistema y Windows reescala la ventana si se mueve a un monitor con otra escala. Se probó
  `PerMonitorV2` y descuadra las vistas embebidas (`TopLevel = false`) al pasar a un monitor de menor escala;
  no lo actives sin rehacer antes esas vistas. El diseñador de VS trabaja siempre a 100 %
  (`ForceDesignerDPIUnaware`), así que todos los formularios tienen `AutoScaleDimensions = 7x15`. Cualquier
  tamaño en píxeles que se asigne **en código** después de `InitializeComponent()` (anchos de columna,
  posiciones, tamaños de ventana) ya no lo escala WinForms: pásalo por `LogicalToDeviceUnits(...)`.
  Al restaurar la posición guardada de una ventana, pásala por `Utils/WindowPlacement.EnsureVisible(...)`:
  si el usuario cambió de monitor o de escala, puede caer fuera de todas las pantallas actuales (incluso en
  el hueco entre dos monitores). Un botón pegado a la derecha de la caja de texto sobre la que actúa (`...`,
  `X`) se iguala a ella con `Utils/ControlLayout.AlignToTextBox(...)` tras `InitializeComponent()`: el alto
  real de una `TextBox` de una línea lo decide su fuente en tiempo de ejecución, no el Designer. Los menús
  contextuales son `Utils/KntContextMenuStrip`, no `ContextMenuStrip`: el de WinForms calcula su margen de
  imagen/check y sus rellenos a 96 PPP y solo los reescala en `PerMonitorV2`, así que a 200 % la barra
  lateral queda más estrecha que la casilla del check. Al añadir uno en el diseñador, cambia su tipo. Las
  barras de herramientas se dejan con su `Padding` y `RenderMode` por defecto (`ManagerRenderMode`, que usa
  `KntToolStripRenderer`), para que todas tengan el mismo alto: un `Padding` vertical extra no las hace más
  altas, aplasta sus botones y su grip.
- **Iconos**: salen de `KntIcons` (`KntIcon` + `SetKntIcon(...)`/`KntIconProvider`), asignados en código
  tras `InitializeComponent()`, nunca como imágenes en el diseñador. Ver `KntIcons/CLAUDE.md`. El icono de la
  aplicación es `Resources/Icons/KNote.ico` (el diseño de siempre, con fotogramas de 16 a 256 px), expuesto por
  `Utils/AppIcon`: `KntForm` lo aplica como icono de ventana en `OnLoad` (`WindowIcon`, que una vista sobrescribe
  si su ventana significa otra cosa, p. ej. `AppInfoAlarmsForm`), así que **no** se asigna `Icon` en el
  diseñador (dejaría una copia del `.ico` en cada `.resx`). La marca de check de los menús tampoco es la de
  WinForms (que copia su bitmap sin escalar y sale recortada a escalados altos): la dibuja
  `Utils/KntToolStripRenderer` con `KntIcon.Check`, fijado como `ToolStripManager.Renderer` en `AppTheme.Apply`.
  Los menús sin imágenes dejan `ImageScalingSize` por defecto (16x16): WinForms dimensiona con él el hueco
  del check, y uno mayor lo descentra.
- **Modo oscuro** (`Utils/AppTheme`, ajuste `General.ColorMode`: `Light`/`Dark`/`System`): lo pinta el propio
  WinForms (`Application.SetColorMode`), pero solo en las ventanas creadas después de fijarlo; cambiarlo con
  ventanas abiertas las deja a medio pintar. Por eso se aplica una vez al arrancar (`Program.Main`, tras
  cargar la configuración) y cambiarlo pide reiniciar (`Store.RestartRequested`). Lo que WinForms no resuelve
  se adapta en un único punto: iconos (`KntIcons`), listas sin cuadrícula (`ListViewStyle`), la rejilla de
  notas (`DataGridViewStyle`, con cabeceras planas y su propio glifo de ordenación) y el visor Markdown
  (`KNoteWebViewStyle.css` con `prefers-color-scheme`, que `KntEditView` fija según el modo de la app).
  Dos valores por defecto del diseñador se pintan en oscuro como marcos blancos gruesos: el borde `Fixed3D`
  (TextBox, ListBox, ListView, TreeView...) y `TabPage.UseVisualStyleBackColor = true`; y un combo
  `DropDownList` con `FlatStyle.Standard` deja claros el texto seleccionado y la flecha (pasa a `Flat`).
  `KntForm.OnLoad` los corrige en todos sus controles (`AppTheme.AdjustControlsForDarkMode`); los que se añaden después del
  `Load` (p. ej. los paneles embebidos vía `PanelView()` en `KNoteManagementForm.LinkComponents`, cuyas
  vistas nunca se cargan) necesitan llamarlo otra vez tras añadirlos. Al pasar un TextBox de una línea a
  `FixedSingle` lo encoge 1 px por arriba y por abajo (si no, el texto queda 1 px alto), y los botones
  igualados con `AlignToTextBox` lo siguen. Un `TabControl` cuyo contenido llega al borde con su propia
  scrollbar (el árbol de carpetas) usa `Utils/FlushTabControl`: en oscuro el margen interior entre páginas
  y marco se ve como un hueco junto a la scrollbar.
  En las vistas, usa `SystemColors.*` en vez de colores fijos (`Color.White`...): los fijos no cambian en
  oscuro. Los informes y su previsualización se quedan siempre en claro (son para imprimir).

## Autenticación y autorización

Dos niveles, al estilo de ASP.NET Core (`[Authorize]` en controladoras y acciones), sin guardas escritas a
mano en los métodos:

- **Casos de uso → controladoras.** La clase del `Ctrl` de nivel superior declara su rol mínimo con
  `[KntAuthorize(rol, ámbito)]` (`Model/KntAuthorization.cs`). `CtrlBase.CheckPreconditions()` (lo llama
  `Run()`/`RunModal()`) lo comprueba con `CheckAccess()`: si el usuario no llega, avisa con
  `Store.NotifyAccessDenied` (el `MessageBox` lo conecta `Program` en `Store.AccessDeniedNotifier`; es `null`
  fuera de la app, p. ej. en tests), finaliza la controladora y deja `PreconditionsMet = false`, así que
  `CtrlViewBase` **ni crea la vista**. `RequiredAuthorization` es virtual (`RepositoryEditorCtrl` lo decide
  por `EditorMode`) y `AuthorizationResource` dice contra qué repositorio se mira el rol (en
  `CtrlEditorBase`, su `Service`; sin repositorio se usa el rol de aplicación). `IsAuthorized()` hace la
  misma comprobación sin avisar, para lo que arranca solo. Un caso de uso que no arranca por `Run()`
  (`NotesListCsvExportCtrl.ExportAsync`, `ReportPreviewCtrl.ShowAsync`) llama a `CheckAccess()` al empezar,
  antes de pedir nada al usuario o de construir el informe. Los `Ctrl` que heredan de `CtrlBase` y muestran
  su vista aparte (`KntChatCtrl`, `KNoteAIAssistantCtrl`, `KntServerCOMCtrl`, `KntLabCtrl`) no la muestran si
  `!PreconditionsMet`.
- **Sin atributo = sin requisito**: así se marcan los `Ctrl` *incluidos* en otro caso de uso (selectores,
  pestañas del editor de repositorio, editores de tareas/recursos/mensajes de una nota...) — decide el que
  los incluye. `KNoteManagementCtrl` (el hub) tampoco lleva atributo: cada caso de uso que lanza decide.
- **Operaciones → comandos del Service.** Lo más fino (guardar, borrar, mover...) lo autoriza la capa
  Service en cada comando (ver el `CLAUDE.md` raíz). Un resultado denegado llega con `Result.NotAuthorized`:
  `CtrlViewBase.ShowResultError(result)` lo avisa igual que una controladora denegada (sin crear la vista) y
  `KntNotAuthorizedException.ThrowIfNotAuthorized(result)` corta un lote (`HeavyProcessCtrl`) en la primera
  denegación.
- **Roles** (`EnumRoles`, jerárquicos: `Guest < Staff < ProjectManager < Admin`; `KntRoles` lee
  `User.RoleDefinition`). Son **por repositorio**; el rol de aplicación es el más alto entre los
  repositorios vinculados (Guest como mínimo). Viven en `Store.Security` (`Core/KntSecurityContext.cs`):
  modo de acceso de la sesión, contraseña en memoria (nunca persistida), rol por repositorio (clave
  `IKntService.IdServiceRef`) e `IsAuthorized(requisito | tipo, service)` — el tipo puede ser un `Ctrl` o un
  comando del Service (así `NoteEditorCtrl.ConsultMode` usa el rol de `KntNotesSaveExtendedAsyncCommand` sin
  repetirlo).
- **Acceso** (`Program.SignInAsync`/`LinkRepositoriesAsync`): con `Settings.Security.AuthenticationMode =
  Credentials`, `LoginCtrl` pide usuario y contraseña (que solo inician la sesión); cada repositorio pasa por
  `Store.AuthenticateRepositoryAsync(service)` antes de vincularse — registrado (y, con credenciales, con la
  misma contraseña), registro ofrecido si no existe (`UserRegisterCtrl`, rol decidido por
  `Users.RegisterAsync`), o no vinculado en esta sesión. Tras gestionar un repositorio,
  `Store.RefreshRepositoryRoleAsync`.
- **Excepciones conscientes**, cada una en un único punto: ejecutar código exige ProjectManager de
  aplicación (`Store.CanRunScripts`/`CheckCanRunScripts`, en todos los `Run*`/`ExecuteCommand` de `Store` y en
  los KNote assistants del catálogo, que crean su propio motor KntScript); lo que arranca solo se salta en
  silencio (`MessagesManagementCtrl.RepositoriesAuthorizedFor`, panel AppInfo y chat al arrancar, alarmas de
  script); `NoteEditorCtrl.ConsultMode` (abrir para leer, sin autoguardado).
- **Las vistas no saben nada de seguridad**: todos los menús siguen habilitados y el aviso sale al usarlos.
  La barra de estado solo pinta los textos que prepara `KNoteManagementCtrl` (`SessionUserInfo`/
  `SessionUserDetail`).
- **Caso de uso nuevo**: decide si es de nivel superior (atributo) o incluido (sin atributo) y añádelo a la
  matriz de `ClientWin.Tests/ControllerAuthorizationMatrixTests` (falla hasta que lo hagas). Un comando nuevo
  del Service, igual con `Tests/ServiceTests/CommandAuthorizationMatrixTests`.

Esta autorización la aplica la aplicación: quien tenga la cadena de conexión de una BD no queda limitado por
ella.

## Informes imprimibles y exportación (`Core/Reports`)

Los informes son **HTML + CSS** que se muestran en una ventana de previsualización con WebView2, desde la que
se imprimen (diálogo de impresión de Chromium, con su propia vista previa y "Guardar como PDF") o se guardan
como PDF (`PrintToPdfAsync`). No hay `PrintDocument`/GDI+ ni librerías de PDF.

- **Modelo y renderizado (lógica pura, testeada en `ClientWin.Tests`)**: un generador construye un
  `ReportDocument` (título, cabecera, metadatos, cuerpo HTML, `Orientation`, `Layout` `Report`/`Book`,
  `ResolvePageReferences`) y `ReportHtml.Render` lo convierte en la página completa: hoja de estilo
  incrustada (`Resources/KNoteReport.css` o `KNoteBook.css`, `EmbeddedResource` con `LogicalName` en el
  csproj), `@page` (A4, orientación, cabecera de página y numeración con cajas de margen) y, en los
  informes, la banda de cabecera. Generadores existentes: `NotesListReport` (lista, horizontal),
  `NoteDetailReport` + `NoteDetailReportData` (detalle de nota, vertical), `NotesBook` (lista como libro),
  y `NotesListCsv` (exportación CSV, sin previsualización). `ReportFileName` propone nombres de fichero y la
  carpeta inicial de los diálogos (`AppUserState.Reports.LastExportFolder`, compartida por PDF y CSV).
- **Datos de la lista de notas "tal como se ve"**: el orden, las columnas visibles (vista compacta,
  `HiddenColumns`) y el filtro de texto en memoria **solo existen en la vista**, así que
  `NotesSelectorForm` los entrega como `NotesListSnapshot` (`INotesListSnapshotProvider`, combinada en
  `IViewNotesSelector`; textos de celda tal como los formatea el grid y `NoteIds` en orden). El origen de la
  lista (carpeta, búsqueda o filtro, con nombres resueltos) es `NotesListContext`, construido por
  `KNoteManagementCtrl.GetNotesListContextAsync`. Las descripciones se convierten a HTML con
  `NoteDetailReportData.RenderDescription` (mismas reglas que el editor; los recursos se apuntan al host
  virtual `KntConst.VirtualHostNameToFolderMapping`). Las trazas se resuelven con `Core/TraceNoteRows`
  (compartido con `NoteEditorForm`).
- **Previsualización**: `ReportPreviewCtrl.ShowAsync(Store, service, buildReport, resourcesRootPath)` abre
  `ReportPreviewForm`: comprueba primero el rol (Staff en el repositorio `service`) y solo después llama a
  `buildReport`. **Un informe nuevo no necesita vista ni Ctrl nuevos**: generador puro en `Core/Reports`
  (con tests) + llamada a `ReportPreviewCtrl.ShowAsync` desde el Ctrl del caso de uso. La exportación CSV
  es su propio caso de uso, `NotesListCsvExportCtrl` (sin ventana: `IViewFileExport`/`FileExportForm` solo
  aportan el diálogo de guardar). Todo texto del usuario va por
  `ReportHtml.Encode`; el que acaba en una caja de margen (`content: "..."`), por `ReportHtml.CssString`
  (escapa también `<`/`>`: va dentro de `<style>`).

Particularidades de WebView2/Chromium que motivan el diseño de `ReportPreviewForm` (no las deshagas):

- **Nada modal tras un `await` de WebView2.** La continuación de `await PrintToPdfAsync(...)`,
  `EnsureCoreWebView2Async(...)`, etc. se ejecuta **en línea dentro de la devolución de llamada de
  WebView2**; un `MessageBox` ahí abre un bucle de mensajes anidado que WebView2 no admite y el proceso
  muere sin excepción .NET (fail-fast `0x80000003` en `EmbeddedBrowserWebView.dll`). Antes de cualquier
  diálogo, `await LeaveWebView2CallbackAsync()` (un `Task.Yield()` que encola el resto en el contexto de
  WinForms).
- **`DefaultBackgroundColor` blanco**: el motor de impresión pinta los márgenes de página del PDF con ese
  color, no con el fondo del documento (con gris salían bordes grises). El gris de "escritorio" de la
  previsualización lo pone el CSS (`@media screen`).
- **HTML servido desde memoria** (`WebResourceRequested` sobre `https://knote.report/`), no
  `NavigateToString` (límite de 2 MB). Scripts de página desactivados (las descripciones las escribe el
  usuario); los enlaces solo se abren fuera si son http/https/mailto.
- **Números de página de un índice**: Chromium soporta cajas de margen (`@top-right`, `counter(page)`,
  `counter(pages)`) y páginas con nombre (`page: front`), pero **no** `target-counter()`. Por eso un
  informe con `ResolvePageReferences` se pagina en dos pasadas: se imprime a un PDF en memoria
  (`PrintToPdfStreamAsync`), `PdfNamedDestinations` lee de él en qué página quedó cada destino de enlace
  interno (`<a href="#id">` → elemento con ese `id`) y `ReportHtml.FillPageReferences` rellena los
  marcadores `ReportHtml.PageReference(id)` antes de mostrar la versión final. Los marcadores tienen ancho
  fijo para que rellenarlos no cambie la paginación.

## Tests (`ClientWin.Tests`)

Proyecto MSTest hermano (parte de `KNote.slnx`, referencia `ClientWin.csproj` directamente), con su propio
`ClientWin.Tests/CLAUDE.md` — léelo antes de tocar o añadir tests aquí, especialmente la convención de
"fakes hechos a mano" (`Fakes/`, sin Moq/NSubstitute) y la suite `RequiresRealAiProvider` (smoke tests
reales contra OpenAI/Anthropic/Ollama, pensada para detectar roturas tras actualizar los paquetes NuGet de
IA — no se ejecuta por defecto).

Dos detalles de `ClientWin` motivados exclusivamente por esa suite de tests, a tener en cuenta si tocas
código de IA:
- `ClientWin/Properties/AssemblyInfo.cs` declara `[assembly: InternalsVisibleTo("KNote.ClientWin.Tests")]`
  — `AiChatClientFactory.ResolveApiKey` y `KNoteAIAssistantCtrl.SetChatClientForTesting` son `internal`
  en vez de `private` únicamente para que los tests los ejerciten sin red real.
- `KNoteAiTools` recibe `IKntService` en el constructor (no `ServiceRef`) para las tools de solo lectura
  (`search_notes`/`get_note_details`) — así se pueden testear contra los fakes de servicio ya existentes
  (`Fakes/FakeKntService.cs`) sin base de datos real. `AiChatClientFactory.Create` sigue recibiendo
  `ServiceRef` (lo necesita para otras cosas) y le pasa `serviceRef.Service`. La tool `create_task`
  necesita además un `Store` completo — para leer `Store.DefaultFolderWithServiceRef` — por eso
  `KNoteAiTools` también recibe `Store`.
  - `create_task` persiste la nota **solo por la capa Service** (`Service.Notes.NewExtendedAsync()` +
    `SaveExtendedAsync(...)` contra el `IKntService` de `Store.DefaultFolderWithServiceRef`, el mismo
    patrón que `search_notes`/`get_note_details`, no algo especial) y luego abre esa nota **ya
    persistida** con el mismo camino "editar nota existente" que usa el resto de la app:
    `NoteEditorCtrl.LoadModelById(service, noteId)` + `.Run()`. No modifica `NoteEditorCtrl` ni accede a
    su `View` directamente — usa su API pública tal cual está, sin trucos de precarga de `Model`. Queda
    bajo responsabilidad del usuario modificarla y volver a guardarla, salir sin más, o borrarla.
  - Como la llamada a la tool ocurre dentro de la propia pipeline async del SDK de IA
    (OpenAI/Anthropic/Ollama), que puede perder el `SynchronizationContext` de UI a mitad de camino,
    `KNoteAiTools` captura `SynchronizationContext.Current` en el constructor (siempre el hilo de UI, ya
    que `AiChatClientFactory.Create` solo se llama desde manejadores de eventos de UI) y usa
    `_uiContext.Post(...)` para volver a él antes de construir el `NoteEditorCtrl`/`Form` — sin este
    marshaling, mostrar el editor desde un hilo de fondo lanzaría una excepción de WinForms de acceso
    entre hilos. Esta es la única parte de `create_task` con dependencia a `KNote.ClientWin.Controllers`
    desde `Core` (la dirección opuesta a la habitual en este proyecto), justificada por necesitar lanzar
    un `Ctrl` completo, no solo llamar a un método de servicio.
