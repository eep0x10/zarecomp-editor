export type Locale = "pt-BR" | "en" | "es";
const words: Record<string, [string, string]> = {
  "Choose optimizations": ["Escolher otimizações", "Elegir optimizaciones"],
  "Optimization options": ["Opções de otimização", "Opciones de optimización"],
  "Recommended nature (Mint)": [
    "Natureza recomendada (Mint)",
    "Naturaleza recomendada (Menta)",
  ],
  "Alpha with at least 3 perfect IVs": [
    "Alpha com ao menos 3 IVs perfeitos",
    "Alfa con al menos 3 IVs perfectos",
  ],
  "All IVs at 31": ["Todos os IVs em 31", "Todos los IVs en 31"],
  "All shiny": ["Todos shiny", "Todos variocolor"],
  "Maximum size": ["Tamanho máximo", "Tamaño máximo"],
  "Maximum friendship (255)": ["Amizade máxima (255)", "Amistad máxima (255)"],
  "Level 100": ["Nível 100", "Nivel 100"],
  "Choose one or more changes. Preview first, apply to drafts, then save with backup.":
    [
      "Escolha uma ou mais alterações. Confira a prévia, aplique aos rascunhos e depois salve com backup.",
      "Elige uno o varios cambios. Revisa la vista previa, aplícalos a los borradores y después guarda con copia.",
    ],
  "Recommendations are stable for each species/form and apply as a Mint. Original nature is preserved.":
    [
      "A recomendação é estável por espécie/forma e é aplicada como Mint. A natureza original é preservada.",
      "La recomendación es estable por especie/forma y se aplica como menta. Se conserva la naturaleza original.",
    ],
  "Custom Alpha preserves origin and may remain incompatible. The actual PKHeX report will be shown.":
    [
      "Alpha personalizado preserva a origem e pode continuar incompatível. O relatório real do PKHeX será mostrado.",
      "Alfa personalizado conserva el origen y puede seguir siendo incompatible. Se mostrará el informe real de PKHeX.",
    ],
  "Planning team changes…": [
    "Planejando alterações da equipe…",
    "Planificando cambios del equipo…",
  ],
  "Preview team changes": [
    "Prévia das alterações da equipe",
    "Vista previa de cambios del equipo",
  ],
  "Cancel preview": ["Cancelar prévia", "Cancelar vista previa"],
  "Select at least one option.": [
    "Selecione ao menos uma opção.",
    "Selecciona al menos una opción.",
  ],
  "Pokémon reviewed": ["Pokémon revisados", "Pokémon revisados"],
  "with proposed changes": [
    "com alterações propostas",
    "con cambios propuestos",
  ],
  "Nothing was applied. At least one Pokémon has no available plan. Adjust the options or review Alpha mode before trying again.":
    [
      "Nada foi aplicado. Ao menos um Pokémon não tem plano disponível. Ajuste as opções ou confira o modo Alpha antes de tentar novamente.",
      "No se aplicó nada. Al menos un Pokémon no tiene un plan disponible. Ajusta las opciones o revisa el modo Alfa antes de intentarlo de nuevo.",
    ],
  Available: ["Disponível", "Disponible"],
  Unavailable: ["Indisponível", "No disponible"],
  "Apply team plan to drafts": [
    "Aplicar plano da equipe aos rascunhos",
    "Aplicar plan del equipo a los borradores",
  ],
  "Close optimizer": ["Fechar otimização", "Cerrar optimización"],
  "Inputs changed. Generate a new preview before applying.": [
    "Os dados mudaram. Gere uma nova prévia antes de aplicar.",
    "Los datos cambiaron. Genera otra vista previa antes de aplicar.",
  ],
  "The team preview could not be applied. No drafts were changed. Generate a new preview.":
    [
      "Não foi possível aplicar a prévia da equipe. Nenhum rascunho foi alterado. Gere uma nova prévia.",
      "No se pudo aplicar la vista previa del equipo. No se modificó ningún borrador. Genera otra vista previa.",
    ],
  "Pokémon updated in drafts. Save with backup when ready.": [
    "Pokémon atualizados nos rascunhos. Salve com backup quando estiver pronto.",
    "Pokémon actualizados en los borradores. Guarda con copia cuando esté listo.",
  ],
  "Apply or cancel the team preview before saving.": [
    "Aplique ou cancele a prévia da equipe antes de salvar.",
    "Aplica o cancela la vista previa del equipo antes de guardar.",
  ],
  "Alpha disabled. Other adjustments remain in the draft; validate the encounter before saving.":
    [
      "Alpha desativado. Os outros ajustes permanecem no rascunho; valide o encontro antes de salvar.",
      "Alfa desactivado. Los otros ajustes permanecen en el borrador; valida el encuentro antes de guardar.",
    ],

  "This species/form has no Alpha encounter. Choose another species/form or use Custom Alpha, which preserves origin and reports incompatibilities.":
    [
      "Esta espécie/forma não possui encontro Alpha. Escolha outra espécie/forma ou use Alpha personalizado, que preserva a origem e informa incompatibilidades.",
      "Esta especie/forma no tiene un encuentro Alfa. Elige otra especie/forma o usa Alfa personalizado, que conserva el origen e informa incompatibilidades.",
    ],
  "No compatible Alpha origin preserves this identity. Review the species/origin or use Custom Alpha to preserve the current encounter.":
    [
      "Nenhuma origem Alpha compatível preserva esta identidade. Reveja a espécie/origem ou use Alpha personalizado para manter o encontro atual.",
      "Ningún origen Alfa compatible conserva esta identidad. Revisa la especie/origen o usa Alfa personalizado para conservar el encuentro actual.",
    ],
  "Available Alpha encounters conflict with these choices. Review level, form, gender, nature and origin, or choose Custom Alpha.":
    [
      "Os encontros Alpha disponíveis conflitam com estas escolhas. Confira nível, forma, gênero, natureza e origem, ou escolha Alpha personalizado.",
      "Los encuentros Alfa disponibles no son compatibles con estas opciones. Revisa nivel, forma, género, naturaleza y origen, o elige Alfa personalizado.",
    ],
  "No Alpha plan could be built. Review the reported constraints before changing these values.":
    [
      "Não foi possível criar um plano Alpha. Confira as restrições informadas antes de mudar os valores.",
      "No se pudo crear un plan Alfa. Revisa las restricciones indicadas antes de cambiar los valores.",
    ],

  Custom: ["Personalizado", "Personalizado"],
  Yes: ["Sim", "Sí"],
  No: ["Não", "No"],

  "No custom plan could be built for these values. Review the warnings and editable fields.":
    [
      "Não foi possível criar um plano personalizado para estes valores. Confira os avisos e os campos editáveis.",
      "No se pudo crear un plan personalizado para estos valores. Revisa los avisos y los campos editables.",
    ],
  "No compatible Alpha origin preserves this Pokémon's identity and other constraints. Custom Alpha keeps its origin and reports incompatibility honestly.":
    [
      "Nenhuma origem Alpha compatível preserva a identidade deste Pokémon e as demais restrições. Alpha personalizado mantém a origem e informa a incompatibilidade.",
      "Ningún origen Alfa compatible conserva la identidad de este Pokémon y las demás restricciones. Alfa personalizado conserva el origen e informa de la incompatibilidad.",
    ],
  "No Alpha encounter exists for this species/form in the current database. Custom Alpha remains available with an incompatible legality report.":
    [
      "Não existe encontro Alpha para esta espécie/forma na base atual. Alpha personalizado continua disponível com relatório de legalidade incompatível.",
      "No existe un encuentro Alfa para esta especie/forma en la base actual. Alfa personalizado sigue disponible con un informe de legalidad incompatible.",
    ],
  "Available Alpha encounters conflict with the selected form, gender, nature or origin fields. Adjust those choices or use explicit Custom mode.":
    [
      "Os encontros Alpha disponíveis conflitam com a forma, gênero, natureza ou origem escolhidos. Ajuste essas escolhas ou use o modo Personalizado explicitamente.",
      "Los encuentros Alfa disponibles no son compatibles con la forma, género, naturaleza u origen elegidos. Ajusta esas opciones o usa explícitamente el modo Personalizado.",
    ],
  "Custom Alpha changes the flag and required size/IV dependencies while preserving the requested origin. The PKHeX report remains authoritative; incompatible encounter/RNG results are not hidden.":
    [
      "Alpha personalizado altera a marca e os requisitos de tamanho/IVs, preservando a origem solicitada. O relatório do PKHeX é mantido; incompatibilidades de encontro/RNG não são ocultadas.",
      "Alfa personalizado cambia la marca y los requisitos de tamaño/IVs conservando el origen solicitado. Se mantiene el informe PKHeX; no se ocultan incompatibilidades de encuentro/RNG.",
    ],
  "This plan does not make the entire Pokémon legal. Review the unchanged PKHeX legality report, including any RNG correlation errors.":
    [
      "Este plano não torna o Pokémon totalmente legal. Confira o relatório original de legalidade do PKHeX, incluindo erros de correlação de RNG.",
      "Este plan no hace que el Pokémon sea totalmente legal. Revisa el informe original de legalidad de PKHeX, incluidos los errores de correlación de RNG.",
    ],

  "The write did not complete. Reopen the save and inspect the backup; drafts have been kept.":
    [
      "A gravação não foi concluída. Reabra o save e confira o backup; os rascunhos foram mantidos.",
      "La escritura no se completó. Abre el guardado y comprueba la copia; los borradores se conservaron.",
    ],

  "Alpha mode": ["Modo Alpha", "Modo Alfa"],
  "Compatible encounter": ["Encontro compatível", "Encuentro compatible"],
  "Custom: keep encounter": [
    "Personalizado: manter origem",
    "Personalizado: conservar origen",
  ],
  "Compatible previews encounter adjustments; Custom keeps origin and reports incompatibilities. Nothing is saved automatically.":
    [
      "Compatível mostra ajustes do encontro; Personalizado mantém a origem e informa incompatibilidades. Nada é salvo automaticamente.",
      "Compatible muestra ajustes del encuentro; Personalizado conserva el origen e informa incompatibilidades. Nada se guarda automáticamente.",
    ],
  "Alpha conversion preview": [
    "Prévia de conversão Alpha",
    "Vista previa de conversión Alfa",
  ],
  "Custom Alpha preserves the encounter. Compatibility findings remain visible; this does not certify legality.":
    [
      "Alpha personalizado preserva o encontro. As incompatibilidades continuam visíveis; isso não certifica legalidade.",
      "Alfa personalizado conserva el encuentro. Las incompatibilidades siguen visibles; esto no certifica la legalidad.",
    ],
  "Compatible Alpha may adapt encounter details. Review the changes before applying them to the draft.":
    [
      "Alpha compatível pode adaptar dados do encontro. Confira as mudanças antes de aplicá-las ao rascunho.",
      "Alfa compatible puede adaptar datos del encuentro. Revisa los cambios antes de aplicarlos al borrador.",
    ],
  "Minimum level": ["Nível mínimo", "Nivel mínimo"],
  "Encounter details will change only after you apply this plan.": [
    "Os dados do encontro só mudarão após aplicar este plano.",
    "Los datos del encuentro solo cambiarán al aplicar este plan.",
  ],
  Field: ["Campo", "Campo"],
  Before: ["Antes", "Antes"],
  After: ["Depois", "Después"],
  "No legality findings": [
    "Sem pendências de legalidade",
    "Sin problemas de legalidad",
  ],
  "Legality findings": ["Pendências de legalidade", "Problemas de legalidad"],
  "No compatible plan is available for these values. Adjust the level or choose Custom Alpha to keep the current encounter.":
    [
      "Não há plano compatível para estes valores. Ajuste o nível ou escolha Alpha personalizado para manter o encontro atual.",
      "No hay un plan compatible para estos valores. Ajusta el nivel o elige Alfa personalizado para conservar el encuentro actual.",
    ],
  "Cancel plan": ["Cancelar plano", "Cancelar plan"],
  "Apply custom Alpha to draft": [
    "Aplicar Alpha personalizado ao rascunho",
    "Aplicar Alfa personalizado al borrador",
  ],
  "Apply compatible Alpha to draft": [
    "Aplicar Alpha compatível ao rascunho",
    "Aplicar Alfa compatible al borrador",
  ],
  "This preview does not write the save. Save with backup remains a separate action.":
    [
      "Esta prévia não grava o save. Salvar com backup continua sendo uma ação separada.",
      "Esta vista previa no escribe el guardado. Guardar con copia sigue siendo una acción separada.",
    ],
  "Review the Alpha plan. No draft or save was changed.": [
    "Confira o plano Alpha. Nenhum rascunho ou save foi alterado.",
    "Revisa el plan Alfa. No se modificó ningún borrador ni guardado.",
  ],
  "Custom Alpha applied to draft; review the actual legality report before saving.":
    [
      "Alpha personalizado aplicado ao rascunho; confira o relatório real de legalidade antes de salvar.",
      "Alfa personalizado aplicado al borrador; revisa el informe real de legalidad antes de guardar.",
    ],
  "Compatible Alpha plan applied to draft. Save with backup when ready.": [
    "Plano Alpha compatível aplicado ao rascunho. Salve com backup quando estiver pronto.",
    "Plan Alfa compatible aplicado al borrador. Guarda con copia cuando esté listo.",
  ],
  "Alpha adjustments reverted; your other edits were kept.": [
    "Ajustes Alpha revertidos; suas outras edições foram mantidas.",
    "Ajustes Alfa revertidos; se conservaron tus otras ediciones.",
  ],
  "Alpha plan cancelled. The draft was not changed.": [
    "Plano Alpha cancelado. O rascunho não foi alterado.",
    "Plan Alfa cancelado. El borrador no se modificó.",
  ],
  "Alpha disabled. Other adjustments remain; use Revert Alpha adjustments to restore the planned fields.":
    [
      "Alpha desativado. Os outros ajustes permanecem; use Reverter ajustes Alpha para restaurar os campos do plano.",
      "Alfa desactivado. Los otros ajustes se mantienen; usa Revertir ajustes Alfa para restaurar los campos del plan.",
    ],
  "Revert Alpha adjustments": [
    "Reverter ajustes Alpha",
    "Revertir ajustes Alfa",
  ],
  "Apply or cancel the Alpha plan before saving.": [
    "Aplique ou cancele o plano Alpha antes de salvar.",
    "Aplica o cancela el plan Alfa antes de guardar.",
  ],
  Preview: ["Prévia", "Vista previa"],
  "The previous neutral effective nature is unavailable as a Mint here; effective nature now follows the original.":
    [
      "A natureza efetiva neutra anterior não está disponível como Mint neste caso; a efetiva agora segue a original.",
      "La naturaleza efectiva neutra anterior no está disponible como menta en este caso; la efectiva ahora sigue a la original.",
    ],

  "Alpha needs at least three perfect IVs. Keep three IVs at 31 before reducing another.":
    [
      "Alpha exige ao menos três IVs perfeitos. Mantenha três IVs em 31 antes de reduzir outro.",
      "Alfa requiere al menos tres IVs perfectos. Mantén tres IVs en 31 antes de reducir otro.",
    ],

  "Choose Pokémon": ["Escolher Pokémon", "Elegir Pokémon"],
  "Search by name or number": [
    "Buscar por nome ou número",
    "Buscar por nombre o número",
  ],
  results: ["resultados", "resultados"],
  "available species": ["espécies disponíveis", "especies disponibles"],
  "Available Pokémon": ["Pokémon disponíveis", "Pokémon disponibles"],
  "Back to editor": ["Voltar ao editor", "Volver al editor"],

  "Neutral: no stat changes (0%)": [
    "Neutra: sem mudanças (0%)",
    "Neutra: sin cambios (0%)",
  ],
  "Alpha Pokémon": ["Pokémon Alpha", "Pokémon Alfa"],
  "Automatically sets maximum size and at least 3 perfect IVs. Encounter compatibility is checked without saving.":
    [
      "Ajusta automaticamente tamanho máximo e ao menos 3 IVs perfeitos. Confere o encontro sem salvar.",
      "Ajusta automáticamente el tamaño máximo y al menos 3 IVs perfectos. Comprueba el encuentro sin guardar.",
    ],
  "Alpha disabled. Size and IVs are kept in the draft.": [
    "Alpha desativado. Tamanho e IVs são mantidos no rascunho.",
    "Alfa desactivado. El tamaño y los IVs se conservan en el borrador.",
  ],
  "Alpha prepared in the draft. No save was changed.": [
    "Alpha preparado no rascunho. Nenhum save foi alterado.",
    "Alfa preparado en el borrador. No se modificó el guardado.",
  ],
  "Alpha prepared in the draft; the report still has legality findings. No save was changed.":
    [
      "Alpha preparado no rascunho; o relatório ainda aponta pendências de legalidade. Nenhum save foi alterado.",
      "Alfa preparado en el borrador; el informe aún señala problemas de legalidad. No se modificó el guardado.",
    ],
  "This encounter cannot become Alpha. To create a valid Alpha, use Add Pokémon. The draft is kept; you can turn Alpha off.":
    [
      "Este encontro não permite Alpha. Para criar um Alpha válido, use Adicionar Pokémon. O rascunho foi mantido; você pode desativar Alpha.",
      "Este encuentro no permite Alfa. Para crear un Alfa válido, usa Añadir Pokémon. Se conserva el borrador; puedes desactivar Alfa.",
    ],

  "Nature suggestions added to drafts.": [
    "sugestões de natureza adicionadas aos rascunhos.",
    "sugerencias de naturaleza añadidas a los borradores.",
  ],
  "View nature suggestions": [
    "Ver sugestões de natureza",
    "Ver sugerencias de naturaleza",
  ],

  "Saved with backup, but refresh failed. Reopen the save before editing.": [
    "A gravação e o backup foram confirmados, mas a atualização falhou. Reabra o save antes de editar.",
    "La escritura y la copia se confirmaron, pero falló la actualización. Abre el guardado antes de editar.",
  ],
  Move: ["Golpe", "Movimiento"],
  "Add move": ["Adicionar golpe", "Añadir movimiento"],
  "Stored value": ["Valor armazenado", "Valor almacenado"],
  Default: ["Padrão", "Predeterminada"],
  "Trainer ID (16-bit)": [
    "ID do treinador (16 bits)",
    "ID del entrenador (16 bits)",
  ],
  "Secret ID (16-bit)": ["ID secreto (16 bits)", "ID secreto (16 bits)"],
  "Displayed trainer IDs": [
    "IDs exibidos do treinador",
    "IDs mostrados del entrenador",
  ],
  "Mint changes effective nature only. Species and form changes remain subject to encounter validation.":
    [
      "Mint altera apenas a natureza efetiva. Espécie e forma continuam sujeitas à validação do encontro.",
      "La menta solo cambia la naturaleza efectiva. La especie y forma siguen sujetas a validación del encuentro.",
    ],
  "Registered + moves do not bypass learnability. PP and relearn data are preserved.":
    [
      "Golpes + registrados não ignoram regras de aprendizagem. PP e dados de reaprendizagem são preservados.",
      "Los movimientos + registrados no evitan las reglas de aprendizaje. Se conservan los PP y datos recordables.",
    ],
  "Trainer and encounter changes affect identity and legality. A backup does not guarantee online compatibility.":
    [
      "Mudanças de treinador e encontro afetam identidade e legalidade. O backup não garante compatibilidade online.",
      "Los cambios de entrenador y encuentro afectan a la identidad y legalidad. La copia no garantiza compatibilidad en línea.",
    ],
  "Native Z-A keeps stored height and weight at zero; HOME records keep both equal to Scale.":
    [
      "Z-A nativo mantém altura e peso armazenados em zero; registros HOME mantêm ambos iguais à Escala.",
      "Z-A nativo mantiene altura y peso almacenados en cero; los registros HOME mantienen ambos iguales a Escala.",
    ],
  "Unchecked fields use generated values. Edit a value or enable customization to override it.":
    [
      "Campos desmarcados usam valores gerados. Edite um valor ou ative a personalização para substituí-lo.",
      "Los campos sin marcar usan valores generados. Edita un valor o activa la personalización para cambiarlo.",
    ],
  "Local API reference": [
    "Referência da API local",
    "Referencia de la API local",
  ],
  "Interactive API documentation is available at /api-docs on this local host. The runtime token stays private.":
    [
      "A documentação interativa da API está em /api-docs neste host local. O token da sessão permanece privado.",
      "La documentación interactiva de la API está en /api-docs de este host local. El token de sesión permanece privado.",
    ],
  "Automatic values may differ when the Pokémon is generated.": [
    "Valores automáticos podem variar ao gerar o Pokémon.",
    "Los valores automáticos pueden variar al generar el Pokémon.",
  ],

  "Copy failed. Select and copy the backup path manually.": [
    "Falha ao copiar. Selecione e copie o caminho do backup manualmente.",
    "No se pudo copiar. Selecciona y copia la ruta manualmente.",
  ],
  "Stopping…": ["Encerrando…", "Cerrando…"],
  "Local API": ["API local", "API local"],
  "Stop editor": ["Encerrar editor", "Cerrar editor"],
  "Editor stopped. Close this tab or launch the portable EXE again.": [
    "Editor encerrado. Feche esta aba ou abra o EXE portátil novamente.",
    "Editor cerrado. Cierra esta pestaña o abre de nuevo el EXE portátil.",
  ],
  "Reloading this page discards unsaved drafts. Save or discard them before leaving.":
    [
      "Recarregar esta página descarta rascunhos não salvos. Salve ou descarte antes de sair.",
      "Recargar esta página descarta los borradores no guardados. Guarda o descarta antes de salir.",
    ],

  "The write outcome is unknown. Reopen the save to reconcile; drafts have been kept.":
    [
      "O resultado da gravação é incerto. Reabra o save para conferir; os rascunhos foram mantidos.",
      "El resultado de la escritura es incierto. Abre el guardado para comprobarlo; los borradores se han conservado.",
    ],
  Save: ["Save", "Guardado"],
  "Choose a save": ["Escolher save", "Elegir guardado"],
  "Find saves": ["Localizar saves", "Buscar guardados"],
  Open: ["Abrir", "Abrir"],
  "Save path": ["Caminho do save", "Ruta del guardado"],
  Trainer: ["Treinador", "Entrenador"],
  "Automatic backup on every write": [
    "Backup automático a cada gravação",
    "Copia automática en cada escritura",
  ],
  "Add Pokémon": ["Adicionar Pokémon", "Añadir Pokémon"],
  Team: ["Equipe", "Equipo"],
  Box: ["Caixa", "Caja"],
  Boxes: ["Caixas", "Cajas"],
  "Search Pokémon": ["Buscar Pokémon", "Buscar Pokémon"],
  "Optimize team": ["Otimizar equipe", "Optimizar equipo"],
  "Suggest natures": ["Sugerir naturezas", "Sugerir naturalezas"],
  Level: ["Nível", "Nivel"],
  Nature: ["Natureza", "Naturaleza"],
  Mint: ["Mint", "Menta"],
  Ball: ["Pokébola", "Poké Ball"],
  Nickname: ["Apelido", "Apodo"],
  Favorite: ["Favorito", "Favorito"],
  Profile: ["Perfil", "Perfil"],
  Attributes: ["Atributos", "Atributos"],
  Moves: ["Golpes", "Movimientos"],
  Origin: ["Origem", "Origen"],
  Advanced: ["Avançado", "Avanzado"],
  Stat: ["Atributo", "Atributo"],
  Attack: ["Ataque", "Ataque"],
  Defense: ["Defesa", "Defensa"],
  Speed: ["Velocidade", "Velocidad"],
  "Sp. Atk": ["Atq. Esp.", "At. Esp."],
  "Sp. Def": ["Def. Esp.", "Def. Esp."],
  Scale: ["Escala", "Escala"],
  "Alpha requires a compatible encounter.": [
    "Alpha exige um encontro compatível.",
    "Alfa requiere un encuentro compatible.",
  ],
  "Technical data and report": [
    "Dados técnicos e relatório",
    "Datos técnicos e informe",
  ],
  "Discard drafts": ["Descartar rascunhos", "Descartar borradores"],
  "Save with backup": ["Salvar com backup", "Guardar con copia"],
  "No pending changes": [
    "Nenhuma alteração pendente",
    "Sin cambios pendientes",
  ],
  "Pokémon with changes": ["Pokémon com alterações", "Pokémon con cambios"],
  "Loading…": ["Carregando…", "Cargando…"],
  "Saving…": ["Salvando…", "Guardando…"],
  "Saved with backup.": ["Salvo com backup.", "Guardado con copia."],
  "Changes remain drafts until saved.": [
    "Alterações ficam em rascunho até salvar.",
    "Los cambios son borradores hasta guardar.",
  ],
  "Close the game before saving.": [
    "Feche o jogo antes de salvar.",
    "Cierra el juego antes de guardar.",
  ],
  "New Pokémon": ["Novo Pokémon", "Nuevo Pokémon"],
  "Cancel creation": ["Cancelar criação", "Cancelar creación"],
  "Add with backup": ["Adicionar com backup", "Añadir con copia"],
  Destination: ["Destino", "Destino"],
  "Automatic: team, then boxes": [
    "Automático: equipe, depois caixas",
    "Automático: equipo, después cajas",
  ],
  "First available": ["Primeira disponível", "Primera disponible"],
  "Resolve pending drafts before adding.": [
    "Salve ou descarte os rascunhos antes de adicionar.",
    "Guarda o descarta los borradores antes de añadir.",
  ],
  "Select a Pokémon": ["Selecione um Pokémon", "Selecciona un Pokémon"],
  "Choose a save to begin.": [
    "Escolha um save para começar.",
    "Elige un guardado para empezar.",
  ],
  "No results": ["Nenhum resultado", "Sin resultados"],
  "Empty slot": ["Espaço vazio", "Espacio vacío"],
  "Show 30 slots": ["Mostrar 30 espaços", "Mostrar 30 espacios"],
  "Hide boxes": ["Recolher caixas", "Ocultar cajas"],
  "Keep editing": ["Continuar editando", "Seguir editando"],
  "Discard and continue": ["Descartar e continuar", "Descartar y continuar"],
  "Discard unsaved changes?": [
    "Descartar alterações não salvas?",
    "¿Descartar cambios sin guardar?",
  ],
  Validation: ["Validação", "Validación"],
  "Validate draft": ["Validar rascunho", "Validar borrador"],
  "Validated by engine.": ["Validado pelo motor.", "Validado por el motor."],
  Backup: ["Backup", "Copia"],
  Error: ["Erro", "Error"],
  "Try again": ["Tentar novamente", "Reintentar"],
  "Opening…": ["Abrindo…", "Abriendo…"],
  Language: ["Idioma", "Idioma"],
  "Open a save": ["Abrir um save", "Abrir un guardado"],
  "Local portable editor": ["Editor portátil local", "Editor portátil local"],
  "IV presets clear Hyper Training.": [
    "Presets de IVs limpam Hyper Training.",
    "Los ajustes de IVs borran Entrenamiento Extremo.",
  ],
  "Alpha sets Scale 255 and at least three IVs to 31.": [
    "Alpha ajusta Escala 255 e ao menos três IVs para 31.",
    "Alfa fija Escala 255 y al menos tres IVs a 31.",
  ],
  Size: ["Tamanho", "Tamaño"],
  Minimum: ["Mínimo", "Mínimo"],
  Small: ["Pequeno", "Pequeño"],
  Average: ["Médio", "Mediano"],
  Large: ["Grande", "Grande"],
  Maximum: ["Máximo", "Máximo"],
  "Original nature": ["Natureza original", "Naturaleza original"],
  "Effective nature": ["Natureza efetiva", "Naturaleza efectiva"],
  Form: ["Forma", "Forma"],
  Species: ["Espécie", "Especie"],
  Gender: ["Gênero", "Género"],
  Male: ["Masculino", "Masculino"],
  Female: ["Feminino", "Femenino"],
  Genderless: ["Sem gênero", "Sin género"],
  "Use nickname": ["Usar apelido", "Usar apodo"],
  "Held item": ["Item equipado", "Objeto equipado"],
  Friendship: ["Amizade", "Amistad"],
  Markings: ["Marcações", "Marcas"],
  Unmarked: ["Sem marca", "Sin marca"],
  Blue: ["Azul", "Azul"],
  Red: ["Vermelho", "Rojo"],
  Automatic: ["Automático", "Automático"],
  Customize: ["Personalizar", "Personalizar"],
  "Non-shiny": ["Normal", "Normal"],
  "Star shiny": ["Shiny estrela", "Variocolor estrella"],
  "Square shiny": ["Shiny quadrado", "Variocolor cuadrado"],
  "Shiny style": ["Estilo shiny", "Tipo variocolor"],
  "Original trainer": ["Treinador original", "Entrenador original"],
  "Trainer gender": ["Gênero do treinador", "Género del entrenador"],
  "Pokémon language": ["Idioma do Pokémon", "Idioma del Pokémon"],
  "Encounter level": ["Nível do encontro", "Nivel del encuentro"],
  "Encounter location": ["Local do encontro", "Lugar del encuentro"],
  "Encounter date": ["Data do encontro", "Fecha del encuentro"],
  "Obedience level": ["Nível de obediência", "Nivel de obediencia"],
  "Registered + moves": ["Golpes + registrados", "Movimientos + registrados"],
  Remove: ["Remover", "Quitar"],
  None: ["Nenhum", "Ninguno"],
  "Stored PP": ["PP armazenados", "PP almacenados"],
  "Stored PP Ups": ["PP Ups armazenados", "PP Ups almacenados"],
  "Relearn moves": ["Golpes de reaprendizagem", "Movimientos recordables"],
  "Height scalar": ["Escala de altura", "Escala de altura"],
  "Weight scalar": ["Escala de peso", "Escala de peso"],
  "Affixed title": ["Título afixado", "Título asignado"],
  "Read-only information": [
    "Informações somente leitura",
    "Información de solo lectura",
  ],
  "HOME tracker": ["Identificador HOME", "Identificador HOME"],
  "Origin game": ["Jogo de origem", "Juego de origen"],
  "Ability / slot": ["Habilidade / slot", "Habilidad / posición"],
  Handler: ["Treinador atual", "Entrenador actual"],
  "Owned ribbons": ["Ribbons possuídos", "Cintas obtenidas"],
  "Egg data": ["Dados de ovo", "Datos de huevo"],
  "Preserved fields": ["Campos preservados", "Campos conservados"],
  "Original PKHeX report": [
    "Relatório PKHeX original",
    "Informe PKHeX original",
  ],
  "No save was changed.": [
    "Nenhum save foi alterado.",
    "No se ha modificado el guardado.",
  ],
  "The save changed externally. Reopen it before writing.": [
    "O save mudou externamente. Reabra antes de gravar.",
    "El guardado cambió externamente. Ábrelo de nuevo antes de escribir.",
  ],
  "Reopen save": ["Reabrir save", "Reabrir guardado"],
  "Open the portable EXE to start a local session.": [
    "Abra o EXE portátil para iniciar uma sessão local.",
    "Abre el EXE portátil para iniciar una sesión local.",
  ],
  "Personal data stays on this computer.": [
    "Os dados ficam neste computador.",
    "Los datos permanecen en este equipo.",
  ],
  "Backup folder": ["Pasta do backup", "Carpeta de la copia"],
  "Copy path": ["Copiar caminho", "Copiar ruta"],
  Copied: ["Copiado", "Copiado"],
  "Suggestion only; no universally best nature.": [
    "Sugestão heurística; não há natureza universalmente melhor.",
    "Sugerencia heurística; no hay una naturaleza universalmente mejor.",
  ],
  "Hyper Training requires level 50.": [
    "Hyper Training exige nível 50.",
    "El Entrenamiento Extremo requiere nivel 50.",
  ],
  "Invalid numeric value": [
    "Valor numérico inválido",
    "Valor numérico no válido",
  ],
  "Choose a valid catalog value.": [
    "Escolha um valor válido da lista.",
    "Elige un valor válido de la lista.",
  ],
};
export const translate = (locale: Locale, key: string) =>
  locale === "en" ? key : (words[key]?.[locale === "es" ? 1 : 0] ?? key);
