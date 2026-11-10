namespace ZaTool.App;

internal static partial class L
{
    public static string Language { get; set; } = "pt-BR";
    private static readonly Dictionary<string, (string Pt, string Es)> Strings = new()
    {
        ["Language"] = ("Idioma", "Idioma"), ["Lv."] = ("Nv.", "Nv."),
        ["Warnings:"] = ("Avisos:", "Avisos:"),
        ["Save changes"] = ("Salvar alterações", "Guardar cambios"),
        ["Changes are drafts. Click Save changes to write them."] = ("Alterações em rascunho. Clique em Salvar alterações para gravar.", "Cambios en borrador. Pulsa Guardar cambios para escribirlos."),
        ["Choose save"] = ("Escolher save", "Elegir guardado"), ["Find saves"] = ("Localizar saves", "Buscar guardados"), ["Add Pokémon"] = ("Adicionar Pokémon", "Añadir Pokémon"),
        ["Your team"] = ("Sua equipe", "Tu equipo"), ["Box"] = ("Caixa", "Caja"), ["Empty slot"] = ("Espaço vazio", "Espacio vacío"), ["Select a Pokémon"] = ("Selecione um Pokémon", "Selecciona un Pokémon"),
        ["Level"] = ("Nível", "Nivel"), ["Nature"] = ("Natureza", "Naturaleza"), ["Form"] = ("Forma", "Forma"), ["Species"] = ("Espécie", "Especie"), ["Shiny"] = ("Shiny", "Variocolor"),
        ["Save team changes"] = ("Salvar alterações da equipe", "Guardar cambios del equipo"), ["Save box changes"] = ("Salvar alterações das caixas", "Guardar cambios de cajas"),
        ["Discard drafts"] = ("Descartar rascunhos", "Descartar borradores"), ["Optimize team"] = ("Optimize — sugerir naturezas", "Optimize — sugerir naturalezas"), ["Recommendation"] = ("Recomendação", "Recomendación"),
        ["Saved, but could not reload. Reopen the save."] = ("Gravado, mas não foi possível reler. Reabra o save.", "Guardado, pero no se pudo volver a leer. Abre el guardado de nuevo."),
        ["Poké Ball"] = ("Pokébola", "Poké Ball"), ["Alolan"] = ("Alola", "Alola"), ["Galarian"] = ("Galar", "Galar"), ["Hisuian"] = ("Hisui", "Hisui"), ["Paldean"] = ("Paldea", "Paldea"),
        ["Family of Three"] = ("Família de três", "Familia de tres"), ["Family of Four"] = ("Família de quatro", "Familia de cuatro"),
        ["No pending changes."] = ("Nenhuma alteração pendente.", "Sin cambios pendientes."), ["Draft"] = ("Rascunho", "Borrador"), ["Saved"] = ("Salvo", "Guardado"),
        ["Choose a save to view your team and boxes."] = ("Escolha um save para ver sua equipe e caixas.", "Elige un guardado para ver tu equipo y cajas."),
        ["Close the game before saving. Automatic backup on every write."] = ("Feche o jogo antes de salvar. Backup automático a cada gravação.", "Cierra el juego antes de guardar. Copia automática en cada escritura."),
        ["Only the selected area is saved: team or boxes. Other drafts are preserved."] = ("Salva apenas a área selecionada: equipe ou caixas. Outros rascunhos são preservados.", "Solo guarda el área seleccionada: equipo o cajas. Conserva los otros borradores."),
        ["Open backup"] = ("Abrir backup", "Abrir copia"), ["Loading save…"] = ("Lendo save…", "Leyendo guardado…"), ["Finding saves…"] = ("Localizando saves…", "Buscando guardados…"),
        ["No saves found. Choose the main file manually."] = ("Nenhum save encontrado. Escolha o arquivo main manualmente.", "No se encontraron guardados. Elige el archivo main manualmente."),
        ["Select a discovered save to load it."] = ("Selecione um save localizado para carregá-lo.", "Selecciona un guardado encontrado para cargarlo."),
        ["Save loaded. Changes remain drafts until you save."] = ("Save carregado. Alterações ficam em rascunho até você salvar.", "Guardado cargado. Los cambios quedan en borrador hasta guardarlos."),
        ["Save could not be opened: "] = ("Não foi possível abrir o save: ", "No se pudo abrir el guardado: "), ["Operation failed: "] = ("Falha na operação: ", "Error en la operación: "),
        ["Creating backup and saving…"] = ("Criando backup e salvando…", "Creando copia y guardando…"), ["Changes saved."] = ("Alterações salvas.", "Cambios guardados."),
        ["Adding Pokémon with backup…"] = ("Adicionando Pokémon com backup…", "Añadiendo Pokémon con copia…"), ["Pokémon added."] = ("Pokémon adicionado.", "Pokémon añadido."),
        ["Backup: "] = ("Backup: ", "Copia: "), ["External PKHeX report (English):"] = ("Relatório externo PKHeX (inglês):", "Informe externo PKHeX (inglés):"),
        ["Discard unsaved drafts?"] = ("Descartar alterações em rascunho?", "¿Descartar los cambios en borrador?"), ["Unsaved changes"] = ("Alterações não salvas", "Cambios sin guardar"),
        ["Wait for the current operation to finish."] = ("Aguarde a operação atual terminar.", "Espera a que termine la operación actual."),
        ["Save or discard pending drafts before adding a Pokémon."] = ("Salve ou descarte os rascunhos antes de adicionar um Pokémon.", "Guarda o descarta los borradores antes de añadir un Pokémon."),
        ["Preferences could not be saved beside the executable."] = ("Não foi possível salvar preferências ao lado do executável.", "No se pudieron guardar las preferencias junto al ejecutable."),
        ["Select the main file in PORT/save/save_0/main or save/…/main. Drag and drop is supported."] = ("Selecione main em PORT/save/save_0/main ou save/…/main. Você também pode arrastar o arquivo.", "Selecciona main en PORT/save/save_0/main o save/…/main. También puedes arrastrar el archivo."),
        ["Optimize previews heuristic nature recommendations based on stats and moves. There is no universally best nature."] = ("Optimize sugere naturezas pelos atributos e golpes. Não existe uma natureza universalmente melhor.", "Optimize sugiere naturalezas según atributos y movimientos. No hay una naturaleza universalmente mejor."),
        ["Choose a Pokémon to edit. Clicking an empty slot never creates one."] = ("Selecione um Pokémon para editar. Clicar num espaço vazio nunca cria um Pokémon.", "Selecciona un Pokémon para editar. Pulsar un espacio vacío nunca crea uno."),
        ["Artwork unavailable"] = ("Imagem indisponível", "Imagen no disponible"), ["Normal"] = ("Padrão", "Normal"), ["Nickname"] = ("Apelido", "Apodo"), ["Use species name"] = ("Usar nome da espécie", "Usar nombre de especie"),
        ["Gender"] = ("Gênero", "Género"), ["Male"] = ("Macho", "Macho"), ["Female"] = ("Fêmea", "Hembra"), ["Genderless"] = ("Sem gênero", "Sin género"), ["Destination"] = ("Destino", "Destino"),
        ["Automatic: team, then boxes"] = ("Automático: equipe, depois caixas", "Automático: equipo, luego cajas"), ["Team"] = ("Equipe", "Equipo"), ["Boxes"] = ("Caixas", "Cajas"),
        ["Preferred box"] = ("Caixa preferida", "Caja preferida"), ["First available"] = ("Primeira disponível", "Primera disponible"), ["Optional details"] = ("Detalhes opcionais", "Detalles opcionales"),
        ["Choose moves manually"] = ("Escolher golpes manualmente", "Elegir movimientos manualmente"), ["Move"] = ("Golpe", "Movimiento"), ["(Empty)"] = ("(Vazio)", "(Vacío)"),
        ["Customize IVs / EVs"] = ("Personalizar IVs / EVs", "Personalizar IVs / EVs"), ["HP"] = ("HP", "PS"), ["Attack"] = ("Ataque", "Ataque"), ["Defense"] = ("Defesa", "Defensa"), ["Speed"] = ("Velocidade", "Velocidad"), ["Sp. Atk"] = ("Atq. Esp.", "At. Esp."), ["Sp. Def"] = ("Def. Esp.", "Def. Esp."),
        ["Cancel"] = ("Cancelar", "Cancelar"), ["Select a valid value: "] = ("Selecione um valor válido: ", "Selecciona un valor válido: "), ["Total EVs cannot exceed 510."] = ("A soma dos EVs não pode ultrapassar 510.", "Los EVs totales no pueden superar 510."),
        ["IVs 0–31; EVs 0–252 each, 510 total. Automatic moves follow species and level."] = ("IVs 0–31; EVs 0–252 cada, total 510. Golpes automáticos seguem espécie e nível.", "IVs 0–31; EVs 0–252 cada, total 510. Movimientos automáticos según especie y nivel."),
        ["Trainer"] = ("Treinador", "Entrenador"), ["Free slots"] = ("Espaços livres", "Espacios libres"), ["Pending"] = ("Pendentes", "Pendientes"), ["Default"] = ("Padrão", "Normal"),
        ["Nature recommendations are drafts. Review them and save the team when ready."] = ("Recomendações são rascunhos. Revise e salve a equipe quando estiver pronto.", "Las recomendaciones son borradores. Revísalas y guarda el equipo cuando quieras."),
    };
    public static string T(string key) => Language == "en" ? key : Strings.TryGetValue(key, out var value) || DetailStrings.TryGetValue(key, out value) ? Language == "es" ? value.Es : value.Pt : key;
    public static string Status(string source)
    {
        bool externalReport = false;
        return string.Join(Environment.NewLine, source.Split(["\r\n", "\n"], StringSplitOptions.None).Select(line =>
        {
            if (externalReport) return line;
            if (line == "External PKHeX report (English):") { externalReport = true; return T(line); }
            foreach (var key in Strings.Keys.OrderByDescending(x => x.Length)) if (line.StartsWith(key, StringComparison.Ordinal)) return T(key) + (key == "Backup: " ? line[key.Length..] : Backend(line[key.Length..]));
            return Backend(line);
        }));
    }
    public static string Nature(int id)
    {
        string english = ZaTool.Engine.Catalog.Natures.FirstOrDefault(x => x.Id == id)?.Name ?? "";
        string[] pt = ["Forte", "Solitária", "Audaz", "Firme", "Travessa", "Ousada", "Dócil", "Relaxada", "Esperta", "Indiferente", "Tímida", "Apressada", "Séria", "Alegre", "Ingênua", "Modesta", "Moderada", "Tranquila", "Tímida", "Imprudente", "Calma", "Gentil", "Atrevida", "Cuidadosa", "Peculiar"];
        string[] es = ["Fuerte", "Huraña", "Audaz", "Firme", "Pícara", "Osada", "Dócil", "Plácida", "Agitada", "Floja", "Miedosa", "Activa", "Seria", "Alegre", "Ingenua", "Modesta", "Afable", "Mansa", "Tímida", "Alocada", "Serena", "Amable", "Grosera", "Cauta", "Rara"];
        return Language == "en" || id < 0 || id >= 25 ? english : $"{(Language == "es" ? es[id] : pt[id])} ({english})";
    }
    public static string Backend(string text)
    {
        if (Language == "pt-BR") return text;
        // Engine messages are translated here; paths, names and external legality reports stay intact.
        foreach (var pair in BackendMessages.Concat(DetailBackendMessages).OrderByDescending(x => x.Key.Length)) text = text.Replace(pair.Key, Language == "es" ? pair.Value.Es : pair.Value.En, StringComparison.Ordinal);
        return text;
    }
    private static readonly Dictionary<string, (string En, string Es)> BackendMessages = new()
    {
        ["Caixa inválida ou bloqueada."] = ("Invalid or locked box.", "Caja inválida o bloqueada."),
        ["nível abaixo do nível original de encontro; legalidade não garantida."] = ("level is below the original encounter level; legality is not guaranteed.", "nivel inferior al nivel original del encuentro; legalidad no garantizada."),
        ["PKHeX sinaliza legalidade; edição não garante uso online."] = ("PKHeX flagged legality concerns; editing does not guarantee online use.", "PKHeX detecta problemas de legalidad; la edición no garantiza el uso en línea."),
        ["Equipe "] = ("Team ", "Equipo "), [", posição "] = (", slot ", ", posición "),
        ["Selecione posições distintas para editar."] = ("Select distinct slots to edit.", "Selecciona posiciones distintas para editar."),
        ["Posição inválida ou bloqueada."] = ("Invalid or locked slot.", "Posición inválida o bloqueada."),
        ["Natureza inválida."] = ("Invalid nature.", "Naturaleza inválida."),
        ["Informe uma alteração para cada posição selecionada."] = ("Provide a change for each selected slot.", "Indica un cambio para cada posición seleccionada."),
        ["A posição selecionada está vazia."] = ("The selected slot is empty.", "La posición seleccionada está vacía."),
        ["Pokémon tem checksum inválido."] = ("The Pokémon has an invalid checksum.", "El Pokémon tiene un checksum inválido."),
        ["Estrutura da equipe alterada."] = ("Team structure changed.", "La estructura del equipo cambió."),
        ["Bloco alheio alterado: "] = ("Unrelated block changed: ", "Bloque ajeno modificado: "),
        ["Checksum da edição inválido."] = ("Edited Pokémon checksum is invalid.", "El checksum de la edición es inválido."),
        ["Falha ao verificar arquivo temporário."] = ("Temporary file verification failed.", "Falló la verificación del archivo temporal."),
        ["Verificação final falhou. Backup em "] = ("Final verification failed. Backup at ", "Falló la verificación final. Copia en "),
        ["Contagem da equipe inválida."] = ("Invalid team count.", "Cantidad del equipo inválida."),
        ["Não foi possível confirmar a pasta do ZA Recomp aberto. Feche o jogo antes de editar."] = ("The running ZA Recomp folder could not be verified. Close the game before editing.", "No se pudo verificar la carpeta de ZA Recomp abierto. Cierra el juego antes de editar."),
        ["Feche o ZA Recomp antes de editar o save desta instalação."] = ("Close ZA Recomp before editing this installation's save.", "Cierra ZA Recomp antes de editar el guardado de esta instalación."),
        ["Espécie ou forma não disponível em Z-A."] = ("Species or form unavailable in Z-A.", "Especie o forma no disponible en Z-A."),
        ["Gênero inválido para esta forma."] = ("Invalid gender for this form.", "Género inválido para esta forma."),
        ["Pokébola inválida para captura em Z-A."] = ("Invalid capture ball for Z-A.", "Poké Ball de captura inválida para Z-A."),
        ["Apelido deve ter até 12 caracteres, sem controles."] = ("Nickname must have up to 12 characters with no control characters.", "El apodo debe tener hasta 12 caracteres y ningún carácter de control."),
        ["Destino inválido."] = ("Invalid destination.", "Destino inválido."),
        ["Informe quatro golpes, sem repetidos, com ao menos um válido."] = ("Provide four moves, without duplicates and with at least one valid move.", "Indica cuatro movimientos, sin repetidos y con al menos uno válido."),
        ["Outro editor está usando este save. Aguarde e reabra."] = ("Another editor is using this save. Wait and reopen it.", "Otro editor está usando este guardado. Espera y vuelve a abrirlo."),
        ["Falha na contagem da equipe."] = ("Team count verification failed.", "Falló la verificación de cantidad del equipo."),
        ["Estrutura do save alterada."] = ("Save structure changed.", "La estructura del guardado cambió."),
        ["Pokémon gravado não confere."] = ("Written Pokémon verification failed.", "Falló la verificación del Pokémon escrito."),
        ["Um slot existente foi alterado."] = ("An existing slot was changed.", "Una posición existente fue modificada."),
        ["Pasta do save sem diretório pai."] = ("The save folder has no parent directory.", "La carpeta del guardado no tiene directorio padre."),
        ["Backup não aceita links/junções na pasta do save."] = ("Backup does not allow links or junctions in the save folder.", "La copia no admite enlaces o uniones en la carpeta del guardado."),
        ["Backup não conferiu: "] = ("Backup verification failed: ", "Falló la verificación de la copia: "),
        ["Save mudou durante o backup."] = ("Save changed during backup.", "El guardado cambió durante la copia."),
        ["informe seis valores entre 0 e "] = ("provide six values between 0 and ", "indica seis valores entre 0 y "),
        ["O save mudou desde a leitura. Reabra antes de editar."] = ("The save changed since it was loaded. Reopen it before editing.", "El guardado cambió desde la lectura. Vuelve a abrirlo antes de editar."),
        ["O save mudou durante a edição. Operação cancelada."] = ("The save changed while editing. Operation cancelled.", "El guardado cambió durante la edición. Operación cancelada."),
        ["Feche o ZA Recomp antes de editar o save."] = ("Close ZA Recomp before editing the save.", "Cierra ZA Recomp antes de editar el guardado."),
        ["Save corrompido ou checksum inválido."] = ("Corrupt save or invalid checksum.", "Guardado dañado o checksum inválido."),
        ["Selecione um save main de Pokémon Legends: Z-A."] = ("Select a Pokémon Legends: Z-A main save.", "Selecciona un guardado main de Pokémon Legends: Z-A."),
        ["A equipe já tem seis Pokémon. Escolha Caixa ou Automático."] = ("The team already has six Pokémon. Choose Boxes or Automatic.", "El equipo ya tiene seis Pokémon. Elige Cajas o Automático."),
        ["As caixas estão cheias. Nenhum Pokémon foi sobrescrito."] = ("The boxes are full. No Pokémon was overwritten.", "Las cajas están llenas. No se sobrescribió ningún Pokémon."),
        ["Caixa inválida."] = ("Invalid box.", "Caja inválida."), ["Nível deve estar entre 1 e 100."] = ("Level must be between 1 and 100.", "El nivel debe estar entre 1 y 100."),
        ["EVs não podem somar mais de 510."] = ("Total EVs cannot exceed 510.", "Los EVs totales no pueden superar 510."),
        ["Um golpe não pode ser aprendido por esta forma no nível escolhido (nível/TM)."] = ("A move cannot be learned by this form at the selected level (level/TM).", "Esta forma no puede aprender un movimiento al nivel elegido (nivel/MT)."),
        ["Equipe — posição "] = ("Team — slot ", "Equipo — posición "), ["Caixa "] = ("Box ", "Caja "), [" — posição "] = (" — slot ", " — posición "),
    };
}
