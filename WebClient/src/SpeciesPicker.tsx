import {
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
  type KeyboardEvent,
} from "react";
import { Search, ChevronDown, Check } from "lucide-react";
import { createScope, animate } from "animejs";
import type { Choice, Patch } from "./api";
const normalize = (value: string) =>
  value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9♀♂]+/g, " ")
    .trim();
export function filterSpecies(catalog: Choice[], query: string) {
  const q = query.trim();
  if (!q) return catalog;
  if (/^#?\d+$/.test(q)) {
    const id = Number(q.replace("#", ""));
    return catalog.filter((c) => c.id === id);
  }
  const words = normalize(q).split(/\s+/).filter(Boolean);
  return catalog.filter((c) =>
    words.every((word) => normalize(c.name).includes(word)),
  );
}
export function speciesCreationChanges(
  species: number,
  form: number,
  gender: number | null,
): Patch {
  return { species, form, gender, moves: null, plusMoves: null };
}
type Props = {
  catalog: Choice[];
  value: number;
  open: boolean;
  setOpen: (open: boolean) => void;
  onChoose: (id: number) => Promise<boolean>;
  renderSprite: (id: number) => ReactNode;
  t: (s: string) => string;
};
export function SpeciesPicker({
  catalog,
  value,
  open,
  setOpen,
  onChoose,
  renderSprite,
  t,
}: Props) {
  const [query, setQuery] = useState(""),
    [focused, setFocused] = useState(value),
    [choosing, setChoosing] = useState(false);
  const root = useRef<HTMLDivElement>(null),
    trigger = useRef<HTMLButtonElement>(null),
    grid = useRef<HTMLDivElement>(null);
  const filtered = useMemo(
    () => filterSpecies(catalog, query),
    [catalog, query],
  );
  useEffect(() => {
    if (open) {
      setQuery("");
      setFocused(value);
      if (grid.current) grid.current.scrollTop = 0;
    }
  }, [open]);
  useEffect(() => {
    if (!open || !root.current) return;
    const scope = createScope({
      root,
      mediaQueries: { reduced: "(prefers-reduced-motion: reduce)" },
    }).add((self) => {
      if (self && !self.matches.reduced)
        animate(".species-panel", { opacity: [0.6, 1], duration: 160 });
    });
    return () => scope.revert();
  }, [open]);
  const focus = (id: number) => {
    setFocused(id);
    requestAnimationFrame(() =>
      grid.current
        ?.querySelector<HTMLButtonElement>(`[data-species-id="${id}"]`)
        ?.focus(),
    );
  };
  function navigate(e: KeyboardEvent<HTMLDivElement>) {
    if (e.key === "Escape") {
      setOpen(false);
      trigger.current?.focus();
      e.preventDefault();
      return;
    }
    if (!filtered.length) return;
    const current = Math.max(
      0,
      filtered.findIndex((c) => c.id === focused),
    );
    const columns = grid.current
      ? getComputedStyle(grid.current).gridTemplateColumns.split(" ").length
      : 3;
    let next = current;
    if (e.key === "ArrowRight")
      next = Math.min(current + 1, filtered.length - 1);
    else if (e.key === "ArrowLeft") next = Math.max(current - 1, 0);
    else if (e.key === "ArrowDown")
      next = Math.min(current + columns, filtered.length - 1);
    else if (e.key === "ArrowUp") next = Math.max(current - columns, 0);
    else if (e.key === "Home") next = 0;
    else if (e.key === "End") next = filtered.length - 1;
    else return;
    e.preventDefault();
    focus(filtered[next].id);
  }
  async function choose(id: number) {
    if (choosing) return;
    setChoosing(true);
    try {
      if (await onChoose(id)) {
        setOpen(false);
        requestAnimationFrame(() =>
          trigger.current?.focus({ preventScroll: true }),
        );
      }
    } finally {
      setChoosing(false);
    }
  }
  const selected = catalog.find((c) => c.id === value);
  return (
    <div className="species-picker" ref={root}>
      <button
        className="choose-species"
        ref={trigger}
        type="button"
        aria-expanded={open}
        aria-controls="species-panel"
        onClick={() => setOpen(!open)}
      >
        <Search size={18} />
        <strong>{t("Choose Pokémon")}</strong>
        <span>{selected?.name || "#" + value}</span>
        <ChevronDown size={16} />
      </button>
      {open && (
        <section
          id="species-panel"
          className="species-panel"
          aria-label={t("Choose Pokémon")}
        >
          <label className="species-search">
            <Search size={17} />
            <input
              type="search"
              aria-label={t("Search by name or number")}
              placeholder={t("Search by name or number")}
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
                setFocused(
                  filterSpecies(catalog, e.target.value)[0]?.id || value,
                );
                if (grid.current) grid.current.scrollTop = 0;
              }}
              autoComplete="off"
            />
          </label>
          <p className="species-count" aria-live="polite">
            {filtered.length} {t("results")} · {catalog.length}{" "}
            {t("available species")}
          </p>
          <div
            ref={grid}
            className="species-grid"
            role="listbox"
            aria-label={t("Available Pokémon")}
            onKeyDown={navigate}
            aria-busy={choosing}
          >
            {filtered.map((c, i) => (
              <button
                key={c.id}
                type="button"
                role="option"
                data-species-id={c.id}
                aria-selected={c.id === value}
                aria-label={c.name + " #" + c.id}
                tabIndex={
                  c.id === focused ||
                  (!filtered.some((c) => c.id === focused) && i === 0)
                    ? 0
                    : -1
                }
                disabled={choosing}
                onFocus={() => setFocused(c.id)}
                onClick={() => void choose(c.id)}
              >
                {renderSprite(c.id)}
                <strong>{c.name}</strong>
                <small>#{String(c.id).padStart(4, "0")}</small>
                {c.id === value && <Check size={13} />}
              </button>
            ))}
          </div>
          {!filtered.length && <p className="helper">{t("No results")}</p>}
          <button
            className="species-back"
            type="button"
            onClick={() => {
              setOpen(false);
              trigger.current?.focus();
            }}
          >
            {t("Back to editor")}
          </button>
        </section>
      )}
    </div>
  );
}
