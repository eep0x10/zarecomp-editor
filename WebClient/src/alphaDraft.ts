import type { Patch, Catalogs } from "./api";
export function alphaChangeValue(
  field: string,
  raw: string,
  catalogs: Catalogs,
  t: (s: string) => string,
): string {
  let value: unknown = raw;
  try {
    value = JSON.parse(raw);
  } catch {
    /* Human-readable backend labels stay intact. */
  }
  if (value == null || value === "") return "—";
  const key = field.toLowerCase();
  if (key === "alphamode") {
    if (value === 0 || value === "Compatible") return t("Compatible encounter");
    if (value === 1 || value === "Custom") return t("Custom");
  }
  if (typeof value === "boolean") return t(value ? "Yes" : "No");
  if (Array.isArray(value)) {
    if (["moves", "plusmoves", "relearnmoves"].includes(key))
      return (
        value
          .map((id) =>
            Number(id) === 0
              ? t("None")
              : catalogs.moves?.find((c) => c.id === Number(id))?.name ||
                "#" + id,
          )
          .join(" / ") || "—"
      );
    return (
      value
        .map((item) =>
          typeof item === "boolean" ? t(item ? "Yes" : "No") : String(item),
        )
        .join(" / ") || "—"
    );
  }
  if (typeof value === "number") {
    if (key === "nature" || key === "statnature")
      return catalogs.natures?.find((c) => c.id === value)?.name || "#" + value;
    if (key === "ball")
      return catalogs.balls?.find((c) => c.id === value)?.name || "#" + value;
    if (key === "gender" || key === "originaltrainergender")
      return t(["Male", "Female", "Genderless"][value] || String(value));
    if (key === "shiny")
      return t(
        ["Non-shiny", "Star shiny", "Square shiny"][value] || String(value),
      );
  }
  return String(value);
}
export type AlphaMode = "Compatible" | "Custom";
export type AlphaPlan = {
  available: boolean;
  code: string;
  hash: string;
  mode: AlphaMode;
  minimumLevel: number | null;
  requiresOriginChange: boolean;
  patch: Patch | null;
  options: Record<string, any> | null;
  preview: import("./api").Details | null;
  changes: { field: string; before: string; after: string }[];
  warnings: string[];
};
export function alphaUnavailableMessage(
  plan: Pick<AlphaPlan, "mode" | "code">,
) {
  if (plan.mode === "Custom")
    return "No custom plan could be built for these values. Review the warnings and editable fields.";
  if (plan.code === "alpha_encounter_unavailable")
    return "This species/form has no Alpha encounter. Choose another species/form or use Custom Alpha, which preserves origin and reports incompatibilities.";
  if (plan.code === "alpha_origin_unavailable")
    return "No compatible Alpha origin preserves this identity. Review the species/origin or use Custom Alpha to preserve the current encounter.";
  if (plan.code === "alpha_constraints_conflict")
    return "Available Alpha encounters conflict with these choices. Review level, form, gender, nature and origin, or choose Custom Alpha.";
  return "No Alpha plan could be built. Review the reported constraints before changing these values.";
}
export type AlphaUndo = {
  before: Patch;
  applied: Patch;
  baseline: Patch;
  creationSeed?: string | null;
};
export function planDraft(plan: AlphaPlan): Patch {
  // The API serializes nullable PokemonPatch members. Null means "preserve",
  // so only actual plan changes may override the existing client draft.
  const next: Patch = Object.fromEntries(
    Object.entries(plan.patch || plan.options?.details || {}).filter(
      ([, value]) => value != null,
    ),
  ) as Patch;
  const options = plan.options;
  if (options) {
    for (const name of ["species", "form", "level", "nature", "ball"])
      if (next[name] == null && options[name] != null)
        next[name] = options[name];
    if (next.shiny == null && options.shiny != null)
      next.shiny = options.shiny ? "Star" : "NonShiny";
  }
  return next;
}
export function undoAlphaChanges(values: Patch, undo: AlphaUndo): Patch {
  const changes: Patch = {};
  for (const field of Object.keys(undo.applied)) {
    if (JSON.stringify(values[field]) !== JSON.stringify(undo.applied[field]))
      continue;
    changes[field] = Object.hasOwn(undo.before, field)
      ? undo.before[field]
      : (undo.baseline[field] ?? null);
  }
  return changes;
}
const isMint = (id: number) =>
  id === 12 || (id >= 0 && id < 25 && id % 6 !== 0);
export function natureChange(
  values: Patch,
  next: number,
  explicitMint: boolean,
  automatic: boolean,
) {
  const original = Number(values.nature ?? 15),
    effective = Number(values.statNature ?? original);
  const independent = explicitMint || effective !== original;
  const invalidNeutral =
    independent && !isMint(effective) && effective !== next;
  return {
    patch: {
      nature: next,
      statNature: automatic
        ? null
        : !independent || invalidNeutral
          ? next
          : effective,
    } as Patch,
    resetMint: invalidNeutral,
  };
}
