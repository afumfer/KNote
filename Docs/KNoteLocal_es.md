# KNote Local

KNote Local es la aplicación Web de KNote funcionando entera dentro de tu propio ordenador: sin internet, con tus notas guardadas en tu equipo, en **Windows, macOS y Linux**. Esta guía explica, con lenguaje sencillo, qué es, cómo funciona y cómo se usa.

## Las formas de usar KNote

KNote se puede usar de tres maneras.

La **aplicación de escritorio** es un programa de Windows de toda la vida: lo instalas, lo abres y trabajas. Guarda tus notas en ficheros de tu ordenador o en una base de datos de tu empresa. Solo funciona en Windows.

La **aplicación Web** se abre desde el navegador (Chrome, Edge, Safari…) escribiendo una dirección. Detrás hay un **servidor**: un ordenador siempre encendido en algún lugar (una oficina, internet) que guarda las notas y se las entrega al navegador cuando las pide. Funciona en cualquier sistema y desde cualquier sitio, pero alguien tiene que mantener ese servidor y, sin conexión, no puedes entrar.

**KNote Local** junta lo mejor de las dos: la misma aplicación Web, pero funcionando dentro de tu propio ordenador, sin internet, en Windows, Mac o Linux.

## La idea central: un servidor que vive en tu ordenador

Aunque la palabra "servidor" suene a máquina grande en una sala refrigerada, un servidor es simplemente **un programa que espera peticiones y las contesta**. Puede funcionar en cualquier ordenador, también en tu portátil.

En KNote Local, ese programa servidor se ejecuta en tu equipo de forma discreta, sin ventanas. Tu navegador le habla como si estuviera lejos, pero la conversación nunca sale de tu ordenador.

Para eso se usa una dirección especial, **`127.0.0.1`**, también llamada **`localhost`**, que siempre significa "este mismo ordenador". Nadie desde fuera puede llegar a ella: es como un teléfono interno que solo suena dentro de casa.

Además de la dirección hace falta un **puerto**. Un ordenador puede tener muchos programas esperando conexiones a la vez, y el puerto es el número que distingue a cada uno, igual que en un edificio todos comparten la dirección de la calle pero cada vivienda tiene su número de puerta. KNote Local usa el puerto **5280** (u otro libre, si ese está ocupado), así que su dirección es algo como `http://127.0.0.1:5280/KNote`.

La aplicación que ves es **exactamente la misma que la versión Web**: mismas pantallas, mismos botones, mismas funciones. No es una copia ni una versión recortada.

## El lanzador: el botón que lo pone todo en marcha

Lo que abres desde el menú de inicio o el escritorio, con el icono de KNote, es el **lanzador**: un programa pequeño que en un par de segundos hace por ti lo siguiente.

1. Mira si el servidor de KNote ya está funcionando en tu ordenador. Para saberlo le hace una pregunta muy sencilla, la **comprobación de salud**: "¿estás ahí y eres KNote?". Si contesta que sí, no hace falta arrancarlo otra vez.
2. Si no está funcionando, lo arranca en segundo plano, de forma invisible.
3. Espera a que esté listo.
4. Abre tu navegador directamente en KNote.

El servidor sigue funcionando aunque cierres el navegador, igual que la música de un reproductor sigue sonando aunque minimices su ventana; así, la próxima vez KNote aparece al instante. Si lo eliges al instalar, el servidor arranca solo al iniciar sesión en el ordenador, sin abrir el navegador, para estar siempre listo.

KNote también se puede abrir en una **ventana de aplicación**: una ventana del navegador sin barra de direcciones ni pestañas, que parece un programa normal. Y desde Chrome, Edge o Safari puedes **"instalar KNote como aplicación"**, con su propio icono en la barra de tareas o en el Dock del Mac.

Si algo va mal, por ejemplo que otro programa ya esté usando el puerto de KNote, el lanzador te muestra un aviso sencillo que explica qué ocurre y cómo arreglarlo (por ejemplo, eligiendo otro número de puerto).

## Dónde se guardan tus cosas

KNote Local separa **el programa** de **tus datos**.

