# Checklist de regresión manual — ClientWin

Checklist de referencia para validar manualmente que una fase del plan de refactorización de
`ClientWin` (ver `ClientWin/CLAUDE.md`) no ha roto ningún caso de uso. No sustituye a los tests
automatizados de `ClientWin.Tests`, los complementa donde todavía no hay cobertura (la mayor parte
de la interacción real con WinForms).

Repetir esta checklist al cerrar cada fase del plan de refactorización, antes de dar la fase por
buena.

## Arranque y configuración

- [ ] La app arranca sin excepciones y muestra `KNoteManagementForm`.
- [ ] Se puede añadir/editar/eliminar un repositorio (`RepositoryEditorCtrl`) y aparece en la lista
      de servicios configurados.
- [ ] Cambiar entre dos repositorios/bases de datos abiertas simultáneamente actualiza correctamente
      la carpeta y el filtro activos.

## Notas (`NoteEditorCtrl`)

- [ ] Crear una nota nueva desde el menú, guardarla y verla aparecer en el listado.
- [ ] Abrir una nota existente por doble clic, editarla y guardar los cambios.
- [ ] Cancelar la edición de una nota no persiste cambios.
- [ ] Eliminar una nota la hace desaparecer del listado y de cualquier panel embebido abierto.
- [ ] El editor de notas embebido (panel dentro de `KNoteManagementForm`) funciona igual que una nota
      abierta en ventana flotante.
- [ ] Añadir/editar/eliminar un adjunto (`ResourceEditorCtrl`), una tarea (`TaskEditorCtrl`), un
      mensaje (`MessageEditorCtrl`) y un atributo (`NoteAttributeEditorCtrl`) desde dentro del editor
      de notas.
- [ ] Cambiar el tipo de una nota (`NoteTypesSelectorCtrl`) se refleja correctamente.

## Carpetas y selección

- [ ] Crear/editar/eliminar una carpeta (`FolderEditorCtrl`).
- [ ] Seleccionar una carpeta (`FoldersSelectorCtrl`) actualiza `Store.ActiveFolderWithServiceRef` y
      el listado de notas mostrado.
- [ ] En el tab "Search", conmutar entre búsqueda rápida (`NotesSearchParamCtrl`) y filtro estructurado
      (`NotesFilterParamCtrl`) muestra el panel correcto y ambos devuelven, vía `NotesSelectorCtrl`, los
      resultados esperados.

## Post-its

- [ ] Crear un post-it desde una nota y que aparezca como ventana flotante independiente.
- [ ] Ocultar/activar todos los post-its (`Store.HidePostIts`/`ActivatePostIts`) desde el menú
      correspondiente.
- [ ] Guardar/eliminar un post-it actualiza la nota asociada.

## Scripting (KntScript)

- [ ] Abrir la consola de scripts (`KntScriptConsoleCtrl`) y ejecutar un script de
      `AutoKntScripts/` sin errores.
- [ ] Un script que abre una nota (vía `KNoteScriptLibrary`) la abre correctamente.

## Notificaciones y coordinación entre controladores

- [ ] Guardar una nota desde una ventana flotante actualiza el listado de notas visible en otra
      ventana/panel abierto simultáneamente (verifica el relé de eventos de `Store`).
- [ ] Las notificaciones tipo "toast" (`Store.Events`, mensaje `ControllerNotification`) se muestran
      correctamente.

## Escalado (DPI) e iconos

Probar al menos a 100 % y a 200 %, y, si hay dos monitores con escalas distintas, arrancando en cada uno.

- [ ] Ninguna ventana se ve descuadrada al arrancar (barra de estado de `KNoteManagementForm`, tabs del
      editor de notas embebido, altura de filas del selector de notas).
- [ ] Los iconos (barras de herramientas, tabs, árbol de carpetas, botones de listas, PostIt, editor HTML,
      botones de navegación) se ven nítidos y proporcionados al texto, sin bitmaps estirados ni diminutos.
- [ ] El icono de la aplicación se ve nítido en la barra de título, la barra de tareas, Alt+Tab, la bandeja
      del sistema, el splash y "Acerca de".
- [ ] Una ventana guardada en una posición que ya no cae en ninguna pantalla (otro monitor o escala) se
      abre visible: `KNoteManagementForm`, `AppInfoAlarmsForm` y los PostIt.
- [ ] Los botones `...`/`X` junto a una caja de texto tienen su mismo alto (p. ej. fechas de
      `TaskEditorForm`, carpeta de `NotesFilterParamForm`, quitar filtro del selector de notas).
- [ ] En un PostIt, el icono del menú toma los colores de la barra de título (y el de la caja de la URL en
      modo navegación) y el asa de redimensionar sigue funcionando.

## Impresión y exportación

Los informes se muestran en la previsualización (`ReportPreviewCtrl`/`ReportPreviewForm`, WebView2).

- [ ] File → "Print notes list ..." (y el botón Print de la barra de `KNoteManagementForm`) imprime la
      lista tal como se ve: mismas columnas visibles (también en vista compacta), mismo orden de filas
      tras ordenar por otra columna y respetando el filtro de texto de la lista; en horizontal y con todas
      las columnas dentro de la página. Probar con una carpeta, una búsqueda rápida y un filtro
      estructurado: la cabecera muestra la ruta de la carpeta o el resumen de la búsqueda/filtro (con
      nombres de tipo de nota, carpeta y atributos, no identificadores).
- [ ] File → "Print notes list as book ..." genera portada, índice y un capítulo por nota en el orden de
      la lista; los números de página del índice coinciden con las páginas reales en el PDF guardado
      (probar con notas largas, con imágenes y tablas, y con un título que ocupe dos líneas en el índice).
- [ ] File → "Print selected note details ..." imprime la nota seleccionada (o avisa si no hay ninguna):
      propiedades, descripción con sus imágenes, atributos, recursos con miniatura de las imágenes,
      tareas y notas trazadas; nunca el script de la nota ni de sus atributos, ni sus alarmas.
- [ ] El botón Print del editor de notas (ventana flotante) imprime la nota tal como se ve; tras
      modificarla sin guardar, el informe muestra el aviso "Unsaved changes".
- [ ] En la previsualización: **Print ...** abre el diálogo de impresión, **Save as PDF ...** guarda el
      PDF (márgenes blancos, orientación del informe) y al contestar "Sí" a abrirlo se abre sin que la app
      se cierre; el zoom funciona y los enlaces de una descripción se abren en el navegador.
- [ ] File → "Export notes list to CSV ..." propone un nombre según la carpeta/búsqueda/filtro y guarda
      el mismo contenido que la lista impresa; abierto en Excel con doble clic, las columnas se separan
      solas, los acentos se ven bien y un topic como "- Accesos rápidos" no sale como `#¿NOMBRE?`.
- [ ] Con la lista vacía, imprimir/exportar avisan en vez de abrir la previsualización o el diálogo.
- [ ] Los diálogos de guardar (PDF y CSV) empiezan en la última carpeta usada, también tras reiniciar.

## Otros

- [ ] Opciones de la aplicación (`OptionsEditorCtrl`) se guardan y se recargan correctamente al
      reiniciar la app.
- [ ] KNoteAIAssistant (`KNoteAIAssistantCtrl`), con al menos un proveedor de IA configurado, sigue
      funcionando (streaming/completion, catálogo de prompts, gestión de proveedores).
