# ZA Recomp Editor — Español

**Editor portátil de partidas de Pokémon Legends: Z-A para ZA Recomp — versión 2.1.1.** Equipo, cajas, creación y edición detallada en una única interfaz web local, oscura y acrílica, con copia de seguridad automática. El encabezado muestra la versión del host en ejecución.

**Corrección 2.1.1:** aplicar un plan Alfa conserva la identidad y la imagen del Pokémon. Los campos nulos de la API significan “mantener el valor actual”; solo se aplican cambios reales. [Evidencia de la corrección](docs/audits/2026-11-13-alpha-preview.md).

[Descargar para Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [English](README.md) · [Português](README.pt-BR.md) · [Guía detallada](docs/EDITING.md)

Windows x64. Extrae el ZIP en una carpeta con permiso de escritura y abre **RecompPokemon.exe**. El EXE abre el navegador en una dirección temporal `127.0.0.1`; no necesitas instalar Node.js, .NET ni un servidor. No incluye archivos del juego ni partidas personales.

![Versión 2.0.3: equipo, caja y editor integrado](docs/screenshots/web/current-desktop.jpg)

Las capturas generales son reales del build 2.0.3 con una partida sintética. Los ejemplos Alfa siguientes muestran la 2.1.0; las propuestas antiguas de diseño son registros históricos.

<details>
<summary>Creación personalizada en el mismo panel</summary>

![Versión 2.0.3: creación integrada con opciones detalladas](docs/screenshots/web/current-add.jpg)

<img src="docs/screenshots/web/current-narrow.jpg" alt="Versión 2.0.3 en pantalla estrecha" width="390">

</details>

## Abrir y editar

Cierra ZA Recomp. Busca una partida o escribe la ruta absoluta de su archivo **main** en el selector integrado. Rutas habituales: `<ZA Recomp>/save/save_0/main` y `<ZA Recomp>/PORT/save/save_0/main`. Comprueba entrenador y fecha cuando haya varias copias.

Selecciona un Pokémon del equipo o de una caja; el editor aparece a la derecha. Cambiar de selección conserva los borradores. **Perfil, Atributos, Movimientos, Origen y Avanzado** permanecen en el mismo panel:

- Identidad, forma persistente, apodo, género, nivel, naturaleza original/menta, tipo variocolor, Poké Ball, objeto y favorito.
- Las 25 bolas seleccionables incluyen PNGs locales disponibles sin conexión; naturaleza original y menta muestran efectos de +10% / −10% o un efecto neutro.
- IVs, EVs y Hyper Training; IV 0–31, EV hasta 252 por estadística y 510 en total.
- Cuatro movimientos y registros Plus; PP e historial de reaprendizaje son de solo lectura.
- Entrenador original, identificadores, idioma y datos del encuentro/obediencia.
- Alfa, escala, título ya obtenido y datos técnicos/informe para consulta.

**Guardar con copia** valida y escribe los borradores del equipo y cajas en una transacción. **Descartar borradores** abandona los cambios pendientes mediante confirmación integrada. Guarda antes de recargar o cerrar la pestaña; los borradores no se escriben automáticamente.

Naturaleza original y menta son distintas. Hay un efecto de menta neutro; sin una sustitución por menta, la naturaleza efectiva sigue a la original. Los tamaños predefinidos sincronizan los escalares nativos/HOME; los botones IV 31/0 eliminan Hyper Training.

## Revisar una conversión Alfa

Alfa abre un plan antes de cambiar el borrador. **Compatible** busca un encuentro Alfa y puede proponer otro lugar/nivel de encuentro, aumentar el nivel actual y corregir movimientos necesarios. Todos los cambios propuestos aparecen en el antes/después explícito. **Personalizado** es una elección explícita que conserva el origen solicitado y ajusta Alfa y sus dependencias de tamaño/IVs. Este modo puede producir un informe PKHeX inválido; el editor muestra los resultados reales.

Revisa el antes/después de origen, nivel, escalares, IVs, Hyper Training y registros Plus, junto con las advertencias y el informe PKHeX. Aplicar el plan solo cambia el borrador; **Guardar con copia** o **Añadir con copia** escribe el resultado. Ambos modos exigen Escala 255 y al menos tres IVs 31. Desmarcar Alfa borra el modo de conversión, sin deshacer otros valores ya aplicados. Un encuentro Alfa compatible no garantiza legalidad completa ni aceptación online.

![Versión 2.1.0: plan Alfa para revisión — partida sintética](docs/screenshots/web/alpha-plan-2.1.0.jpg)

<details>
<summary>Alfa personalizado y pantalla estrecha — versión 2.1.0</summary>

![Modo personalizado explícito y resultados reales de PKHeX](docs/screenshots/web/alpha-custom-2.1.0.jpg)

<img src="docs/screenshots/web/alpha-narrow-2.1.0.jpg" alt="Plan Alfa en pantalla estrecha en la versión 2.1.0" width="390">

</details>

![Versión 2.0.3: controles detallados, bolas y naturalezas — partida sintética](docs/screenshots/web/current-details.jpg)

## Añadir Pokémon personalizado

**+ Añadir Pokémon** abre directamente el selector de las **364 especies** del catálogo actual del motor, ordenadas por ID de la Pokédex Nacional. Las imágenes cargan cuando las tarjetas se acercan al área visible. Busca por nombre o ID numérico exacto (`25` o `#25`); utiliza toque/clic o flechas, Home/End y Enter. Escape vuelve al editor. Una imagen no disponible no elimina la especie del catálogo.

![Versión 2.0.3: catálogo completo y búsqueda por nombre o ID](docs/screenshots/web/current-species.jpg)

La elección abre su primera forma admitida en el mismo editor. **Elegir Pokémon** reabre el selector. Cambiar de especie conserva los demás detalles de creación y restablece movimientos y registros Plus a **Automático** para la nueva especie. Elige nivel, naturaleza, tipo variocolor y bola; activa las opciones que quieras personalizar. **Automático** conserva los valores generados, no significa cero o IVs perfectos.

El destino automático ocupa primero una plaza libre del equipo y después una caja desbloqueada. Nunca sobrescribe Pokémon existentes. Resuelve los borradores de edición antes de añadir. **Añadir con copia escribe inmediatamente**, separado del lote de borradores. Cancelar la creación no escribe nada.

El plan Alfa también admite creación. Compatible propone un encuentro disponible y el aumento de nivel necesario: Charmander requiere al menos nivel 25; Mewtwo no tiene plantilla Alfa compatible en los datos incluidos. Personalizado sigue disponible como elección explícita, con incompatibilidades reales en el informe. Variocolor **Automático** conserva la generación y es distinto de elegir normal, estrella o cuadrado.

## Optimize e idiomas

Optimize propone **solo naturalezas**, con motivos y revisión editable. No cambia IVs, EVs, movimientos ni niveles. El papel del equipo, la megaevolución y las mecánicas de Z-A afectan a la mejor elección.

Interfaz en PT-BR, inglés y español. Movimientos/naturalezas del catálogo e informes PKHeX permanecen en inglés. Se respeta movimiento reducido; el acrílico tiene alternativa opaca. En pantallas estrechas, el equipo se convierte en una franja y las cajas se expanden dentro de la misma página.

## Copias y privacidad

Cada escritura crea una copia completa de la carpeta del guardado en `ZA-Tool-Backups/<fecha>-<id>/`, con manifiesto SHA-256, validación y reemplazo atómico. Se muestra el recibo de la copia. Los IDs de operación evitan duplicar una adición reenviada. Un archivo modificado externamente o resultado incierto exige comprobar el estado, sin reintento automático.

Para restaurar: cierra juego/editor, conserva el archivo actual aparte y copia **main** desde la copia elegida. Cierra siempre el juego antes de guardar; no se detecta todo ejecutable renombrado. La validación no garantiza aceptación por el juego ni compatibilidad online.

El host escucha solo en el propio equipo y utiliza una credencial temporal. No se envían partidas, nombres de entrenador ni apodos. Preferencias/cache permanecen junto al EXE. Las imágenes Pokémon opcionales usan IDs públicos de PokéAPI/GitHub; esos servicios reciben información HTTP normal. El resolver conserva la forma visible y el variocolor exactos, probando artwork, HOME y PNG estándar sin sustituir otra forma ni el color normal. La caché usa `cache/pokemon/v2`; una petición recuperada actualiza todas las copias visibles de esa misma imagen. La ausencia se muestra explícitamente, con reintentos limitados. La edición y las imágenes de bolas funcionan sin conexión; pueden faltar imágenes Pokémon no almacenadas. No se garantiza la disponibilidad de todas las imágenes. No compartas URLs de sesión ni diagnósticos privados.

La referencia interactiva de la API local está en `/api-docs`, con esquema `/openapi/v1.json`. Las escrituras aplican las mismas reglas del editor.

## Desarrollo y licencia

Para compilar: Windows, SDK .NET 10 x64 y Node.js 24/npm; ejecuta `./build.ps1`. Se genera `dist/RecompPokemon.exe` con los assets incorporados. Los usuarios del ZIP no necesitan estas herramientas. Consulta [pruebas y fuentes](README.md#build-from-source) y usa copias desechables.

La verificación local de la 2.1.0 superó **61 pruebas Alfa, 44 HTTP con partida generada, 67 Engine y 39 regresiones del cliente**. `--generated` ejecuta Alfa antes de HTTP; `npm test --prefix WebClient` ejecuta las regresiones del cliente. CI está configurado para estos modos tras compilar; una aprobación local no demuestra un resultado publicado de CI. Las 32 pruebas deterministas de sprites y 24 consultas externas son resultados históricos, sin nueva ejecución para este cambio. Las fuentes externas pueden fallar. Consulta la [auditoría 2.1.0](docs/audits/2026-11-13-alpha-review.md) y la [guía del harness](HostTests/README.md).

Código GPL-3.0-or-later con fuentes correspondientes de PKHeX.Core. Consulta [avisos de terceros](THIRD_PARTY_NOTICES.md) y [licencias del frontend](WEB-THIRD-PARTY-NOTICES.txt). Los recursos y marcas Pokémon pertenecen a sus titulares. Proyecto independiente, sin afiliación a Nintendo, Game Freak, The Pokémon Company, ZA Recomp o PKHeX.
