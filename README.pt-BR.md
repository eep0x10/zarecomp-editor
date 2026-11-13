# ZA Recomp Editor — Português (Brasil)

**Editor portátil de saves de Pokémon Legends: Z-A para o ZA Recomp — versão 2.0.3.** Equipe, boxes, criação e edição detalhada em uma única bancada web, com tema escuro, acrílico e backup automático.

[Baixar para Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [English](README.md) · [Español](README.es.md) · [Guia detalhado](docs/EDITING.md)

Windows x64. Extraia o ZIP em uma pasta gravável e abra **RecompPokemon.exe**. O EXE abre o navegador em um endereço local temporário `127.0.0.1`; você não precisa instalar Node.js, .NET ou um servidor. Não inclui jogo nem saves pessoais.

![Versão 2.0.3: equipe, box e edição inline](docs/screenshots/web/current-desktop.jpg)

As capturas são reais do build 2.0.3 com save sintético de demonstração. Capturas antigas e propostas de design são registros históricos, não a apresentação da versão atual.

<details>
<summary>Veja como adicionar com detalhes</summary>

![Versão 2.0.3: criação personalizada na mesma bancada](docs/screenshots/web/current-add.jpg)

<img src="docs/screenshots/web/current-narrow.jpg" alt="Versão 2.0.3 em tela estreita" width="390">

</details>

## Abrir seu save

Feche o ZA Recomp. No seletor de save, use a busca ou informe o caminho completo do arquivo **main**. Locais comuns: `<pasta do ZA Recomp>/save/save_0/main` ou `<pasta do ZA Recomp>/PORT/save/save_0/main`. Confira treinador e data se houver várias cópias. A seleção e os avisos ficam na própria página, sem abrir diálogos.

## Editar equipe e boxes

A equipe fica à esquerda, a box com 30 posições no centro e o editor à direita. Selecione um Pokémon e altere os campos; trocar a seleção mantém os rascunhos. As seções **Perfil, Atributos, Golpes, Origem e Avançado** ficam no mesmo painel.

- **Perfil:** espécie/forma persistente, apelido, gênero, nível, natureza original/Mint, estilo shiny, Pokébola, item, favorito, amizade e marcações.
- **Escolhas visuais:** as 25 Pokébolas têm PNGs locais disponíveis offline; natureza original e Mint mostram os efeitos de +10% / −10% nos atributos ou indicam efeito neutro.
- **Atributos:** IVs, EVs e Hyper Training. IVs 0–31; EVs até 252 por atributo e 510 no total.
- **Golpes:** quatro posições e registros Plus; PP e histórico são somente leitura.
- **Origem:** treinador, IDs brutos, idioma, local/nível/data de encontro e obediência.
- **Avançado:** Alpha, tamanho, título já possuído e dados técnicos/relatório para consulta.

**Salvar com backup** valida e grava os rascunhos da equipe e das boxes em uma operação. **Descartar rascunhos** abandona as alterações pendentes, com confirmação inline. Salve antes de atualizar ou fechar a aba: rascunhos não são arquivos gravados automaticamente.

Um clique em **Alpha** pré-valida a compatibilidade, ajusta Escala para 255 e completa pelo menos três IVs 31, preservando os demais IVs. O encontro original precisa ser compatível; o editor não substitui seu histórico. Presets de tamanho sincronizam os escalares nativos/HOME; os botões de IVs 31/0 removem Hyper Training.

![Versão 2.0.3: controles detalhados, Pokébolas e naturezas — save sintético](docs/screenshots/web/current-details.jpg)

## Adicionar Pokémon personalizado

**+ Adicionar Pokémon** abre diretamente o seletor com as **364 espécies** do catálogo atual do motor, em ordem da numeração da Pokédex Nacional. As imagens carregam conforme os cartões se aproximam da área visível. Busque por nome ou ID numérico exato (`25` ou `#25`); escolha por toque/clique ou use setas, Home/End e Enter. Escape retorna ao editor. Uma imagem indisponível não remove a espécie da lista.

![Versão 2.0.3: catálogo completo e busca por nome ou ID](docs/screenshots/web/current-species.jpg)

A escolha abre a primeira forma suportada no mesmo painel. **Escolher Pokémon** reabre a seleção. Trocar a espécie preserva os demais detalhes da criação e redefine golpes e registros Plus para **Automático**, adequados à nova espécie. Escolha nível, natureza, shiny e Pokébola; ative os campos opcionais desejados. **Automático** mantém a geração do motor, não significa zero ou IV perfeito.

Destino **Automático** usa primeiro uma vaga na equipe e depois uma box desbloqueada. Equipe exige vaga; Boxes usa uma posição livre sem substituir outro Pokémon. Resolva os rascunhos de edição antes de adicionar. **Adicionar com backup grava imediatamente**, em uma operação separada do lote de rascunhos. Cancelar a criação não grava nada.

A criação Alpha procura um encontro compatível com espécie/forma/nível. Charmander Alpha exige nível 25 ou maior; Mewtwo não tem template Alpha nos dados incorporados. Combinações indisponíveis são recusadas antes da gravação.

## Optimize e idiomas

Optimize sugere **somente naturezas**, com motivo e revisão em rascunho. Não muda IVs, EVs, golpes ou nível. Não há uma natureza competitiva universalmente melhor; função da equipe, Mega Evolução e mecânicas de Z-A influenciam a escolha.

Interface em **PT-BR, inglês e espanhol**. Nomes de golpes/natures e relatórios PKHeX permanecem em inglês. O acrílico tem alternativa opaca; a interface respeita movimento reduzido. Em telas estreitas, a equipe vira faixa e as boxes se expandem dentro da mesma página.

## Backups e privacidade

Cada gravação cria uma cópia integral da pasta do save em `ZA-Tool-Backups/<data>-<id>/`, com manifesto SHA-256, validação, conferência de hash e substituição atômica. O recibo mostra a pasta do backup. Adições usam IDs de operação para evitar duplicação por reenvio. Save alterado externamente ou resultado de gravação incerto exige conferência, sem retentativa automática.

Para restaurar: feche jogo e editor, preserve o arquivo atual separadamente e copie o **main** do backup desejado para o lugar do editado. Feche sempre o jogo antes de salvar; a detecção não reconhece todo executável renomeado. Validação de campos não garante aceitação no jogo ou online.

O servidor escuta apenas no próprio computador e usa uma credencial temporária por execução. Saves, treinador e apelidos não são enviados. Preferências/cache ficam junto ao EXE. Imagens Pokémon opcionais usam PokéAPI/GitHub com IDs públicos; esses hosts recebem os dados normais da conexão HTTP. A resolução preserva forma visível e shiny exatos, tentando artwork, HOME e PNG padrão sem trocar por outra forma ou pela cor normal. O cache de imagens usa `cache/pokemon/v2`; quando uma busca se recupera, todas as cópias visíveis da mesma imagem são atualizadas. A indisponibilidade é explícita, com retentativas limitadas. Edição e imagens das Pokébolas funcionam offline; imagens Pokémon ainda não armazenadas podem faltar. Não há garantia de disponibilidade de todas as imagens. Não compartilhe URLs de sessão ou diagnósticos privados de execução.

A referência interativa da API local fica em `/api-docs`, e o esquema em `/openapi/v1.json`. São ferramentas de desenvolvimento; as gravações seguem as mesmas regras do editor.

## Código e licença

Para compilar: Windows, SDK .NET 10 x64 e Node.js 24/npm; execute `./build.ps1`. O script compila a interface, incorpora os assets e gera `dist/RecompPokemon.exe`. Esses requisitos são apenas para desenvolvimento. Consulte os [comandos de testes](README.md#build-from-source), usando cópias descartáveis de saves.

O harness atual tem **40 verificações HTTP com save gerado** e **32 verificações determinísticas de sprites**, executadas pelo CI. O modo opcional `--sprites-live` também consulta imagens externas: o registro histórico contém 24 consultas aprovadas, sujeitas à disponibilidade da fonte. A suíte Engine passou anteriormente em 67 verificações; isso não indica nova execução em toda versão. Veja o [guia do harness](HostTests/README.md).

Código GPL-3.0-or-later e PKHeX.Core com fontes correspondentes. Consulte [avisos de terceiros](THIRD_PARTY_NOTICES.md) e [licenças do frontend](WEB-THIRD-PARTY-NOTICES.txt). Assets e marcas Pokémon pertencem aos titulares. Projeto independente, sem afiliação à Nintendo, Game Freak, The Pokémon Company, ZA Recomp ou PKHeX.
