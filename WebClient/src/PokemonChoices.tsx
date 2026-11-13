import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import { createScope, animate } from "animejs";
import { ChevronDown, Check } from "lucide-react";
import type { Choice } from "./api";
import type { Locale } from "./i18n";
import { translate } from "./i18n";
export function genderChoiceLabel(choice: Choice) {
  return ["Male", "Female", "Genderless"][choice.id] || choice.name;
}
export function natureModifiers(id: number): {
  up: string | null;
  down: string | null;
} {
  if (id < 0 || id >= 25) return { up: null, down: null };
  const index = Math.floor(id / 5),
    decrease = id % 5;
  const order = ["Attack", "Defense", "Speed", "Sp. Atk", "Sp. Def"];
  return index === decrease
    ? { up: null, down: null }
    : { up: order[index], down: order[decrease] };
}
export function mintChoices(choices: Choice[], original: number) {
  return choices.filter(
    (c) => c.id === original || c.id === 12 || !!natureModifiers(c.id).up,
  );
}
export function natureEffect(id: number, t: (s: string) => string) {
  const { up, down } = natureModifiers(id);
  return up && down
    ? `+10% ${t(up)} / −10% ${t(down)}`
    : t("Neutral: no stat changes (0%)");
}
export function NatureChoice({
  label,
  value,
  choices,
  onChange,
  t,
}: {
  label: string;
  value: number;
  choices: Choice[];
  onChange: (n: number) => void;
  t: (s: string) => string;
}) {
  const mods = natureModifiers(value);
  return (
    <div className="field nature-field">
      <label htmlFor={"nature-" + label}>{t(label)}</label>
      <select
        id={"nature-" + label}
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
      >
        {choices.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name} · {natureEffect(c.id, t)}
          </option>
        ))}
      </select>
      <div className="nature-effect" aria-live="polite">
        {mods.up && mods.down ? (
          <>
            <span className="nature-up">+10% {t(mods.up)}</span>
            <span className="nature-down">−10% {t(mods.down)}</span>
          </>
        ) : (
          <span>{t("Neutral: no stat changes (0%)")}</span>
        )}
      </div>
    </div>
  );
}
export function BallPicker({
  value,
  choices,
  onChange,
  locale,
}: {
  value: number;
  choices: Choice[];
  onChange: (n: number) => void;
  locale: Locale;
}) {
  const t = (s: string) => translate(locale, s);
  const [open, setOpen] = useState(false),
    [focused, setFocused] = useState(value);
  const root = useRef<HTMLDivElement>(null),
    trigger = useRef<HTMLButtonElement>(null);
  const selected = choices.find((c) => c.id === value);
  const picture = (id: number) => (
    <img
      src={`/assets/balls/${id}.png`}
      width={32}
      height={32}
      alt=""
      draggable={false}
    />
  );
  useEffect(() => {
    setOpen(false);
    setFocused(value);
  }, [value]);
  useEffect(() => {
    if (!root.current || !open) return;
    const scope = createScope({
      root,
      mediaQueries: { reduce: "(prefers-reduced-motion: reduce)" },
    }).add((self) => {
      if (self && !self.matches.reduce)
        animate(".ball-options", {
          opacity: [0.6, 1],
          translateY: [-4, 0],
          duration: 160,
          ease: "out(3)",
        });
    });
    return () => scope.revert();
  }, [open]);
  const focusId = (id: number) => {
    setFocused(id);
    requestAnimationFrame(() =>
      root.current
        ?.querySelector<HTMLButtonElement>(`[data-ball-id="${id}"]`)
        ?.focus(),
    );
  };
  function keyDown(e: KeyboardEvent<HTMLDivElement>) {
    if (e.key === "Escape") {
      setOpen(false);
      trigger.current?.focus();
      e.preventDefault();
      return;
    }
    const i = choices.findIndex((c) => c.id === focused);
    const columns = root.current?.querySelector(".ball-options")
      ? getComputedStyle(
          root.current.querySelector(".ball-options")!,
        ).gridTemplateColumns.split(" ").length
      : 3;
    let next = i;
    if (e.key === "ArrowRight") next = Math.min(i + 1, choices.length - 1);
    else if (e.key === "ArrowLeft") next = Math.max(i - 1, 0);
    else if (e.key === "ArrowDown")
      next = Math.min(i + columns, choices.length - 1);
    else if (e.key === "ArrowUp") next = Math.max(i - columns, 0);
    else if (e.key === "Home") next = 0;
    else if (e.key === "End") next = choices.length - 1;
    else return;
    e.preventDefault();
    if (choices[next]) focusId(choices[next].id);
  }
  return (
    <div className="field ball-field" ref={root}>
      <span id="ball-label">{t("Ball")}</span>
      <button
        type="button"
        className="ball-current"
        ref={trigger}
        aria-labelledby="ball-label ball-current-name"
        aria-expanded={open}
        aria-controls="ball-options"
        onClick={() => {
          setOpen((v) => !v);
          if (!open) focusId(value);
        }}
      >
        {picture(value)}
        <span id="ball-current-name">{selected?.name || "#" + value}</span>
        <ChevronDown size={14} />
      </button>
      {open && (
        <div
          id="ball-options"
          role="listbox"
          aria-labelledby="ball-label"
          className="ball-options"
          onKeyDown={keyDown}
        >
          {choices.map((c) => (
            <button
              role="option"
              type="button"
              key={c.id}
              data-ball-id={c.id}
              aria-selected={c.id === value}
              tabIndex={c.id === focused ? 0 : -1}
              className={c.id === value ? "selected" : ""}
              onFocus={() => setFocused(c.id)}
              onClick={() => {
                onChange(c.id);
                setOpen(false);
                trigger.current?.focus();
              }}
            >
              {picture(c.id)}
              <span>{c.name}</span>
              {c.id === value && <Check size={12} />}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
