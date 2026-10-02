# CLAUDE.md

Guía para trabajar en `KntIcons`, la librería de iconos de la UI WinForms de KNote (`ClientWin`,
`HtmlEditorControl` y `KntEditViewControl`). Proyecto hoja: no referencia a ningún otro proyecto.

## Qué resuelve

Los iconos antiguos eran PNG/BMP de tamaño fijo (16/20/24 px) que se veían borrosos o diminutos con el
escalado de Windows. `KntIcons` dibuja cada icono en tiempo de ejecución a partir de una fuente vectorial,
al tamaño exacto en píxeles del DPI de la app, así que se ve nítido a cualquier escala.

- **Fuente**: Fluent UI System Icons, variante *Regular* (Microsoft, licencia MIT, ver `Fonts/LICENSE`),
  incrustada como recurso (`Fonts/FluentSystemIcons-Regular.ttf`). Origen:
  https://github.com/microsoft/fluentui-system-icons, carpeta `fonts/`.
- **Estilo**: monocromo gris oscuro; el color solo aporta significado y solo en unos pocos iconos
  (carpetas, repositorio, PostIt, alarma, ejecutar, parar, borrar). Los colores viven en `KntIconCatalog`,
  cada uno con su tono claro para fondo oscuro (`DarkBackgroundColors`): `GetBitmap` usa ese tono cuando la
  app está en modo oscuro (`Application.IsDarkModeEnabled`, fijo durante todo el proceso). Un color nuevo de
  la paleta necesita también su entrada ahí; los tests de `KntIconProviderTests` exigen contraste 3:1 sobre
  el fondo oscuro.
- **DPI**: `ClientWin` es `SystemAware` (ver su `CLAUDE.md`), así que cada icono se dibuja una sola vez a la
  escala del sistema; Windows reescala la ventana entera si se mueve a un monitor con otra escala.

## Piezas

- `KntIcon` — enum con los iconos **por significado** en la app (`Repository`, `Alarm`, `NewNote`...), no
  por su glifo.
- `KntIconCatalog` (interno) — `KntIcon` → glifo de 16 px y de 20 px (diseños distintos de la fuente) +
  color. El comentario de cada entrada es el nombre Fluent del icono.
- `KntIconProvider` — `GetBitmap(icon, logicalSize, dpi, color?)` (caché: los bitmaps son **compartidos,
  nunca los liberes**). `color` sustituye al del catálogo solo donde hace falta, p. ej. un icono claro sobre
  un fondo oscuro.
- `KntIconExtensions` — `SetKntIcon(...)` para `ToolStripItem`, `ButtonBase` y `PictureBox`, tomando el DPI
  del control. En `ToolStripItem` fija `ImageScaling = None`: el bitmap ya tiene su tamaño final y dejar que
  el ToolStrip lo estire a `ImageScalingSize` lo volvería a emborronar. `ImageList.SetKntIcons(logicalSize,
  dpi, params KntIcon[])` rellena una `ImageList` creada en el diseñador (así la sigue liberando su
  formulario): 32 bits, índice = orden de los argumentos, clave = nombre del `KntIcon`.

## Uso desde una vista

Asigna los iconos en código, tras `InitializeComponent()`, y **no** en el diseñador: así no quedan bitmaps
fijos serializados en los `.resx`.

```csharp
buttonSave.SetKntIcon(KntIcon.Save);
imageListFolders.SetKntIcons(KntIconProvider.DefaultSize, DeviceDpi,
    KntIcon.FolderOpen, KntIcon.Folder, KntIcon.Repository);
```

## Añadir un icono

1. Busca el icono en el catálogo de Fluent (`FluentSystemIcons-Regular.json` o `.html` de la carpeta `fonts/`
   del repo de origen): claves `ic_fluent_{nombre}_16_regular` y `ic_fluent_{nombre}_20_regular`.
2. Añade el miembro a `KntIcon` (por significado) y su entrada a `KntIconCatalog` con los dos codepoints
   (`0` en el de 16 si la fuente no tiene diseño de 16 px) y el color (`Neutral` salvo que aporte significado).
3. `KntIconProviderTests` (en `ClientWin.Tests`) recorre todo el enum: falla si un icono no está en el
   catálogo o no dibuja nada.

Para actualizar la fuente, sustituye `Fonts/FluentSystemIcons-Regular.ttf` por la nueva versión y vuelve a
comprobar los codepoints del catálogo contra el `.json` de esa misma versión (no des por hecho que no
cambian), además de pasar los tests.
