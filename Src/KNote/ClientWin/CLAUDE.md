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
     │    └─ CtrlSelectorBase<TView, TEntity>
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
  es el punto donde cada Ctrl concreto invoca la factory (`Store.FactoryViews.View(this)`). `Run()` llama a
  `View.ShowView()`; `RunModal()` a `View.ShowModalView()`.
- **`CtrlViewEmbeddableBase<TView>`**: para vistas que pueden mostrarse como ventana flotante o embebidas en
  un panel (`ConfigureWindowMode()`/`ConfigureEmbededMode()` según `EmbededMode`).
- **`CtrlSelectorBase<TView, TEntity>`** — familia "seleccionar entidad": `SelectedEntity`,
  `ListEntities`, abstractos `LoadEntities`/`SelectItem`/`RefreshItem`/`AddItem`/`DeleteItem`, eventos
  `EntitySelection`/`EntitySelectionDoubleClick`/`EntitySelectionCanceled`. Ejemplos:
  `NotesSelectorCtrl`, `FoldersSelectorCtrl`, `NoteTypesSelectorCtrl`, `FiltersSelectorCtrl`.
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
(`HeavyProcessCtrl`, `KntChatCtrl`, `KntChatGPTCtrl`, `KntHttpClientCtrl`, `KntLabCtrl`,
`KntServerCOMCtrl`, `MessagesManagmentCtrl`), gestionando su vista manualmente si la necesitan.

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

Más interfaces específicas de un caso de uso concreto: `IViewKNoteManagment`, `IViewPostIt<T>`,
`IViewChat`, `IViewServerCOM`, `IViewHeavyProcess`.

`IFactoryViews` (`Core/IFactoryViews.cs`) declara **una sobrecarga de `View(...)` por cada Ctrl concreto**
(resolución por el tipo estático del controlador), más un par de vistas auxiliares de
`KNoteManagmentCtrl` (`NotifyView`, `AboutView`). `FactoryViewsWinForms` (`Core/FactoryViewsWinForms.cs`)
es la única implementación y simplemente construye el `Form` correspondiente:

```csharp
public IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl controller)
    => new NoteEditorForm(controller);
```

Cada Ctrl implementa `CreateView()` delegando en la factory de `Store`:

```csharp
protected override IViewEditorEmbeddable<NoteExtendedDto> CreateView()
    => Store.FactoryViews.View(this);
```

El `Form` concreto (en `Views/`) recibe el **controlador concreto** en su constructor y lo llama
directamente (`_ctrl.MetodoX()`), mientras que el Ctrl solo ve al `Form` a través de la interfaz `IView*`.
Es un acoplamiento asimétrico a propósito: View → Ctrl concreto (referencia directa), Ctrl → View (solo
interfaz), que es lo que permite sustituir WinForms por otro framework sin tocar `Controllers/`.

**Al añadir un caso de uso nuevo hay que tocar, en este orden**: interfaz `IView*` (si no hay una genérica
que sirva) → sobrecarga en `IFactoryViews` + `FactoryViewsWinForms` → clase `Ctrl` en `Controllers/`
heredando de la base adecuada → `Form` en `Views/` implementando la interfaz.

## `Store` (`Core/Store.cs`)

`Store` es el estado global y el mediador entre controladores; **no** guarda un único servicio, sino una
**lista de `ServiceRef`** (la app puede tener varias bases de datos de notas abiertas a la vez):

- `List<ServiceRef> _servicesRefs` + `AddServiceRef`/`RemoveServiceRef`/`GetServiceRef(Guid|alias)`/
  `GetFirstServiceRef()`/`GetActiveOrDefaultService()`. Cada `ServiceRef` (en `Service/Core/ServiceRef.cs`)
  construye de forma perezosa su `IKntService`, que a su vez usa un `IKntRepository` (Dapper o EF, según
  `RepositoryRef.Orm`) — así es como `Store`/los controladores llegan finalmente a la capa de persistencia.
