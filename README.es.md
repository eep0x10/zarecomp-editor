# ZA Recomp Editor — Español

Editor portable de partidas de **Pokémon Legends: Z-A para ZA Recomp**, con cajas en cuadrícula, equipo lateral y panel de edición. La aplicación se llama **Recomp Pokémon**.

[Descargar para Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [English](README.md) · [Português](README.pt-BR.md)

## Primeros pasos

1. Descarga el ZIP portable de Releases y extráelo en una carpeta con permiso de escritura. Windows x64; no necesitas instalar .NET.
2. Cierra ZA Recomp y abre `RecompPokemon.exe`.
3. Busca las partidas o selecciona `main`. Ubicación habitual: `<carpeta de ZA Recomp>/save/save_0/main`; algunas distribuciones usan `PORT`. Comprueba entrenador y fecha si aparecen varias copias.
4. Selecciona un Pokémon del equipo o una caja para cambiar nivel, naturaleza y shiny. La estrella modifica el borrador; guarda los cambios para escribirlos.
5. Pulsa **+** para añadir especie/forma compatible, nivel, naturaleza y shiny. Opciones adicionales: apodo, género, bola, movimientos, IVs y EVs. Automático utiliza primero el equipo si queda espacio y después una caja desbloqueada con espacio.

Añadir no sobrescribe posiciones ocupadas. No convierte partidas de otros juegos. La interfaz cambia entre PT-BR, inglés y español sin perder borradores; el catálogo y los informes PKHeX siguen en inglés. Tema oscuro con Acrylic solicitado a Windows compatible y superficies opacas como alternativa.

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
