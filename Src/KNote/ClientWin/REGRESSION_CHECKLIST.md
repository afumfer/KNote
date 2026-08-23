# Checklist de regresión manual — ClientWin

Checklist de referencia para validar manualmente que una fase del plan de refactorización de
`ClientWin` (ver `ClientWin/CLAUDE.md`) no ha roto ningún caso de uso. No sustituye a los tests
automatizados de `ClientWin.Tests`, los complementa donde todavía no hay cobertura (la mayor parte
de la interacción real con WinForms).

Repetir esta checklist al cerrar cada fase del plan de refactorización, antes de dar la fase por
buena.

## Arranque y configuración

- [ ] La app arranca sin excepciones y muestra `KNoteManagmentForm`.
- [ ] Se puede añadir/editar/eliminar un repositorio (`RepositoryEditorCtrl`) y aparece en la lista
      de servicios configurados.
- [ ] Cambiar entre dos repositorios/bases de datos abiertas simultáneamente actualiza correctamente
      la carpeta y el filtro activos.

## Notas (`NoteEditorCtrl`)

- [ ] Crear una nota nueva desde el menú, guardarla y verla aparecer en el listado.
- [ ] Abrir una nota existente por doble clic, editarla y guardar los cambios.
- [ ] Cancelar la edición de una nota no persiste cambios.
- [ ] Eliminar una nota la hace desaparecer del listado y de cualquier panel embebido abierto.
- [ ] El editor de notas embebido (panel dentro de `KNoteManagmentForm`) funciona igual que una nota
      abierta en ventana flotante.
- [ ] Añadir/editar/eliminar un adjunto (`ResourceEditorCtrl`), una tarea (`TaskEditorCtrl`), un
      mensaje (`MessageEditorCtrl`) y un atributo (`NoteAttributeEditorCtrl`) desde dentro del editor
      de notas.
- [ ] Cambiar el tipo de una nota (`NoteTypesSelectorCtrl`) se refleja correctamente.

## Carpetas y selección

- [ ] Crear/editar/eliminar una carpeta (`FolderEditorCtrl`).
- [ ] Seleccionar una carpeta (`FoldersSelectorCtrl`) actualiza `Store.ActiveFolderWithServiceRef` y
      el listado de notas mostrado.
- [ ] Aplicar un filtro de notas (`FiltersSelectorCtrl`, `NotesSelectorCtrl`) devuelve los resultados
      esperados.

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
- [ ] Las notificaciones tipo "toast" (`Store.ControllerNotification`) se muestran correctamente.

## Otros

- [ ] Opciones de la aplicación (`OptionsEditorCtrl`) se guardan y se recargan correctamente al
      reiniciar la app.
- [ ] Integración con ChatGPT (`KntChatGPTCtrl`), si está configurada, sigue funcionando.
