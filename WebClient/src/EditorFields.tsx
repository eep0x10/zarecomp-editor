import { useEffect, useState, type ReactNode } from "react";
import { Plus, X, Info, FileText, Code } from "lucide-react";
import type { Patch, Choice, Details, Catalogs } from "./api";
type Props = {
  tab: string;
  values: Patch;
  patch: Patch;
  creating: boolean;
  catalogs: Catalogs;
  metadata: { forms: Choice[]; genders: Choice[]; plusMoves: Choice[] };
  details: Details | null;
  t: (s: string) => string;
  set: (name: string, value: any) => void;
  update: (p: Patch) => void;
  scale: (n: number) => void;
  error: string;
  shutdown: () => void;
  openApi: () => void;
};
export function SecondaryFields({
  tab,
  values,
  patch,
  creating,
  catalogs,
  metadata,
  details,
  t,
  set,
  scale,
  error,
  shutdown,
  openApi,
}: Props) {
  const v = (name: string, defaultValue: any = "") =>
    values[name] ?? defaultValue;
  const [plusCandidate, setPlusCandidate] = useState(0),
    [apiSpec, setApiSpec] = useState<any>(null),
    [apiError, setApiError] = useState("");
  useEffect(
    () => setPlusCandidate(metadata.plusMoves[0]?.id || 0),
    [metadata.plusMoves],
  );
  function field(
    name: string,
    label: string,
    child: ReactNode,
    defaultValue: any,
  ) {
    const automatic = creating && patch[name] == null;
    return (
      <div className={"optional-field " + (automatic ? "automatic" : "")}>
        <div className="optional-heading">
          <span>{t(label)}</span>
          {creating && (
            <button
              type="button"
              className="auto-button"
              onClick={() => set(name, automatic ? defaultValue : null)}
              aria-pressed={!automatic}
            >
              {t(automatic ? "Automatic" : "Customize")}
            </button>
          )}
        </div>
        <fieldset disabled={automatic} className="field-controls">
          {child}
        </fieldset>
        {error.toLowerCase().startsWith(name.toLowerCase() + ":") && (
          <small className="invalid" role="alert">
            {error}
          </small>
        )}
      </div>
    );
  }
  function numeric(
    name: string,
    label: string,
    min: number,
    max: number,
    defaultValue = 0,
  ) {
    return field(
      name,
      label,
      <input
        aria-label={t(label)}
        type="number"
        min={min}
        max={max}
        value={Number(v(name, defaultValue))}
        onChange={(e) => {
          if (e.target.value !== "") set(name, Number(e.target.value));
        }}
      />,
      defaultValue,
    );
  }
  function text(name: string, label: string, max = 12) {
    return field(
      name,
      label,
      <input
        aria-label={t(label)}
        maxLength={max}
        value={String(v(name))}
        onChange={(e) => set(name, e.target.value)}
      />,
      String(v(name)),
    );
  }
  function choose(
    name: string,
    label: string,
    list: Choice[],
    defaultValue = 0,
    required = false,
  ) {
    const selected = Number(v(name, defaultValue));
    const input = (
      <select
        aria-label={t(label)}
        value={selected}
        onChange={(e) => set(name, Number(e.target.value))}
      >
        {!list.some((c) => c.id === selected) && (
          <option value={selected}>
            {t("Stored value")} #{selected}
          </option>
        )}
        {list.map((c) => (
          <option key={c.id} value={c.id}>
            {t(c.name)}
          </option>
        ))}
      </select>
    );
    return required ? (
      <label className="field">
        <span>{t(label)}</span>
        {input}
      </label>
    ) : (
      field(name, label, input, selected)
    );
  }
  function toggle(name: string, label: string) {
    return field(
      name,
      label,
      <label className="switch-field">
        <input
          aria-label={t(label)}
          type="checkbox"
          checked={!!v(name, false)}
          onChange={(e) => set(name, e.target.checked)}
        />
        <span>{t(label)}</span>
      </label>,
      !!v(name, false),
    );
  }
  const gender = [
    { id: 0, name: "Male" },
    { id: 1, name: "Female" },
  ];
  const readonly = (label: string, value: unknown) => (
    <div className="read-only-pair" key={label}>
      <dt>{t(label)}</dt>
      <dd>{Array.isArray(value) ? value.join(" / ") : String(value ?? "—")}</dd>
    </div>
  );
  if (tab === "Profile")
    return (
      <>
        <div className="field-grid">
          {choose("species", "Species", catalogs.species || [], 4, true)}
          {choose("form", "Form", metadata.forms, 0, true)}
          {choose("gender", "Gender", metadata.genders)}
          {choose("heldItem", "Held item", [
            { id: 0, name: "None" },
            ...(catalogs.heldItems || []),
          ])}
          {toggle("isNicknamed", "Use nickname")}
          {numeric("friendship", "Friendship", 0, 255)}
          {field(
            "shiny",
            "Shiny style",
            <select
              aria-label={t("Shiny style")}
              value={String(v("shiny", "NonShiny"))}
              onChange={(e) => set("shiny", e.target.value)}
            >
              {["NonShiny", "Star", "Square"].map((id, i) => (
                <option key={id} value={id}>
                  {t(["Non-shiny", "Star shiny", "Square shiny"][i])}
                </option>
              ))}
            </select>,
            "NonShiny",
          )}
        </div>
        {field(
          "markings",
          "Markings",
          <div className="markings-grid">
            {[0, 1, 2, 3, 4, 5].map((i) => (
              <label key={i}>
                <span aria-hidden="true">
                  {["●", "▲", "■", "♥", "★", "◆"][i]}
                </span>
                <select
                  aria-label={t("Markings") + " " + (i + 1)}
                  value={(v("markings", [0, 0, 0, 0, 0, 0]) as number[])[i]}
                  onChange={(e) => {
                    const marks = [
                      ...(v("markings", [0, 0, 0, 0, 0, 0]) as number[]),
                    ];
                    marks[i] = Number(e.target.value);
                    set("markings", marks);
                  }}
                >
                  {["Unmarked", "Blue", "Red"].map((name, value) => (
                    <option key={value} value={value}>
                      {t(name)}
                    </option>
                  ))}
                </select>
              </label>
            ))}
          </div>,
          [0, 0, 0, 0, 0, 0],
        )}
        <p className="helper">
          <Info size={14} />
          {t(
            "Mint changes effective nature only. Species and form changes remain subject to encounter validation.",
          )}
        </p>
      </>
    );
  if (tab === "Moves")
    return (
      <>
        {field(
          "moves",
          "Moves",
          <div className="moves-list">
            {[0, 1, 2, 3].map((i) => (
              <label className="field" key={i}>
                <span>
                  {t("Move")} {i + 1}
                </span>
                <select
                  aria-label={t("Move") + " " + (i + 1)}
                  value={(v("moves", [0, 0, 0, 0]) as number[])[i]}
                  onChange={(e) => {
                    const moves = [...(v("moves", [0, 0, 0, 0]) as number[])];
                    moves[i] = Number(e.target.value);
                    set("moves", moves);
                  }}
                >
                  {!(catalogs.moves || []).some(
                    (c) => c.id === (v("moves", [0, 0, 0, 0]) as number[])[i],
                  ) && (
                    <option value={(v("moves", [0, 0, 0, 0]) as number[])[i]}>
                      #{(v("moves", [0, 0, 0, 0]) as number[])[i]}
                    </option>
                  )}
                  {(catalogs.moves || []).map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.id === 0 ? t("None") : c.name}
                    </option>
                  ))}
                </select>
              </label>
            ))}
          </div>,
          [0, 0, 0, 0],
        )}
        {field(
          "plusMoves",
          "Registered + moves",
          <>
            <div className="plus-add">
              <select
                aria-label={t("Registered + moves")}
                value={plusCandidate}
                onChange={(e) => setPlusCandidate(Number(e.target.value))}
              >
                {metadata.plusMoves.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
              <button
                aria-label={t("Add move")}
                disabled={
                  !plusCandidate ||
                  (v("plusMoves", []) as number[]).includes(plusCandidate)
                }
                onClick={() =>
                  set("plusMoves", [
                    ...(v("plusMoves", []) as number[]),
                    plusCandidate,
                  ])
                }
              >
                <Plus size={16} />
              </button>
            </div>
            <div className="plus-chips">
              {(v("plusMoves", []) as number[]).map((id) => (
                <button
                  key={id}
                  onClick={() =>
                    set(
                      "plusMoves",
                      (v("plusMoves", []) as number[]).filter((x) => x !== id),
                    )
                  }
                >
                  {catalogs.moves?.find((c) => c.id === id)?.name || "#" + id}
                  <X size={13} />
                  <span className="sr-only">{t("Remove")}</span>
                </button>
              ))}
            </div>
          </>,
          [],
        )}
        <p className="helper">
          <Info size={14} />
          {t(
            "Registered + moves do not bypass learnability. PP and relearn data are preserved.",
          )}
        </p>
        {!creating && (
          <dl className="read-only-list">
            {readonly("Stored PP", details?.values.pp)}
            {readonly("Stored PP Ups", details?.values.ppUps)}
            {readonly(
              "Relearn moves",
              ((details?.values.relearnMoves as number[]) || []).map(
                (id) => catalogs.moves?.find((c) => c.id === id)?.name || id,
              ),
            )}
          </dl>
        )}
      </>
    );
  if (tab === "Origin")
    return (
      <>
        <div className="field-grid">
          {text("originalTrainerName", "Original trainer")}
          {choose("originalTrainerGender", "Trainer gender", gender)}
          {numeric("tiD16", "Trainer ID (16-bit)", 0, 65535)}
          {numeric("siD16", "Secret ID (16-bit)", 0, 65535)}
          {choose("language", "Pokémon language", catalogs.languages || [], 2)}
          {numeric("metLevel", "Encounter level", 1, 100, 10)}
          {choose(
            "metLocation",
            "Encounter location",
            catalogs.metLocations || [],
          )}
          {field(
            "metDate",
            "Encounter date",
            <input
              aria-label={t("Encounter date")}
              type="date"
              min="2000-01-01"
              max="2099-12-31"
              value={String(v("metDate"))}
              onChange={(e) => set("metDate", e.target.value || null)}
            />,
            new Date().toISOString().slice(0, 10),
          )}
          {numeric("obedienceLevel", "Obedience level", 0, 100)}
        </div>
        <p className="helper">
          <Info size={14} />
          {t(
            "Trainer and encounter changes affect identity and legality. A backup does not guarantee online compatibility.",
          )}
        </p>
      </>
    );
  if (tab === "Advanced")
    return (
      <>
        <div className="preset-size">
          <span>{t("Size")}</span>
          {[0, 64, 128, 192, 255].map((n, i) => (
            <button
              key={n}
              onClick={() => scale(n)}
              aria-pressed={Number(v("scale", 128)) === n}
              title={t(["Minimum", "Small", "Average", "Large", "Maximum"][i])}
            >
              {n}
            </button>
          ))}
        </div>
        <div className="field-grid">
          {numeric("heightScalar", "Height scalar", 0, 255)}
          {numeric("weightScalar", "Weight scalar", 0, 255)}
          {choose(
            "affixedRibbon",
            "Affixed title",
            [
              { id: -1, name: "None" },
              ...(details?.readOnly.ribbons || []).map(
                (id: number) =>
                  catalogs.ribbons?.find((c) => c.id === id) || {
                    id,
                    name: "#" + id,
                  },
              ),
            ],
            -1,
          )}
        </div>
        <p className="helper">
          <Info size={14} />
          {t(
            "Native Z-A keeps stored height and weight at zero; HOME records keep both equal to Scale.",
          )}
        </p>
        {!creating && details && (
          <details className="report technical" open>
            <summary>
              <FileText size={16} />
              {t("Read-only information")}
            </summary>
            <dl className="read-only-list">
              {readonly(
                "PID / EC",
                `${details.readOnly.pid} / ${details.readOnly.encryptionConstant}`,
              )}
              {readonly("HOME tracker", details.readOnly.tracker)}
              {readonly("Origin game", details.readOnly.originGame)}
              {readonly(
                "Ability / slot",
                `${details.readOnly.ability} / ${details.readOnly.abilitySlot}`,
              )}
              {readonly(
                "Displayed trainer IDs",
                `${details.readOnly.displayTID} / ${details.readOnly.displaySID}`,
              )}
              {readonly(
                "Handler",
                `${details.readOnly.currentHandler} · ${details.readOnly.handlingTrainerName}`,
              )}
              {readonly(
                "Egg data",
                `${details.readOnly.isEgg} / ${details.readOnly.eggLocation} / ${details.readOnly.eggDate || "—"}`,
              )}
              {readonly(
                "Owned ribbons",
                (details.readOnly.ribbons || []).map(
                  (id: number) =>
                    catalogs.ribbons?.find((c) => c.id === id)?.name ||
                    "#" + id,
                ),
              )}
              {readonly("Preserved fields", details.readOnly.unsupported)}
            </dl>
            <details className="report">
              <summary>{t("Original PKHeX report")}</summary>
              <pre>{details.legalityReport}</pre>
              {details.warnings?.map((w, i) => (
                <p key={i}>{w}</p>
              ))}
            </details>
          </details>
        )}
        <details className="report api-reference">
          <summary
            onClick={() => {
              if (!apiSpec)
                fetch("/openapi/v1.json")
                  .then((r) => {
                    if (!r.ok) throw new Error(r.statusText);
                    return r.json();
                  })
                  .then(setApiSpec)
                  .catch((e) => setApiError(e.message));
            }}
          >
            <Code size={16} />
            {t("Local API reference")}
          </summary>
          <p className="helper">
            {t(
              "Interactive API documentation is available at /api-docs on this local host. The runtime token stays private.",
            )}
          </p>
          {apiError && <p>{apiError}</p>}
          {apiSpec && (
            <div className="api-operations">
              {Object.entries(apiSpec.paths || {}).map(([path, methods]) => (
                <div key={path}>
                  <code>
                    {Object.keys(methods as object)
                      .map((m) => m.toUpperCase())
                      .join(" / ")}{" "}
                    {path}
                  </code>
                </div>
              ))}
            </div>
          )}
        </details>
        <div className="developer-actions">
          <button onClick={openApi}>{t("Local API")}</button>
          <button onClick={shutdown}>{t("Stop editor")}</button>
        </div>
      </>
    );
  return null;
}
