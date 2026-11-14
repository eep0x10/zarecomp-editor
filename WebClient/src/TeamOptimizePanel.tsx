import { useEffect, useRef } from "react";
import { animate, createScope } from "animejs";
import {
  Sparkles,
  X,
  Check,
  Info,
  LoaderCircle,
  TriangleAlert,
  ChevronDown,
} from "lucide-react";
import type { Catalogs } from "./api";
import type { Locale } from "./i18n";
import { alphaChangeValue, type AlphaMode } from "./alphaDraft";
import { localizeError } from "./errors";
import {
  optimizeOptionLabels,
  type OptimizeOptions,
  type TeamOptimizePlan,
} from "./teamOptimize";
type Props = {
  options: OptimizeOptions;
  setOptions: (o: OptimizeOptions) => void;
  mode: AlphaMode;
  setMode: (m: AlphaMode) => void;
  plan: TeamOptimizePlan | null;
  planning: boolean;
  notice: string;
  blocked: boolean;
  stale: boolean;
  generate: () => void;
  apply: () => void;
  cancel: () => void;
  close: () => void;
  locale: Locale;
  catalogs: Catalogs;
  reduced: boolean;
  t: (s: string) => string;
};
const fields: Record<string, string> = {
  statNature: "Effective nature",
  nature: "Original nature",
  isAlpha: "Alpha",
  alphaMode: "Alpha mode",
  iVs: "IVs",
  hyperTrained: "Hyper Training",
  level: "Level",
  friendship: "Friendship",
  scale: "Size",
  heightScalar: "Height scalar",
  weightScalar: "Weight scalar",
  shiny: "Shiny style",
  metLocation: "Encounter location",
  metLevel: "Encounter level",
  obedienceLevel: "Obedience level",
  moves: "Moves",
  plusMoves: "Registered + moves",
  species: "Species",
  form: "Form",
  ball: "Ball",
  nickname: "Nickname",
};
export function TeamOptimizePanel({
  options,
  setOptions,
  mode,
  setMode,
  plan,
  planning,
  notice,
  blocked,
  stale,
  generate,
  apply,
  cancel,
  close,
  locale,
  catalogs,
  reduced,
  t,
}: Props) {
  const root = useRef<HTMLElement>(null);
  useEffect(() => {
    if (!root.current || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".team-options", {
        opacity: [0.6, 1],
        translateY: [-4, 0],
        duration: 160,
        ease: "out(3)",
      });
    });
    return () => scope.revert();
  }, [reduced]);
  useEffect(() => {
    if (!root.current || !plan || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".team-plan-results", { opacity: [0.6, 1], duration: 180 });
    });
    return () => scope.revert();
  }, [plan, reduced]);
  useEffect(() => {
    if (!root.current || !planning || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".team-spinner", {
        rotate: "1turn",
        loop: true,
        duration: 900,
        ease: "linear",
      });
    });
    return () => scope.revert();
  }, [planning, reduced]);
  const selected = Object.values(options).filter(Boolean).length;
  return (
    <section
      className="team-optimize-panel"
      ref={root}
      aria-label={t("Optimize team")}
    >
      <div className="team-optimize-heading">
        <h3 tabIndex={-1}>
          <Sparkles size={18} />
          {t("Optimize team")}
        </h3>
        <button
          className="icon-button"
          aria-label={t("Close optimizer")}
          onClick={close}
        >
          <X size={17} />
        </button>
      </div>
      <p className="helper">
        <Info size={14} />
        {t(
          "Choose one or more changes. Preview first, apply to drafts, then save with backup.",
        )}
      </p>
      <fieldset className="team-options">
        <legend className="sr-only">{t("Optimization options")}</legend>
        {(Object.keys(optimizeOptionLabels) as (keyof OptimizeOptions)[]).map(
          (key) => (
            <label key={key}>
              <input
                type="checkbox"
                checked={options[key]}
                onChange={(e) =>
                  setOptions({ ...options, [key]: e.target.checked })
                }
              />
              <span>{t(optimizeOptionLabels[key])}</span>
            </label>
          ),
        )}
      </fieldset>
      {options.bestNatures && (
        <p className="team-option-note">
          {t(
            "Recommendations are stable for each species/form and apply as a Mint. Original nature is preserved.",
          )}
        </p>
      )}
      {options.alpha && (
        <label className="team-mode">
          <span>{t("Alpha mode")}</span>
          <select
            value={mode}
            onChange={(e) => setMode(e.target.value as AlphaMode)}
          >
            <option value="Compatible">{t("Compatible encounter")}</option>
            <option value="Custom">{t("Custom: keep encounter")}</option>
          </select>
        </label>
      )}
      {options.alpha && mode === "Custom" && (
        <p className="warning team-option-note">
          {t(
            "Custom Alpha preserves origin and may remain incompatible. The actual PKHeX report will be shown.",
          )}
        </p>
      )}
      <div className="team-plan-buttons">
        <button
          className="team-generate"
          disabled={!selected || planning || blocked}
          onClick={generate}
        >
          {planning ? (
            <LoaderCircle size={16} className="team-spinner" />
          ) : (
            <Sparkles size={16} />
          )}{" "}
          {t(planning ? "Planning team changes…" : "Preview team changes")}
        </button>
        {(planning || plan) && (
          <button
            onClick={() => {
              cancel();
              requestAnimationFrame(() =>
                root.current
                  ?.querySelector<HTMLButtonElement>(".team-generate")
                  ?.focus({ preventScroll: true }),
              );
            }}
          >
            {t("Cancel preview")}
          </button>
        )}
      </div>
      {!selected && (
        <p className="helper">{t("Select at least one option.")}</p>
      )}
      {(notice || stale) && (
        <p className="team-plan-notice" role="status">
          {stale
            ? t("Inputs changed. Generate a new preview before applying.")
            : notice}
        </p>
      )}
      {plan && (
        <div className="team-plan-results" aria-live="polite">
          <div
            className={
              "team-plan-summary " + (plan.available ? "good" : "warning")
            }
          >
            {plan.available ? <Check size={17} /> : <TriangleAlert size={17} />}
            <strong>
              {plan.items.length} {t("Pokémon reviewed")} ·{" "}
              {plan.items.filter((item) => item.changes.length).length}{" "}
              {t("with proposed changes")}
            </strong>
          </div>
          {!plan.available && (
            <p className="warning team-option-note">
              {t(
                "Nothing was applied. At least one Pokémon has no available plan. Adjust the options or review Alpha mode before trying again.",
              )}
            </p>
          )}
          {plan.warnings.map((warning, i) => (
            <p key={i} className="warning team-option-note">
              {localizeError(locale, warning)}
            </p>
          ))}
          <div className="team-plan-items">
            {plan.items.map((item) => (
              <details className="team-plan-item" key={item.slot}>
                <summary>
                  <span>
                    <strong>{item.name}</strong>
                    <small>
                      #{String(item.species).padStart(4, "0")} · {t("Team")}{" "}
                      {item.slot + 1}
                    </small>
                  </span>
                  <span className={item.available ? "good" : "warning"}>
                    {t(item.available ? "Available" : "Unavailable")} ·{" "}
                    {item.changes.length}
                    <ChevronDown size={14} aria-hidden="true" />
                  </span>
                </summary>
                {item.nature != null && (
                  <p className="team-nature">
                    <strong>
                      {t("Recommended nature (Mint)")}:{" "}
                      {catalogs.natures?.find((c) => c.id === item.nature)
                        ?.name || "#" + item.nature}
                    </strong>
                    {item.natureReason && <small>{item.natureReason}</small>}
                  </p>
                )}
                {item.changes.length > 0 && (
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
                        {item.changes.map((change, i) => (
                          <tr key={change.field + i}>
                            <th>{t(fields[change.field] || change.field)}</th>
                            <td>
                              {alphaChangeValue(
                                change.field,
                                change.before,
                                catalogs,
                                t,
                              )}
                            </td>
                            <td>
                              {alphaChangeValue(
                                change.field,
                                change.after,
                                catalogs,
                                t,
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
                {item.warnings.map((warning, i) => (
                  <p key={i} className="warning team-option-note">
                    {localizeError(locale, warning)}
                  </p>
                ))}
                {item.preview && (
                  <details className="report">
                    <summary>
                      PKHeX ·{" "}
                      {t(
                        item.preview.legal
                          ? "No legality findings"
                          : "Legality findings",
                      )}
                    </summary>
                    <pre>{item.preview.legalityReport}</pre>
                  </details>
                )}
              </details>
            ))}
          </div>
          <button
            className="primary team-apply"
            disabled={
              !plan.available ||
              plan.items.some((item) => !item.available) ||
              planning ||
              blocked ||
              stale
            }
            onClick={apply}
          >
            <Check size={17} />
            {t("Apply team plan to drafts")}
          </button>
          <small className="team-readonly-note">
            {t(
              "This preview does not write the save. Save with backup remains a separate action.",
            )}
          </small>
        </div>
      )}
    </section>
  );
}
