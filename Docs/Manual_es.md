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
| `KNoteData.config` | Lo que tú configuras: cómo entras, repositorios, proveedores de IA, cuenta de correo (SMTP), opciones de alarmas y autoguardado, chat hub y ajustes de ServerCOM. | Solo cuando cambias un ajuste. |
| `KNoteState.config` | Lo que KaNote recuerda por sí mismo: último usuario con el que entraste (nunca la contraseña), último repositorio y carpeta activos, posición y tamaño de las ventanas, disposición de las listas, paneles visibles y las filas del panel de alarmas. | Mientras usas la aplicación. |

Borrar `KNoteState.config` solo restablece la disposición de las ventanas. Borrar `KNoteData.config` hace que KaNote arranque como si fuera la primera vez: tus bases de datos no se borran, pero se pierde la lista de repositorios y de proveedores de IA.

**Contraseñas y API keys.** La contraseña de la cuenta de correo, las API keys de los proveedores de IA y la contraseña incluida en la cadena de conexión de un repositorio (autenticación SQL) se guardan cifradas con la protección de datos de Windows, ligadas a tu usuario de Windows y a este equipo. Las cadenas de conexión sin contraseña (SQLite, autenticación de Windows) siguen siendo legibles. Si copias `KNoteData.config` a otro equipo o usuario, esos secretos no se pueden descifrar: KaNote te indica al arrancar cuáles debes volver a introducir, y el resto se carga con normalidad. Un secreto escrito en texto plano a mano en el fichero se acepta y se cifra la siguiente vez que se guarde.

**Copias de seguridad.** Cada vez que se guarda un fichero, la versión anterior se conserva a su lado como `KNoteData.config.bak` / `KNoteState.config.bak`. Si un fichero no se puede leer, KaNote carga su `.bak` y te avisa.

**Actualización desde una versión anterior.** Las versiones anteriores guardaban todo en un único `KNoteData.config`. La primera vez que arranca esta versión, convierte ese fichero automáticamente en los dos anteriores, conservando todos tus valores (las contraseñas y API keys se conservan, ahora cifradas). Se guarda una copia del fichero antiguo, **sin** contraseñas ni API keys, como `KNoteData.config.v1.bak`.

**Volver a una versión anterior.** Las versiones anteriores no pueden leer los ficheros nuevos y fallarán al arrancar. Para volver: cierra KaNote, guarda en otro sitio una copia del `KNoteData.config` actual, renombra `KNoteData.config.v1.bak` a `KNoteData.config`, borra `KNoteState.config`, arranca la versión anterior y vuelve a introducir tus contraseñas y API keys.

### Acceso, usuarios y roles (aplicación de escritorio)

**Cómo entras.** Por defecto la aplicación de escritorio te identifica con tu **cuenta de Windows**, sin pedirte nada (como la seguridad integrada de SQL Server). En **Options → Sign in** puedes elegir en su lugar **With a KNote user name and password**; el cambio se aplica la próxima vez que arranque KaNote (te ofrece reiniciar). A partir de entonces, KaNote pide un usuario y una contraseña al arrancar, proponiendo el último usuario usado. La ventana de acceso tiene también un enlace **Use my Windows account instead**, que entra con la cuenta de Windows y vuelve a dejar la opción como estaba.

**Cada repositorio tiene sus propios usuarios.** Al arrancar KaNote, y siempre que vinculas o creas un repositorio, se comprueba tu usuario en ese repositorio:

| Situación | Qué pasa |
|---|---|
| Estás registrado en él | El repositorio se vincula. Con un usuario de KNote, la contraseña también tiene que coincidir. |
| No estás registrado en él | La ventana **Register user** te pide el nombre completo y el correo (y una contraseña). Si entras con un usuario de KNote, la contraseña es la del acceso, para que las mismas credenciales abran todos tus repositorios. |
| Contraseña incorrecta, usuario deshabilitado o registro cancelado | El repositorio no está disponible en esta sesión. Se mantiene en tu configuración y se vuelve a intentar en el siguiente arranque; KaNote te dice el motivo al abrirse la ventana principal. |

Si ninguno de tus repositorios te acepta, KaNote te deja reintentar (volviendo a pedir el usuario, si entras con uno de KNote) o cerrar.

**Roles.** Tu rol es por repositorio, así que puedes ser Admin de tu repositorio personal y Guest en uno compartido. Los roles son jerárquicos: cada uno puede hacer todo lo de los anteriores, y algo más.

