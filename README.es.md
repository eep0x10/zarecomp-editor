# ZA Recomp Editor — Español

**Editor portable de partidas de Pokémon Legends: Z-A para ZA Recomp.** Explora cajas, edita Pokémon existentes y añade Pokémon personalizados con copias de seguridad automáticas. La aplicación se llama **Recomp Pokémon**.

[Descargar para Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [English — README principal](README.md) · [Português (Brasil)](README.pt-BR.md) · [Guía detallada](docs/EDITING.md#español)

Windows x64, sin instalador ni instalación separada de .NET. Extrae el ZIP en una carpeta escribible o memoria USB. Preferencias y caché se guardan junto al ejecutable. No incluye archivos del juego ni partidas personales.

## Empieza con tu partida

1. Descarga el ZIP portable de [Releases](https://github.com/eep0x10/zarecomp-editor/releases/latest) y extráelo.
2. Cierra ZA Recomp y abre `RecompPokemon.exe`.
3. Busca una partida o selecciona **`main`**. Rutas habituales: `<carpeta de ZA Recomp>/save/save_0/main` o `<carpeta de ZA Recomp>/PORT/save/save_0/main`. Comprueba entrenador y fecha si aparecen varias copias.

El equipo queda a la izquierda, una caja de 30 posiciones en el centro y el Pokémon seleccionado a la derecha.

![Explorador de cajas con equipo y Pokémon seleccionado](docs/screenshots/boxes.png)

Las capturas muestran la aplicación real usando una partida sintética de demostración.

## Edita un Pokémon existente

1. Selecciona un Pokémon del equipo o una caja. Usa los controles rápidos o abre **Editar detalles**.
2. Revisa las cinco pestañas y pulsa **Aplicar al borrador**. Esto **no** escribe la partida. **Cancelar** mantiene intacto el borrador anterior.
3. Revisa los borradores y pulsa **Guardar cambios** para validar y escribir el lote completo con copia de seguridad. **Descartar borradores** abandona los cambios pendientes.

| Pestaña | Contenido |
| --- | --- |
| Identidad | Especie/forma, apodo, género, nivel, naturaleza original y efecto de Mint, estilo shiny, bola, objeto y favorito |
| Atributos | IVs, EVs, Hyper Training, amistad y marcas de colores |
| Movimientos | Cuatro posiciones y registros Plus; PP e historial de relearn son de solo lectura |
| Origen | Entrenador original, IDs internos, idioma, datos del encuentro y obediencia |
| Avanzado | Alpha/tamaño, título ya obtenido, datos técnicos e informe PKHeX original |

<details>
<summary>Ver las cinco pestañas de edición</summary>

**Identidad** — naturaleza original y efecto de Mint son elecciones separadas.

![Pokémon existente: pestaña Identidad](docs/screenshots/details.png)

**Atributos** — IVs, EVs e Hyper Training separados, con el total de EVs visible.

![Pokémon existente: pestaña Atributos](docs/screenshots/attributes.png)

**Movimientos** — elige movimientos compatibles y revisa los registros Plus.

![Pokémon existente: pestaña Movimientos](docs/screenshots/moves.png)

**Origen** — revisa la identidad del entrenador y los datos del encuentro antes de cambiarlos.

![Pokémon existente: pestaña Origen](docs/screenshots/origin.png)

**Avanzado** — consulta datos técnicos preservados y opciones de tamaño/título.

![Pokémon existente: pestaña Avanzado](docs/screenshots/advanced.png)

</details>

## Añade un Pokémon personalizado

1. Guarda o descarta los borradores existentes y pulsa **+ Añadir Pokémon**.
2. Elige especie, forma, nivel, naturaleza original, estilo shiny y bola. Las mismas cinco pestañas ofrecen personalización: **marca un campo opcional para reemplazar su valor automático**; déjalo sin marcar para mantener la generación automática.
3. Elige el destino. **Automático** ocupa primero un hueco del equipo y después una caja desbloqueada. **Equipo** necesita espacio; **Cajas** utiliza una posición disponible sin sobrescribir otro Pokémon.
4. Pulsa **Añadir Pokémon**. Esta acción **escribe inmediatamente con copia de seguridad**; no crea un borrador en la ventana principal. Cancelar el diálogo no escribe nada.

![Añadir Pokémon: identidad y reemplazos opcionales](docs/screenshots/add-pokemon.png)

<details>
<summary>Ver atributos opcionales al añadir</summary>

Activa solo los IVs, EVs, Hyper Training u otras opciones que quieras definir. Un campo sin marcar no reemplaza el valor automático; los controles desactivados no muestran una vista previa de los valores generados.

![Añadir Pokémon: reemplazos opcionales de atributos](docs/screenshots/add-attributes.png)

</details>

La interfaz cambia entre PT-BR, inglés y español sin perder borradores; el catálogo y los informes PKHeX siguen en inglés. Tema oscuro con Acrylic solicitado a Windows compatible y superficies opacas como alternativa.

Consulta la [guía detallada](docs/EDITING.md#español) para campos y límites. No hay editor hexadecimal, creación de huevos ni conversión de otros juegos.

## Optimize y límites

Optimize recomienda **solo naturalezas** según estadísticas base y movimientos físicos/especiales, con explicación y vista previa editable. No cambia movimientos, IVs, EVs ni niveles. La mejor naturaleza depende del papel en el equipo, Mega Evolución, rivales y mecánicas de Z-A.

La validación no garantiza encuentros legítimos ni aceptación online. Revisa el análisis PKHeX tras crear un Pokémon. Cierra siempre el juego: la detección reconoce nombres de procesos conocidos, no todos los ejecutables renombrados.

## Copias de seguridad

Antes de escribir, se copia la carpeta de la partida a una carpeta hermana `ZA-Tool-Backups/<fecha>-<id>/`, con manifiesto SHA-256. Se comprueban cambios externos y se valida el resultado antes de reemplazar `main`.

Para restaurar, cierra juego y editor, conserva el archivo actual por separado y copia el `main` de la copia elegida sobre el editado. Abre el juego para comprobarlo. Conserva también una copia duradera fuera de carpetas temporales o sincronizadas.

## Privacidad y desarrollo

La edición funciona sin conexión. Las imágenes opcionales usan recursos públicos de PokéAPI y caché local; solo se consultan IDs de especie/forma, sin subir partidas, entrenador ni apodos. Los servidores reciben datos habituales de conexión HTTP. Preferencias y caché se guardan junto al ejecutable y necesitan una carpeta escribible para persistir.

Para compilar, instala el SDK .NET 10 en Windows y ejecuta `./build.ps1`; salida en `dist/`. Consulta pruebas y arquitectura en el [README principal](README.md#build-from-source). Usa solo copias desechables y nunca publiques partidas en issues.

Código [GPL-3.0-or-later](LICENSE), con PKHeX.Core incluido y [avisos de terceros](THIRD_PARTY_NOTICES.md). Herramienta independiente, sin afiliación con los titulares de Pokémon, ZA Recomp o PKHeX. No incluye archivos del juego.