El programa se instala en una carpeta de aplicaciones; cuando hay una versión nueva, se sustituye esa carpeta y ya está.

Tus datos van a una carpeta personal tuya, dentro de tu perfil de usuario:

| Sistema | Carpeta de datos |
|---|---|
| Windows | `%LocalAppData%\KNote\Server` |
| macOS | `~/Library/Application Support/KNote/Server` |
| Linux | `~/.local/share/KNote/Server` |

Dentro de ella están:

- **Tus bases de datos** (carpeta `Data`): los ficheros donde están guardadas las notas. KNote usa **SQLite**, una base de datos que cabe entera en un solo fichero, sin instalar nada más. Puedes copiarla, guardarla en una copia de seguridad o llevarla a otro ordenador como cualquier otro fichero.
- **Los recursos de las notas** (carpeta `Resources`): imágenes y ficheros adjuntos.
- **El fichero de configuración**: dónde están tus bases de datos, tus proveedores de inteligencia artificial, tu cuenta de correo, el puerto…
- **Las claves de cifrado** (carpeta `Keys`), de las que se habla más abajo.
- **Los registros** (carpeta `Logs`, en inglés *logs*): un diario en el que el programa apunta lo que hace y los errores que encuentra. Sirven para averiguar qué pasó si algo falla.

Como los datos están separados del programa, **actualizar o reinstalar KNote nunca toca tus notas**. Y si en el mismo ordenador hay varias personas, cada una con su usuario del sistema, cada una tiene su propia carpeta y su propio KNote, sin mezclarse.

## El primer arranque

La primera vez que abres KNote Local no tienes que configurar nada. El servidor:

- crea tu carpeta de datos;
- prepara una base de datos vacía lista para usar;
- genera una **clave secreta** propia de tu instalación. Sirve para firmar tu "pase" de entrada cuando inicias sesión, de forma que nadie pueda fabricar uno falso. No viene escrita de fábrica: se crea al azar en tu equipo;
- busca un puerto libre (el 5280, o el siguiente si está ocupado).

Después te pide que te registres. **La primera persona que se registra queda como administradora**, con permiso para configurarlo todo.

La base de datos nueva trae un usuario administrador de ejemplo, `adminKNote`, con una contraseña conocida. En KNote Local solo se puede entrar desde tu ordenador, así que el riesgo es pequeño, pero conviene cambiarle la contraseña (o deshabilitarlo) al empezar.

## Repositorios: tus distintas libretas de notas

Un **repositorio** es una base de datos de notas completa, como una libreta independiente: puedes tener uno para el trabajo y otro personal. Al iniciar sesión eliges con cuál trabajar, y puedes cambiar cuando quieras.

La persona administradora gestiona sus repositorios desde la pantalla **This computer** (ver más abajo):

- **Crear** uno nuevo, vacío.
- **Vincular** uno que ya exista, indicando dónde está el fichero; por ejemplo, el que usas en la aplicación de escritorio. Los dos programas pueden usarlo a la vez sin estropearlo: la base de datos está preparada para que la lean y escriban varios programas al mismo tiempo. Al vincularlo indicas también la carpeta de sus recursos (imágenes y adjuntos).
- **Vincular** una base de datos SQL Server de tu empresa, si la tienes.
- **Cambiar el nombre** con el que se muestra.
- **Desvincular** uno que ya no quieras ver. Solo lo quita de la lista: **nunca borra el fichero**.

Los cambios se aplican al momento, sin reiniciar nada.

**Si vinculas la base de datos de la aplicación de escritorio**: en la aplicación Web siempre se entra con usuario y contraseña. Si en la aplicación de escritorio entras con tu cuenta de Windows, sin contraseña, primero ponte una desde allí (**My account → Change password**); después podrás entrar con ella en KNote Local.

## La pantalla "This computer"

En el panel de administración hay una pestaña, **This computer** ("Este equipo"), que solo aparece cuando KNote funciona en modo local y solo para quien es administrador. En la versión Web de un servidor normal no aparece, porque allí no tiene sentido configurar "el equipo".

