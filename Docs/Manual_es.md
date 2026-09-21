# Manual KaNote

## Introducción

KaNote es un gestor de notas y tareas minimalista. Las tareas o notas se almacenan en bases de datos y se pueden organizar en carpetas temáticas. Estas notas pueden contener diferentes recursos (imágenes, archivos adjuntos, ...), vincular diversas alarmas y enriquecerse con atributos personalizados. KaNote integra también un lenguaje minimalista con el que puedes automatizar tareas. Además de gestor de tareas puedes darle otros usos a KaNote, por ejemplo un pequeño gestor de contenido. 

Se soportan dos motores diferentes de bases de datos, Sqlite para tareas personales y SQL server para uso corporativo. 

KaNote tiene también dos front-end diferentes, una aplicación windows de escritorio a la vieja escuela (WinForm app) y un aplicativo Web (Blazor app).

TODO: ...


## Guía de usuario

### Ficheros de configuración (aplicación de escritorio)

La aplicación de escritorio de Windows guarda su configuración en la carpeta `%LocalAppData%\KNote`, en dos ficheros XML:

| Fichero | Qué contiene | Cuándo cambia |
|---|---|---|
| `KNoteData.config` | Lo que tú configuras: repositorios, proveedores de IA, cuenta de correo (SMTP), opciones de alarmas y autoguardado, chat hub y ajustes de ServerCOM. | Solo cuando cambias un ajuste. |
| `KNoteState.config` | Lo que KaNote recuerda por sí mismo: último repositorio y carpeta activos, posición y tamaño de las ventanas, disposición de las listas, paneles visibles y las filas del panel de alarmas. | Mientras usas la aplicación. |

Borrar `KNoteState.config` solo restablece la disposición de las ventanas. Borrar `KNoteData.config` hace que KaNote arranque como si fuera la primera vez: tus bases de datos no se borran, pero se pierde la lista de repositorios y de proveedores de IA.

**Contraseñas y API keys.** La contraseña de la cuenta de correo, las API keys de los proveedores de IA y la contraseña incluida en la cadena de conexión de un repositorio (autenticación SQL) se guardan cifradas con la protección de datos de Windows, ligadas a tu usuario de Windows y a este equipo. Las cadenas de conexión sin contraseña (SQLite, autenticación de Windows) siguen siendo legibles. Si copias `KNoteData.config` a otro equipo o usuario, esos secretos no se pueden descifrar: KaNote te indica al arrancar cuáles debes volver a introducir, y el resto se carga con normalidad. Un secreto escrito en texto plano a mano en el fichero se acepta y se cifra la siguiente vez que se guarde.

**Copias de seguridad.** Cada vez que se guarda un fichero, la versión anterior se conserva a su lado como `KNoteData.config.bak` / `KNoteState.config.bak`. Si un fichero no se puede leer, KaNote carga su `.bak` y te avisa.

**Actualización desde una versión anterior.** Las versiones anteriores guardaban todo en un único `KNoteData.config`. La primera vez que arranca esta versión, convierte ese fichero automáticamente en los dos anteriores, conservando todos tus valores (las contraseñas y API keys se conservan, ahora cifradas). Se guarda una copia del fichero antiguo, **sin** contraseñas ni API keys, como `KNoteData.config.v1.bak`.

**Volver a una versión anterior.** Las versiones anteriores no pueden leer los ficheros nuevos y fallarán al arrancar. Para volver: cierra KaNote, guarda en otro sitio una copia del `KNoteData.config` actual, renombra `KNoteData.config.v1.bak` a `KNoteData.config`, borra `KNoteState.config`, arranca la versión anterior y vuelve a introducir tus contraseñas y API keys.

TODO: ...