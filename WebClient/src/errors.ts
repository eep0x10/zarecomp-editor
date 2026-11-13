import { translate, type Locale } from "./i18n";
// Existing native editor message translations, retained for domain parity.
const nativeMessages: Record<string, [string, string]> = {
  "EVs não podem somar mais de 510.": [
    "EVs cannot total more than 510.",
    "Los EVs no pueden sumar más de 510.",
  ],
  "Posição inválida ou bloqueada.": [
    "Invalid or locked slot.",
    "Posición inválida o bloqueada.",
  ],
  "A posição selecionada está vazia.": [
    "The selected slot is empty.",
    "La posición seleccionada está vacía.",
  ],
  "Pokémon tem checksum inválido.": [
    "The Pokémon checksum is invalid.",
    "El checksum del Pokémon es inválido.",
  ],

  "Shiny: estilo inválido.": [
    "Shiny: invalid style.",
    "Variocolor: tipo inválido.",
  ],
  "Language: idioma não disponível em Z-A.": [
    "Language: unavailable in Z-A.",
    "Idioma: no disponible en Z-A.",
  ],
  "Ball: Pokébola não disponível para captura em Z-A.": [
    "Ball: unavailable for capture in Z-A.",
    "Poké Ball: no disponible para captura en Z-A.",
  ],
  "HeldItem: item não disponível em Z-A.": [
    "Held item: unavailable in Z-A.",
    "Objeto equipado: no disponible en Z-A.",
  ],
  "OriginalTrainerName: nome obrigatório.": [
    "Original trainer name is required.",
    "El nombre del entrenador original es obligatorio.",
  ],
  "MetDate: use uma data entre 2000 e 2099.": [
    "Encounter date: use a date between 2000 and 2099.",
    "Fecha del encuentro: usa una fecha entre 2000 y 2099.",
  ],
  "MetLocation: escolha um local de origem nativa de Z-A.": [
    "Encounter location: select a native Z-A location.",
    "Lugar del encuentro: elige un lugar nativo de Z-A.",
  ],
  "HyperTrained: seis flags são necessárias.": [
    "Hyper Training: six flags are required.",
    "Entrenamiento Extremo: se necesitan seis marcas.",
  ],
  "Species/Form: esta forma persistente não está disponível em Z-A.": [
    "Species/form: this persistent form is unavailable in Z-A.",
    "Especie/forma: esta forma persistente no está disponible en Z-A.",
  ],
  "Gender: incompatível com a espécie/forma escolhida.": [
    "Gender is incompatible with the selected species/form.",
    "El género es incompatible con la especie/forma elegida.",
  ],
  "StatNature: use uma natureza com mint disponível, ou a natureza original.": [
    "Effective nature: select an available mint nature or the original nature.",
    "Naturaleza efectiva: elige una naturaleza con menta disponible o la naturaleza original.",
  ],
  "Nickname: apelido não pode ser vazio quando IsNicknamed é verdadeiro.": [
    "Nickname cannot be empty when Use a nickname is enabled.",
    "El apodo no puede estar vacío si Usar apodo está activado.",
  ],
  "HyperTrained: nível mínimo 50.": [
    "Hyper Training requires level 50.",
    "El Entrenamiento Extremo requiere nivel 50.",
  ],
  "HyperTrained: um IV 31 não pode receber Hyper Training.": [
    "An IV of 31 cannot receive Hyper Training.",
    "Un IV de 31 no puede recibir Entrenamiento Extremo.",
  ],
  "Moves: ao menos um golpe, sem repetidos.": [
    "Moves: select at least one move, without duplicates.",
    "Movimientos: elige al menos uno y sin repetidos.",
  ],
  "AffixedRibbon: selecione uma fita já existente, ou -1 para nenhuma.": [
    "Affixed title: select an already-owned ribbon, or none.",
    "Título asignado: elige una cinta ya obtenida o ninguna.",
  ],
  "PlusMoves: registro não permitido para esta espécie/forma.": [
    "Registered + move is not allowed for this species/form.",
    "El movimiento + registrado no está permitido para esta especie/forma.",
  ],
  "Scale: registro HOME exige HeightScalar e WeightScalar iguais a Scale.": [
    "HOME records require height and weight scalars to equal Scale.",
    "Los registros HOME requieren que las escalas de altura y peso sean iguales a Scale.",
  ],
  "Scale: origem nativa usa Scale; HeightScalar e WeightScalar devem ser zero.":
    [
      "Native origin uses Scale; height and weight scalars must be zero.",
      "La procedencia nativa usa Scale; las escalas de altura y peso deben ser cero.",
    ],
  "IsAlpha: Alpha exige Scale 255 e ao menos três IVs 31.": [
    "Alpha requires Scale 255 and at least three IVs of 31.",
    "Alfa requiere Scale 255 y al menos tres IVs de 31.",
  ],
  "IsAlpha: os dados de encontro não correspondem a um encontro Alpha válido.":
    [
      "The encounter data does not match a valid Alpha encounter.",
      "Los datos no corresponden a un encuentro Alfa válido.",
    ],
  "IsAlpha: não existe encontro Alpha disponível para esta espécie/forma neste nível. Aumente o nível ou escolha outra espécie/forma.":
    [
      "No Alpha encounter is available for this species/form at this level. Increase the level or choose another species/form.",
      "No hay un encuentro Alfa disponible para esta especie/forma a este nivel. Sube el nivel o elige otra especie/forma.",
    ],
  "ObedienceLevel: incompatível com nível de encontro, nível atual ou treinador.":
    [
      "Obedience level is incompatible with encounter level, current level or trainer.",
      "El nivel de obediencia es incompatible con el nivel de encuentro, nivel actual o entrenador.",
    ],
  "Moves: golpe indisponível para a espécie/forma e nível escolhidos em Z-A.": [
    "Move unavailable for the selected species/form and level in Z-A.",
    "Movimiento no disponible para la especie/forma y nivel elegidos en Z-A.",
  ],
  "PKHeX sinaliza legalidade; este editor não garante compatibilidade online.":
    [
      "PKHeX flagged legality concerns; this editor does not guarantee online compatibility.",
      "PKHeX detecta problemas de legalidad; el editor no garantiza compatibilidad en línea.",
    ],
  "Nível abaixo do nível original de encontro.": [
    "Level is below the original encounter level.",
    "El nivel es inferior al nivel original del encuentro.",
  ],
  ": dados somente para leitura em Z-A; PP é calculado ao selecionar golpes.": [
    ": read-only data in Z-A; PP is calculated when moves are selected.",
    ": datos de solo lectura en Z-A; los PP se calculan al elegir movimientos.",
  ],
  ": texto Unicode inválido.": [
    ": invalid Unicode text.",
    ": texto Unicode inválido.",
  ],
  " caracteres válidos, sem controles.": [
    " valid characters, without control characters.",
    " caracteres válidos, sin caracteres de control.",
  ],
  ": até ": [": up to ", ": hasta "],
  ": use ": [": use ", ": usa "],
  ": informe ": [": provide ", ": indica "],
  " valores entre ": [" values between ", " valores entre "],
};
export function localizeError(locale: Locale, message: string): string {
  const range = message.match(
    /^([A-Za-z][A-Za-z0-9\/]*): use (-?\d+)\.\.(-?\d+)\.$/,
  );
  if (range) {
    const labels: Record<string, string> = {
      Species: "Species",
      Form: "Form",
      Level: "Level",
      Gender: "Gender",
      Nature: "Nature",
      StatNature: "Effective nature",
      Language: "Pokémon language",
      Friendship: "Friendship",
      MetLevel: "Encounter level",
      MetLocation: "Encounter location",
      ObedienceLevel: "Obedience level",
      OriginalTrainerGender: "Trainer gender",
      TID16: "Trainer ID (16-bit)",
      SID16: "Secret ID (16-bit)",
      HeightScalar: "Height scalar",
      WeightScalar: "Weight scalar",
      Scale: "Scale",
      HeldItem: "Held item",
      Ball: "Ball",
    };
    const label = translate(locale, labels[range[1]] || range[1]);
    return locale === "pt-BR"
      ? `${label}: use um valor entre ${range[2]} e ${range[3]}.`
      : locale === "es"
        ? `${label}: usa un valor entre ${range[2]} y ${range[3]}.`
        : `${label}: use a value between ${range[2]} and ${range[3]}.`;
  }
  if (locale === "pt-BR") return translate(locale, message);
  let result = message;
  for (const key of Object.keys(nativeMessages).sort(
    (a, b) => b.length - a.length,
  ))
    result = result
      .split(key)
      .join(nativeMessages[key][locale === "es" ? 1 : 0]);
  return translate(locale, result);
}