Desde ella se gestionan:

- **Los repositorios**, como se explica arriba.
- **Los proveedores de inteligencia artificial**: los servicios (OpenAI, Anthropic u Ollama) que usa el asistente de KNote, con sus **claves de acceso**. Una clave de acceso es como la contraseña que te da uno de esos servicios para que puedas usarlo (y, a menudo, para cobrarte). Una vez guardada, la clave nunca se vuelve a mostrar.
- **La cuenta de correo** (el servidor de envío, llamado **SMTP**) para que KNote pueda enviar las alarmas por correo, y el interruptor que activa ese envío.
- **El puerto**, por si necesitas cambiarlo (se aplica la próxima vez que arranque el servidor).
- Un botón para **detener el servidor local** si quieres apagarlo del todo.

Por seguridad, esta pantalla solo funciona desde el propio ordenador.

## Tus contraseñas y claves, guardadas bajo llave

En el fichero de configuración hay datos delicados: la clave secreta de tu instalación, las claves de los servicios de inteligencia artificial, la contraseña del correo y, si las usas, las contraseñas de bases de datos SQL Server. Guardarlos tal cual, en texto legible, sería como dejar las llaves de casa debajo del felpudo.

Por eso se guardan **cifrados**. Cifrar es transformar un texto en algo ilegible que solo se puede volver a leer con una clave especial. Las claves de cifrado se guardan en tu carpeta de datos y se conservan aunque reinstales KNote.

Hay una diferencia entre sistemas que conviene saber:

- **En Windows**, esas claves de cifrado se protegen a su vez con la protección de tu propia cuenta de Windows: solo tu usuario, en tu ordenador, puede usarlas. Aunque alguien copiara tus ficheros a otro equipo, no podría leer los secretos.
- **En Mac y Linux**, la protección depende de los permisos de tu carpeta personal: otros usuarios del ordenador no pueden entrar en ella, pero las claves no tienen esa capa extra.

## Las alarmas, también en local

Las alarmas funcionan igual que en la aplicación Web. Las de aviso en la aplicación (*Application info*) aparecen en la campana de la parte superior y, como la conexión con tu propio ordenador se considera segura, el navegador permite las notificaciones del sistema: las ves aunque KNote no esté en primer plano.

Las alarmas por correo (*Email*) las envía el propio servidor local si configuras tu cuenta de correo y activas el interruptor. Viene **apagado de serie**, porque la aplicación de escritorio también puede enviarlas; si lo activas, desmarca **Process Email alarms** en las opciones de la aplicación de escritorio para que salgan todas desde un mismo sitio.

## Seguridad sin complicaciones

Además de la dirección `127.0.0.1`, que ya impide que nadie de fuera se conecte, KNote Local tiene una protección menos evidente. Existe un truco con el que una página web maliciosa podría intentar hablar con programas de tu propio ordenador haciéndose pasar por ellos. El servidor local solo acepta peticiones dirigidas expresamente a `localhost` o a `127.0.0.1` y rechaza cualquier otra: es como un portero que solo abre a quien llama por el nombre exacto de la casa.

## Instalar, actualizar y desinstalar

