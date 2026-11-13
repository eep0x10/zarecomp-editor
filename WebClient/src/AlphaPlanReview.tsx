import {
  alphaChangeValue,
  alphaUnavailableMessage,
  type AlphaPlan,
} from "./alphaDraft";
import type { Catalogs } from "./api";
import { localizeError } from "./errors";
import type { Locale } from "./i18n";
import { Info, Check, X } from "lucide-react";
const labels: Record<string, string> = {
  Species: "Species",
  Form: "Form",
  Nickname: "Nickname",
  IsNicknamed: "Use nickname",
  Shiny: "Shiny style",
  HeldItem: "Held item",
  Language: "Pokémon language",
  Friendship: "Friendship",
  Moves: "Moves",
  Pp: "Stored PP",
  PpUps: "Stored PP Ups",
  RelearnMoves: "Relearn moves",
  OriginalTrainerName: "Original trainer",
  OriginalTrainerGender: "Trainer gender",
  TiD16: "Trainer ID (16-bit)",
  SiD16: "Secret ID (16-bit)",
  AffixedRibbon: "Affixed title",
  Favorite: "Favorite",
  Markings: "Markings",
  Level: "Level",
  MetLevel: "Encounter level",
  MetLocation: "Encounter location",
  ObedienceLevel: "Obedience level",
  IsAlpha: "Alpha",
  AlphaMode: "Alpha mode",
  Scale: "Scale",
  HeightScalar: "Height scalar",
  WeightScalar: "Weight scalar",
  IVs: "IVs",
  HyperTrained: "Hyper Training",
  PlusMoves: "Registered + moves",
  Gender: "Gender",
  Nature: "Nature",
  StatNature: "Effective nature",
  Ball: "Ball",
};
export function AlphaPlanReview({
  plan,
  locale,
  catalogs,
  t,
  apply,
  cancel,
}: {
  plan: AlphaPlan;
  locale: Locale;
  catalogs: Catalogs;
  t: (s: string) => string;
  apply: () => void;
  cancel: () => void;
}) {
  return (
    <section
      className="alpha-plan-review"
      aria-label={t("Alpha conversion preview")}
    >
      <h3>
        <Info size={16} />
        {t("Alpha conversion preview")}
      </h3>
      <p>
        {t(
          plan.mode === "Custom"
            ? "Custom Alpha preserves the encounter. Compatibility findings remain visible; this does not certify legality."
            : "Compatible Alpha may adapt encounter details. Review the changes before applying them to the draft.",
        )}
      </p>
      {plan.minimumLevel != null && (
        <p>
          {t("Minimum level")}: <strong>{plan.minimumLevel}</strong>
        </p>
      )}
      {plan.requiresOriginChange && (
        <p className="warning">
          {t("Encounter details will change only after you apply this plan.")}
        </p>
      )}
      {plan.changes.length > 0 && (
        <div className="alpha-diff">
          <table>
            <thead>
              <tr>
                <th>{t("Field")}</th>
                <th>{t("Before")}</th>
                <th>{t("After")}</th>
              </tr>
            </thead>
            <tbody>
              {plan.changes.map((change, i) => (
                <tr key={change.field + i}>
                  <th>
                    {t(
                      labels[change.field] ||
                        labels[
                          change.field[0]?.toUpperCase() + change.field.slice(1)
                        ] ||
                        change.field,
                    )}
                  </th>
                  <td>
                    {alphaChangeValue(change.field, change.before, catalogs, t)}
                  </td>
                  <td>
                    {alphaChangeValue(change.field, change.after, catalogs, t)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {plan.warnings.map((message, i) => (
        <p className="warning" key={i}>
          {localizeError(locale, message)}
        </p>
      ))}
      {plan.preview && (
        <details className="report">
          <summary>
            PKHeX ·{" "}
            {t(
              plan.preview.legal ? "No legality findings" : "Legality findings",
            )}
          </summary>
          <pre>{plan.preview.legalityReport}</pre>
        </details>
      )}
      {!plan.available && (
        <p className="warning">{t(alphaUnavailableMessage(plan))}</p>
      )}
      <div className="alpha-plan-actions">
        <button onClick={cancel}>
          <X size={15} />
          {t("Cancel plan")}
        </button>
        <button
          className="primary"
          disabled={!plan.available || (!plan.patch && !plan.options)}
          onClick={apply}
        >
          <Check size={15} />
          {t(
            plan.mode === "Custom"
              ? "Apply custom Alpha to draft"
              : "Apply compatible Alpha to draft",
          )}
        </button>
      </div>
      <small>
        {t(
          "This preview does not write the save. Save with backup remains a separate action.",
        )}
      </small>
    </section>
  );
}