- `List<CtrlBase> _listControllers` — todos los controladores vivos, usados para operaciones transversales
  como `SaveActiveNotes()`, `SaveAndCloseActiveNotes()`, `HidePostIts()`/`ActivatePostIts()`.
- `FolderWithServiceRef ActiveFolderWithServiceRef` / `SelectedNotesInServiceRef
  ActiveFilterWithServiceRef` — selección activa compartida (carpeta/filtro actuales), cambiada vía
  `ChangeActiveFolderWithServiceRef(...)` con sus eventos `ChangedActiveFolderWithServiceRef`.
- Eventos de coordinación: `AddedController`/`RemovedController`/`ControllerStateChanged`, más
  `ControllerNotification` (canal genérico de "toast" que cualquier Ctrl puede disparar vía
  `CtrlBase.NotifyMessage`).
- **Relé automático de eventos de nota/postit**: `AddController`/`RemoveController` reconocen
  explícitamente `NoteEditorCtrl` y `PostItEditorCtrl` (`if (controller is NoteEditorCtrl) ...`) y
  reexponen sus eventos (`AddedEntity`, `SavedEntity`, `DeletedEntity`, ...) como eventos propios de
  `Store` (`Store.SavedNote`, `Store.DeletedNote`, `Store.AddedPostIt`, etc.), para que un controlador no
  relacionado (p. ej. una lista de notas) pueda suscribirse a `Store.SavedNote` sin conocer la instancia
  concreta del editor. **Este mecanismo no es genérico**: un nuevo controlador tipo "editor de nota" que
  quiera el mismo relé automático requiere modificar `Store.AddController`/`RemoveController`.
- `AppConfig` (serializado a `KNoteData.config`), `Logger` (NLog), helpers de scripting (`RunKntSCode`,
  `RunCSCode`, `ExecuteCommand`) para el motor KntScript.
- Constructor: `Store(IFactoryViews factoryViews)` — la factory se inyecta aquí, no vía DI.

## Ejemplo de flujo completo: `NoteEditorCtrl`

```csharp
// Program.cs — composition root
Store appStore = new Store(new FactoryViewsWinForms());
var knoteManagment = new KNoteManagmentCtrl(appStore);
knoteManagment.Run();
Application.Run(new ApplicationContext { MainForm = (Form)knoteManagment.View });
```

```csharp
// Controllers/KNoteManagmentCtrl.cs — un caso de uso lanza otro
public async Task AddNote(IKntService service)
{
    var noteEditorCtrl = new NoteEditorCtrl(Store);   // construcción manual, sin DI
    await noteEditorCtrl.NewModel(service);
    noteEditorCtrl.Run();                              // → CreateView() → Store.FactoryViews.View(this)
}                                                       //   → new NoteEditorForm(this) → View.ShowView()
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
  de propagarla (existe el flag `ThrowKntException` en `CtrlBase` para el caso contrario).
- **Async**: todo lo que toca `IKntService` es `async Task`/`async Task<T>`; los handlers de UI son
  `async void`. Para trabajo largo se usan hilos/tasks explícitos (`Store.RunKntSCodeInNewThread`,
  `Store.RunCSCodeInNewTask`) en vez de un patrón `async`/`await` puro — no hay disciplina de
  `ConfigureAwait`, se depende del `SynchronizationContext` de WinForms para volver al hilo de UI.
- **Múltiples servicios/BDs simultáneas**: no asumas un único servicio ambiente — los métodos de
  editor/selector (`NewModel`, `LoadModelById`, `LoadEntities`) reciben explícitamente el `IKntService` a
  usar.
- **Modo embebido**: si el nuevo caso de uso necesita mostrarse tanto en ventana flotante como embebido en
  un panel, hereda de `CtrlViewEmbeddableBase`/`CtrlNoteEditorEmbeddableBase` e implementa
  `IViewEmbeddable` en el `Form` (`PanelView()`, `ConfigureEmbededMode()`, `ConfigureWindowMode()`).