| Rol | Qué añade |
|---|---|
| **Guest** | Navegar por carpetas y notas, buscar y filtrar, abrir notas para leerlas, vincular y desvincular repositorios, ayuda y "acerca de". |
| **Staff** | Crear, editar y borrar notas (y sus tareas, recursos, alarmas y notas trazadas), post-its, mover notas y cambiar etiquetas, imprimir y exportar a CSV, alarmas de información de la aplicación, opciones, proveedores de IA, chat y asistente de IA. |
| **Project manager** | Crear, editar y borrar carpetas, consola de KntScript y ejecutar el código de las notas, asistentes de KNote del catálogo. |
| **Admin** | Administrar el repositorio (usuarios, tipos de nota, atributos, tipos de traza), crear repositorios, servidor de puerto COM y herramientas del laboratorio. |

Las opciones que no pertenecen a un repositorio (opciones, IA, chat, scripts, crear un repositorio...) usan tu rol más alto entre tus repositorios.

**Quién recibe qué rol.** El primer usuario que se registra en un repositorio donde `adminKNote` sigue siendo el único Admin pasa a ser Admin también: quien crea un repositorio nuevo lo administra. A partir de ahí, los usuarios nuevos se registran como **Guest** hasta que un Admin les sube el rol en **Repository → Users**.

**Dónde se ve.** La barra de estado de la ventana de gestión muestra tu usuario, tu rol en el repositorio activo (cambia al pasar a una carpeta de otro repositorio) y tu rol de aplicación, p. ej. `jdoe · Personal repository: Admin · Application: Admin`. Su tooltip muestra cómo has entrado y tu rol en cada repositorio vinculado.

**Cuando una opción no es para tu rol.** Todos los menús siguen disponibles; elegir uno que tu rol no permite solo te indica qué rol hace falta. Una nota abierta por un Guest se abre para leerla: KaNote lo avisa y los cambios no se pueden guardar. Los procesos automáticos (reabrir post-its, alarmas, alarmas de script) se saltan los repositorios donde tu rol no los permite.

**Actualización desde una versión anterior.** La primera vez que se abre una base de datos con esta versión se actualiza automáticamente: el rol *Public* pasa a llamarse **Guest** (así que los usuarios registrados como *Public* solo pueden leer hasta que un Admin los sube a Staff), la errata *ProjecManager* pasa a **ProjectManager** y el usuario de ejemplo `owner` se borra si nunca se usó. Las versiones anteriores no conocen los nombres nuevos de los roles; guarda una copia de una base de datos compartida antes de abrirla con esta versión.

Estos permisos los aplica la aplicación. Quien se conecte a la base de datos directamente, con su cadena de conexión, no queda limitado por ellos: protege también las bases de datos compartidas con los permisos de la propia base de datos.

### Impresión y exportación (aplicación de escritorio)

El menú **File** de la ventana de gestión tiene estas opciones, todas sobre la lista de notas tal como la ves: las notas de la carpeta seleccionada, o el resultado de una búsqueda o de un filtro, en el orden y con las columnas que se muestran en pantalla (incluidas la vista compacta y el filtro de la lista).

| Opción | Resultado |
|---|---|
| **Print notes list ...** | La lista como una tabla, en horizontal, encabezada por la ruta de la carpeta o por un resumen de la búsqueda o el filtro. Todas las columnas visibles caben en el ancho de la página. El botón **Print** de la barra de herramientas hace lo mismo. |
| **Print notes list as book ...** | Un libro: portada, índice con números de página y un capítulo por cada nota, en el orden de la lista. El título del capítulo es el topic de la nota y su texto, la descripción de la nota. |
| **Print selected note details ...** | Toda la información de la nota seleccionada, tal como está guardada: propiedades, descripción, atributos, recursos (con una miniatura de las imágenes), tareas y notas trazadas. No se imprimen ni el script de la nota ni sus alarmas. |
| **Export notes list to CSV ...** | El mismo contenido que la lista impresa, como fichero CSV. |

El editor de notas tiene su propio botón **Print**, que imprime el detalle de la nota que se está editando tal como la ves. Si tiene cambios aún sin guardar, el informe lo indica.

**Previsualización, impresora y PDF.** Cada impresión abre primero una ventana de previsualización. Desde ella, **Print ...** abre el diálogo de impresión (para elegir la impresora, o *Guardar como PDF*, que tiene además su propia vista previa de las páginas) y **Save as PDF ...** guarda el informe directamente como fichero PDF. El libro tarda un poco más en abrirse: se pagina una vez para conocer los números de página de su índice.

**Ficheros CSV.** El nombre de fichero propuesto describe la carpeta, búsqueda o filtro de la lista. El fichero usa el separador de listas de tu configuración regional (`;` en español, por ejemplo) y la codificación UTF-8, de modo que Excel lo abre directamente con doble clic. Un texto que empieza por `=`, `+`, `-` o `@` (p. ej. un topic como `- Accesos rápidos`) se escribe con un apóstrofo delante, para que las hojas de cálculo lo muestren como texto en vez de intentar ejecutarlo como fórmula.

Los diálogos de guardar (PDF y CSV) empiezan en la carpeta donde guardaste uno de ellos por última vez.

TODO: ...