KNote Local no necesita instaladores de pago ni herramientas de terceros. El único requisito previo es tener instalados el **.NET Runtime 10** y el **ASP.NET Core Runtime 10**, dos componentes gratuitos de Microsoft: el "motor" sobre el que funciona KNote, igual que algunos juegos necesitan un componente gráfico concreto. Se descargan de la web oficial de Microsoft (<https://dotnet.microsoft.com/download/dotnet/10.0>) y se instalan una sola vez. En **Windows**, lo más sencillo es el **Hosting Bundle** del bloque ASP.NET Core Runtime, que trae los dos (el instalador normal del ASP.NET Core Runtime no trae el .NET Runtime); en **Mac**, el instalador del **SDK 10**, que también trae los dos; en **Linux**, el paquete `aspnetcore-runtime-10.0` de tu distribución. El `README_es.md` del paquete lo explica paso a paso.

KNote se distribuye como un paquete único, el mismo para los tres sistemas, con unos pequeños **scripts** de instalación (un *script* es una lista de instrucciones que el ordenador ejecuta automáticamente, como una receta):

- **Windows**: el script copia el programa, crea el acceso en el menú de inicio con el icono de KNote y, si quieres, hace que arranque al iniciar sesión.
- **Mac**: crea la aplicación `KNote.app`, que puedes arrastrar al Dock, y opcionalmente la arranca al iniciar sesión.
- **Linux**: crea el acceso en el menú de aplicaciones y, opcionalmente, el arranque automático.

Como el programa no está firmado digitalmente (la firma es un certificado de pago que identifica al fabricante), Windows o Mac pueden mostrar al principio un aviso de "programa de origen desconocido". Los scripts reducen esos avisos; si aun así aparece, confirma que quieres abrirlo.

**Actualizar** es sencillo: se detiene el servidor, se sustituye la carpeta del programa por la nueva y se vuelve a abrir. Tus notas no se tocan. **Desinstalar** quita el programa y sus accesos; tu carpeta de datos se conserva, por si vuelves a instalarlo (bórrala a mano si ya no la quieres).

### Los comandos, paso a paso

Descomprime el paquete y, desde su carpeta, ejecuta el script de tu sistema:

| Sistema | Instalar | Desinstalar | Dónde queda KNote |
|---|---|---|---|
| Windows | `powershell -ExecutionPolicy Bypass -File install-windows.ps1` | `uninstall-windows.ps1` | Menú de inicio, **KNote** |
| Mac | `bash install-macos.sh` | `bash uninstall-macos.sh` | `~/Applications/KNote.app` (y Launchpad) |
| Linux | `bash install-linux.sh` | `bash uninstall-linux.sh` | Menú de aplicaciones, **KNote** |

Para que el servidor arranque al iniciar sesión, añade `-StartAtSignIn` (Windows) o `--start-at-login` (Mac, Linux). En Windows, `-AppWindow` hace que el acceso del menú de inicio abra KNote en una ventana de aplicación. Para actualizar, instala el paquete nuevo de la misma forma.

## Si algo va mal

- **Otro programa usa el puerto de KNote**: el lanzador te lo dice. Abre KNote una vez con otro puerto, `KNote.Launcher --port 5300` (en Mac y Linux, `dotnet KNote.Launcher.dll --port 5300` desde la carpeta donde está instalado); KNote lo recuerda para las próximas veces.
- **Falta el .NET Runtime 10 o el ASP.NET Core Runtime 10**: el lanzador te lo dice (o, si falta el .NET Runtime, Windows dice que hay que instalar .NET para ejecutar la aplicación); instala lo que se indica arriba (en Windows, el Hosting Bundle) y vuelve a abrir KNote.
- **Detener el servidor local**: **panel de administración → This computer → Stop local server**, o `KNote.Launcher --stop`.
- **Para averiguar qué pasó**: los registros del lanzador (`launcher.log`) y del servidor están en la carpeta `Logs` de tu carpeta de datos.

## Convivencia con lo que ya existe

**La aplicación de escritorio no desaparece.** Sigue siendo una opción más y puede convivir con KNote Local en el mismo ordenador, incluso usando las mismas bases de datos a la vez.

**La versión Web en un servidor normal sigue funcionando igual.** Todo lo propio del modo local solo se activa cuando el servidor arranca en modo local.

## Lo que KNote Local no hace (por ahora)

KNote Local está pensado para usarse **en tu propio ordenador**. Que otros equipos de casa se conecten a él no está disponible: exigiría resolver antes la conexión segura (HTTPS), el cortafuegos y que el ordenador no se suspenda. Tampoco hay, por ahora, actualización automática ni instaladores firmados.

---

En resumen: **KNote Local es la misma aplicación Web que ya conoces, funcionando por completo dentro de tu ordenador.** Un lanzador la pone en marcha con un clic, tus notas se guardan en tu carpeta personal, tus claves van cifradas y una pantalla te permite organizar tus libretas y tu configuración, todo sin necesitar internet.
