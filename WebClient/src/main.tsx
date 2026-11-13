import React, { useState, useEffect, useRef, useCallback } from "react";
import { createRoot } from "react-dom/client";
import { animate, createScope } from "animejs";
import {
  FolderOpen,
  Plus,
  ChevronLeft,
  ChevronRight,
  Search,
  Save,
  Trash2,
  Sparkles,
  Info,
  ChevronDown,
  Check,
  RefreshCw,
  X,
  Box,
  FileText,
  LoaderCircle,
  TriangleAlert,
  SlidersHorizontal,
} from "lucide-react";
import "@fontsource-variable/inter";
import {
  api,
  ApiError,
  spriteUrl,
  subscribeSprite,
  invalidateSprite,
  type Choice,
  type Pokemon,
  type Patch,
  type Details,
  type Session,
  type Catalogs,
} from "./api";
import { translate, type Locale } from "./i18n";
import "./style.css";
import { SecondaryFields } from "./EditorFields";
import { localizeError } from "./errors";
import { AlphaPlanReview } from "./AlphaPlanReview";
import {
  planDraft,
  undoAlphaChanges,
  natureChange,
  type AlphaPlan,
  type AlphaMode,
  type AlphaUndo,
} from "./alphaDraft";
import { BallPicker, NatureChoice, mintChoices } from "./PokemonChoices";
import { SpeciesPicker, speciesCreationChanges } from "./SpeciesPicker";
import { observeSpriteVisibility } from "./spriteVisibility";
const stats = ["HP", "Attack", "Defense", "Speed", "Sp. Atk", "Sp. Def"];
const initial: Patch = {
  species: 4,
  form: 0,
  level: 10,
  nature: 15,
  shiny: "NonShiny",
  ball: 4,
};
const same = (a: unknown, b: unknown) =>
  JSON.stringify(a) === JSON.stringify(b);
const english = (name: string) => name.match(/\(([^)]+)\)$/)?.[1] || name;
const keyOf = (party: boolean, slot: number) =>
  (party ? "party" : "box") + ":" + slot;
function Sprite({
  species,
  form = 0,
  shiny = false,
  className = "",
  lazy = false,
}: {
  species: number;
  form?: number;
  shiny?: boolean;
  className?: string;
  lazy?: boolean;
}) {
  const [visible, setVisible] = useState(!lazy);
  const placeholder = useRef<HTMLSpanElement>(null);
  useEffect(() => {
    if (!lazy || visible) return;
    if (!placeholder.current) return;
    return observeSpriteVisibility(placeholder.current, () => setVisible(true));
  }, [lazy, visible]);
  const [url, setUrl] = useState(""),
    [failed, setFailed] = useState(false);
  useEffect(() => {
    if (!visible) return;
    let live = true,
      timer: ReturnType<typeof setTimeout> | undefined,
      attempt = 0;
    setUrl("");
    setFailed(false);
    const receive = (value: string) => {
      if (!live) return;
      if (timer) clearTimeout(timer);
      timer = undefined;
      setUrl(value);
      setFailed(false);
    };
    const unsubscribe = subscribeSprite(species, form, shiny, receive);
    const load = () =>
      spriteUrl(species, form, shiny)
        .then(receive)
        .catch(() => {
          if (!live) return;
          if (attempt++ === 0) timer = setTimeout(load, 1200);
          else setFailed(true);
        });
    const reconnect = () => {
      if (timer) clearTimeout(timer);
      attempt = 0;
      void load();
    };
    window.addEventListener("online", reconnect);
    void load();
    return () => {
      live = false;
      if (timer) clearTimeout(timer);
      unsubscribe();
      window.removeEventListener("online", reconnect);
    };
  }, [species, form, shiny, visible]);
  return url ? (
    <img
      className={"sprite " + className}
      src={url}
      alt=""
      draggable={false}
      onError={() => {
        invalidateSprite(species, form, shiny);
        setUrl("");
        setFailed(true);
      }}
    />
  ) : (
    <span
      ref={placeholder}
      className={"sprite sprite-fallback " + className}
      aria-hidden="true"
    >
      <span>#{String(species).padStart(4, "0")}</span>
      <small>{failed ? "—" : "…"}</small>
    </span>
  );
}
function App() {
  const [locale, setLocale] = useState<Locale>("pt-BR");
  const [appVersion, setAppVersion] = useState("");
  const [alphaPlan, setAlphaPlan] = useState<AlphaPlan | null>(null),
    [alphaPlanning, setAlphaPlanning] = useState(false),
    [alphaChoice, setAlphaChoice] = useState<AlphaMode>("Compatible"),
    [creationSeed, setCreationSeed] = useState<string | null>(null);
  const alphaUndo = useRef<Record<string, AlphaUndo>>({}),
    mintIntent = useRef(new Set<string>());
  const t = useCallback((key: string) => translate(locale, key), [locale]);
  const [session, setSession] = useState<Session | null>(null),
    [catalogs, setCatalogs] = useState<Catalogs>({}),
    [candidates, setCandidates] = useState<any[]>([]),
    [path, setPath] = useState(""),
    [openPath, setOpenPath] = useState(false);
  const [selection, setSelection] = useState<{
      party: boolean;
      slot: number;
    } | null>(null),
    [details, setDetails] = useState<Details | null>(null),
    [drafts, setDrafts] = useState<Record<string, Patch>>({}),
    [creating, setCreating] = useState(false),
    [newPatch, setNewPatch] = useState<Patch>(initial),
    [destination, setDestination] = useState("Auto"),
    [preferredBox, setPreferredBox] = useState(-1),
    [speciesOpen, setSpeciesOpen] = useState(false);
  const [metadata, setMetadata] = useState<{
      forms: Choice[];
      genders: Choice[];
      plusMoves: Choice[];
      baseStats: number[];
    }>({ forms: [], genders: [], plusMoves: [], baseStats: [] }),
    [tab, setTab] = useState("Attributes"),
    [filter, setFilter] = useState(""),
    [boxesOpen, setBoxesOpen] = useState(false);
  const [suggestionRows, setSuggestionRows] = useState<
    { slot: number; name: string; nature: string; reason: string }[]
  >([]);
  const [busy, setBusy] = useState(""),
    [message, setMessage] = useState(""),
    [error, setError] = useState(""),
    [backup, setBackup] = useState(""),
    [preview, setPreview] = useState<Details | null>(null),
    [validating, setValidating] = useState(false),
    [pending, setPending] = useState<null | (() => void)>(null),
    [conflict, setConflict] = useState(false);
  const [reduced, setReduced] = useState(
      matchMedia("(prefers-reduced-motion: reduce)").matches,
    ),
    [motion, setMotion] = useState(0);
  const validationRequest = useRef<AbortController | null>(null),
    revision = useRef(0);
  const root = useRef<HTMLDivElement>(null),
    selectionRequest = useRef<AbortController | null>(null),
    boxRequest = useRef<AbortController | null>(null),
    metadataRequest = useRef<AbortController | null>(null),
    speciesRequest = useRef<AbortController | null>(null),
    operation = useRef(false),
    loadedKey = useRef(""),
    activeKey = selection ? keyOf(selection.party, selection.slot) : "";
  const patch = creating ? newPatch : drafts[activeKey] || {};
  const values = creating ? newPatch : { ...(details?.values || {}), ...patch };
  const draftCount = Object.keys(drafts).length;
  const editContext = creating ? "new" : activeKey;
  const sessionId = useRef("");
  sessionId.current = session?.id || "";
  const sessionHash = useRef("");
  sessionHash.current = session?.hash || "";
  const val = (name: string, fallback: any = "") => values[name] ?? fallback;
  const num = (name: string, fallback = 0) => Number(val(name, fallback));
  const localeRef = useRef(locale);
  localeRef.current = locale;
  const readyKey = creating
    ? `new:${num("species")}:${num("form")}`
    : details && loadedKey.current === activeKey
      ? `${activeKey}:${num("species")}:${num("form")}`
      : "";
  const fail = useCallback(
    (e: any) => {
      if (e?.name === "AbortError") return;
      setError(
        e instanceof ApiError && e.code === "stale_hash"
          ? t("The save changed externally. Reopen it before writing.")
          : localizeError(locale, e?.message || String(e)),
      );
      if (e instanceof ApiError && e.code === "stale_hash") setConflict(true);
      setMessage("");
    },
    [t, locale],
  );
  useEffect(() => {
    const q = matchMedia("(prefers-reduced-motion: reduce)");
    const change = () => setReduced(q.matches);
    q.addEventListener("change", change);
    return () => q.removeEventListener("change", change);
  }, []);
  useEffect(() => {
    if (!root.current || !readyKey || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".inspector .preview-art", {
        opacity: [0.55, 1],
        translateY: [5, 0],
        duration: 220,
        ease: "out(3)",
      });
      if (creating)
        animate(".creation-banner", { opacity: [0.5, 1], duration: 160 });
    });
    return () => scope.revert();
  }, [readyKey, reduced]);
  useEffect(() => {
    if (!root.current || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".receipt", { opacity: [0.6, 1], duration: 180 });
    });
    return () => scope.revert();
  }, [message, error, reduced]);
  useEffect(() => {
    if (!root.current || !motion || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".changed-field", {
        backgroundColor: ["rgba(255,202,85,.23)", "rgba(255,202,85,0)"],
        duration: 700,
      });
    });
    return () => scope.revert();
  }, [motion, reduced]);
  useEffect(() => {
    if (!root.current || reduced) return;
    const scope = createScope({ root }).add(() => {
      animate(".box-card", { opacity: [0.75, 1], duration: 140 });
    });
    return () => scope.revert();
  }, [filter, reduced]);
  useEffect(() => {
    if (!root.current || reduced || (!busy && !validating)) return;
    const scope = createScope({ root }).add(() => {
      animate(".spinning", {
        rotate: "1turn",
        loop: true,
        duration: 900,
        ease: "linear",
      });
    });
    return () => scope.revert();
  }, [busy, validating, reduced]);
  useEffect(() => {
    const ac = new AbortController();
    api<any>("/bootstrap", undefined, ac.signal)
      .then(async (b) => {
        setAppVersion(b.version || "");
        setLocale(b.preferences?.language || "pt-BR");
        localeRef.current = b.preferences?.language || "pt-BR";
        document.documentElement.lang = localeRef.current;
        setPath(b.initialPath || b.preferences?.lastSavePath || "");
        setCatalogs(await api<Catalogs>("/catalogs", undefined, ac.signal));
        let resumed = false;
        const previous = history.state?.zaSessionId;
        if (typeof previous === "string" && /^[a-f0-9]{32}$/.test(previous)) {
          try {
            acceptSession(
              await api<Session>(`/sessions/${previous}`, undefined, ac.signal),
            );
            resumed = true;
          } catch (e) {
            if (!(e instanceof ApiError && e.status === 404)) throw e;
          }
        }
        if (!resumed) {
          if (b.initialPath) await openSave({ path: b.initialPath }, true);
          else setOpenPath(true);
        }
      })
      .catch(fail);
    const heartbeat = setInterval(
      () => api("/heartbeat", {}).catch(() => {}),
      30000,
    );
    return () => {
      ac.abort();
      clearInterval(heartbeat);
      validationRequest.current?.abort();
      selectionRequest.current?.abort();
      boxRequest.current?.abort();
      metadataRequest.current?.abort();
      speciesRequest.current?.abort();
    };
  }, []);
  function acceptSession(s: Session) {
    sessionId.current = s.id;
    sessionHash.current = s.hash;
    setSession(s);
    alphaUndo.current = {};
    mintIntent.current.clear();
    setAlphaPlan(null);
    setAlphaPlanning(false);
    setCreationSeed(null);
    setSuggestionRows([]);
    setDrafts({});
    setCreating(false);
    setDetails(null);
    setSelection(s.team[0] ? { party: true, slot: s.team[0].slot } : null);
    setOpenPath(false);
    setConflict(false);
    setPath(s.summary.path);
    setMessage(
      translate(localeRef.current, "Changes remain drafts until saved."),
    );
    history.replaceState(
      { zaSessionId: s.id },
      "",
      location.pathname + location.search,
    );
  }
  function guard(action: () => void) {
    if (draftCount || creating) {
      setPending(() => action);
      return;
    }
    action();
  }
  async function openSave(body: any, unguarded = false) {
    if (!unguarded) {
      guard(() => void openSave(body, true));
      return;
    }
    if (operation.current) return;
    operation.current = true;
    setBusy("Opening…");
    setError("");
    try {
      boxRequest.current?.abort();
      selectionRequest.current?.abort();
      validationRequest.current?.abort();
      revision.current++;
      const previousId = session?.id;
      const s = await api<Session>("/sessions", body);
      if (previousId)
        await api(
          `/sessions/${previousId}`,
          undefined,
          undefined,
          "DELETE",
        ).catch(() => {});
      acceptSession(s);
      void api(
        "/preferences",
        { language: localeRef.current, lastSavePath: s.summary.path },
        undefined,
        "PUT",
      ).catch(() => {});
    } catch (e) {
      fail(e);
    } finally {
      operation.current = false;
      setBusy("");
    }
  }
  async function discover() {
    setBusy("Loading…");
    setError("");
    try {
      setCandidates(await api<any[]>("/discovery", {}));
      setOpenPath(true);
    } catch (e) {
      fail(e);
    } finally {
      setBusy("");
    }
  }
  useEffect(() => {
    if (!session || !selection || creating) return;
    selectionRequest.current?.abort();
    const ac = new AbortController();
    selectionRequest.current = ac;
    loadedKey.current = "";
    setDetails(null);
    setPreview(null);
    api<Details>(
      `/sessions/${session.id}/pokemon/${selection.party ? "party" : "box"}/${selection.slot}`,
      undefined,
      ac.signal,
    )
      .then((d) => {
        if (
          ac.signal.aborted ||
          sessionId.current !== session.id ||
          sessionHash.current !== d.hash
        )
          return;
        if (d.hash !== session.hash) throw new ApiError(409, "stale_hash", "");
        loadedKey.current = keyOf(selection.party, selection.slot);
        setDetails(d);
        setAlphaChoice(
          (drafts[keyOf(selection.party, selection.slot)]?.alphaMode ||
            d.values.alphaMode ||
            "Compatible") as AlphaMode,
        );
      })
      .catch(fail);
    return () => ac.abort();
  }, [session?.id, selection?.party, selection?.slot, creating]);
  useEffect(() => {
    if (!num("species")) return;
    metadataRequest.current?.abort();
    const ac = new AbortController();
    metadataRequest.current = ac;
    api<any>(
      `/catalogs/species/${num("species")}/forms/${num("form")}`,
      undefined,
      ac.signal,
    )
      .then(setMetadata)
      .catch(fail);
    return () => ac.abort();
  }, [values.species, values.form]);
  function update(changes: Patch) {
    revision.current++;
    validationRequest.current?.abort();
    setValidating(false);
    setPreview(null);
    setAlphaPlan(null);
    setAlphaPlanning(false);
    setError("");
    if (creating) {
      setNewPatch((p) => ({ ...p, ...changes }));
      return;
    }
    if (!details || loadedKey.current !== activeKey) return;
    setDrafts((all) => {
      const merged = { ...(all[activeKey] || {}), ...changes };
      const effectiveAlpha = merged.isAlpha ?? details.values.isAlpha;
      if (
        effectiveAlpha === true &&
        merged.alphaMode === undefined &&
        details.values.alphaMode === "Custom"
      )
        merged.alphaMode = "Custom";
      for (const k in merged)
        if (
          same(merged[k], details.values[k]) &&
          !(k === "alphaMode" && effectiveAlpha === true && merged[k] != null)
        )
          delete merged[k];
      if (
        Object.keys(merged).length === 1 &&
        Object.hasOwn(merged, "alphaMode") &&
        same(merged.alphaMode, details.values.alphaMode)
      )
        delete merged.alphaMode;
      const next = { ...all };
      if (Object.keys(merged).length) next[activeKey] = merged;
      else delete next[activeKey];
      return next;
    });
  }
  function set(name: string, value: any) {
    if (name === "statNature") {
      if (value == null) mintIntent.current.delete(editContext);
      else mintIntent.current.add(editContext);
    }
    if (name === "nature") {
      const change = natureChange(
        values,
        Number(value),
        mintIntent.current.has(editContext),
        creating && newPatch.statNature == null,
      );
      if (change.resetMint) {
        mintIntent.current.delete(editContext);
        setMessage(
          t(
            "The previous neutral effective nature is unavailable as a Mint here; effective nature now follows the original.",
          ),
        );
      }
      update(change.patch);
      return;
    }
    if (name === "species") {
      if (creating) void chooseSpecies(Number(value));
      else void chooseSpecies(Number(value));
      return;
    }
    if (name === "form") {
      void chooseForm(Number(value));
      return;
    }
    update({ [name]: value });
  }
  async function chooseForm(form: number) {
    if (operation.current) return;
    clearValidationContext();
    metadataRequest.current?.abort();
    const ac = new AbortController();
    speciesRequest.current = ac;
    const expectedRevision = revision.current;
    operation.current = true;
    setBusy("Loading…");
    setError("");
    try {
      const metadata = await api<any>(
        `/catalogs/species/${num("species")}/forms/${form}`,
        undefined,
        ac.signal,
      );
      if (ac.signal.aborted || revision.current !== expectedRevision) return;
      const gender = metadata.genders.some(
        (g: Choice) => g.id === num("gender"),
      )
        ? num("gender")
        : (metadata.genders[0]?.id ?? null);
      update({
        form,
        gender,
        ...(creating ? { moves: null, plusMoves: null } : {}),
      });
      setMetadata(metadata);
    } catch (e) {
      fail(e);
    } finally {
      operation.current = false;
      setBusy("");
    }
  }
  async function chooseSpecies(id: number): Promise<boolean> {
    if (operation.current) return false;
    clearValidationContext();
    metadataRequest.current?.abort();
    const ac = new AbortController();
    speciesRequest.current = ac;
    const expectedRevision = revision.current;
    operation.current = true;
    setBusy("Loading…");
    setError("");
    try {
      const forms = await api<Choice[]>(
        `/catalogs/species/${id}/forms`,
        undefined,
        ac.signal,
      );
      if (!forms.length) throw new Error(t("Choose a valid catalog value."));
      const form = forms[0].id;
      const metadata = await api<any>(
        `/catalogs/species/${id}/forms/${form}`,
        undefined,
        ac.signal,
      );
      if (ac.signal.aborted || revision.current !== expectedRevision)
        return false;
      update(
        creating
          ? speciesCreationChanges(id, form, metadata.genders[0]?.id ?? null)
          : { species: id, form, gender: metadata.genders[0]?.id ?? null },
      );
      setMetadata(metadata);
      setSpeciesOpen(false);
      setTab("Profile");
      requestAnimationFrame(() =>
        root.current?.querySelector(".inspector")?.scrollTo({ top: 0 }),
      );
      return true;
    } catch (e) {
      fail(e);
      return false;
    } finally {
      operation.current = false;
      setBusy("");
    }
  }
  async function loadBox(index: number) {
    if (!session || index < 0 || index >= session.summary.boxCount) return;
    boxRequest.current?.abort();
    const ac = new AbortController();
    boxRequest.current = ac;
    try {
      const b = await api<any>(
        `/sessions/${session.id}/boxes/${index}`,
        undefined,
        ac.signal,
      );
      if (
        sessionId.current !== session.id ||
        ac.signal.aborted ||
        sessionHash.current !== b.hash
      )
        return;
      if (b.hash !== session.hash) throw new ApiError(409, "stale_hash", "");
      setSession((s) =>
        s && s.id === session.id && s.hash === b.hash
          ? { ...s, box: b.entries, boxIndex: index }
          : s,
      );
    } catch (e) {
      fail(e);
    }
  }
  function select(party: boolean, slot: number) {
    clearValidationContext();
    setAlphaChoice(
      (drafts[keyOf(party, slot)]?.alphaMode as AlphaMode) || "Compatible",
    );
    if (creating) {
      guard(() => {
        setCreating(false);
        setSelection({ party, slot });
      });
      return;
    }
    setSelection({ party, slot });
    if (matchMedia("(max-width: 900px)").matches) setBoxesOpen(false);
  }
  function clearValidationContext() {
    revision.current++;
    validationRequest.current?.abort();
    speciesRequest.current?.abort();
    setValidating(false);
    setPreview(null);
    setAlphaPlan(null);
    setAlphaPlanning(false);
  }
  function cancelCreation() {
    clearValidationContext();
    setCreating(false);
  }
  function beginAdd() {
    if (draftCount) {
      setError(t("Resolve pending drafts before adding."));
      return;
    }
    clearValidationContext();
    setNewPatch({ ...initial });
    setCreationSeed(null);
    delete alphaUndo.current.new;
    mintIntent.current.delete("new");
    setAlphaChoice("Compatible");
    setCreating(true);
    setSpeciesOpen(true);
    requestAnimationFrame(() =>
      root.current?.querySelector(".inspector")?.scrollTo({ top: 0 }),
    );
    setTab("Profile");
    setPreview(null);
    setError("");
    setDestination("Auto");
    setPreferredBox(-1);
  }
  async function requestAlphaPlan(mode: AlphaMode = alphaChoice) {
    if (!session || operation.current) return;
    clearValidationContext();
    setAlphaChoice(mode);
    setAlphaPlanning(true);
    setError("");
    const ac = new AbortController();
    validationRequest.current = ac;
    const expectedRevision = revision.current;
    try {
      const plan = await api<AlphaPlan>(
        `/sessions/${session.id}/${creating ? "alpha-plan-add" : "alpha-plan"}`,
        creating
          ? { expectedHash: session.hash, options: options(), mode }
          : {
              expectedHash: session.hash,
              party: selection?.party,
              slot: selection?.slot,
              patch,
              mode,
            },
        ac.signal,
      );
      if (ac.signal.aborted || revision.current !== expectedRevision) return;
      if (plan.hash !== session.hash) throw new ApiError(409, "stale_hash", "");
      setAlphaPlan(plan);
      setMessage(t("Review the Alpha plan. No draft or save was changed."));
    } catch (e) {
      if (ac.signal.aborted || revision.current !== expectedRevision) return;
      fail(e);
    } finally {
      if (revision.current === expectedRevision) setAlphaPlanning(false);
    }
  }
  function applyAlphaPlan() {
    if (!alphaPlan?.available || alphaPlan.hash !== session?.hash) return;
    const planned = planDraft(alphaPlan);
    alphaUndo.current[editContext] = {
      before: { ...patch },
      applied: planned,
      baseline: creating ? {} : { ...(details?.values || {}) },
      creationSeed: creating ? creationSeed : undefined,
    };
    const preview = alphaPlan.preview,
      seed = alphaPlan.options?.creationSeed;
    update(planned);
    if (creating && seed != null) setCreationSeed(String(seed));
    setPreview(preview);
    setMessage(
      t(
        alphaPlan.mode === "Custom"
          ? "Custom Alpha applied to draft; review the actual legality report before saving."
          : "Compatible Alpha plan applied to draft. Save with backup when ready.",
      ),
    );
    setMotion((n) => n + 1);
  }
  function revertAlphaPlan() {
    const undo = alphaUndo.current[editContext];
    if (!undo) return;
    delete alphaUndo.current[editContext];
    update(undoAlphaChanges(values, undo));
    if (creating) setCreationSeed(undo.creationSeed ?? null);
    setMessage(t("Alpha adjustments reverted; your other edits were kept."));
  }
  async function alpha(enabled: boolean) {
    if (enabled) {
      await requestAlphaPlan();
      return;
    }
    if (alphaPlan || alphaPlanning) {
      clearValidationContext();
      setMessage(t("Alpha plan cancelled. The draft was not changed."));
      return;
    }
    update({ isAlpha: false, alphaMode: null });
    setMessage(
      t(
        "Alpha disabled. Other adjustments remain; use Revert Alpha adjustments to restore the planned fields.",
      ),
    );
  }

  function scale(n: number) {
    const home =
      !!details && String(details.readOnly.tracker) !== "0" && !creating;
    update({
      scale: n,
      heightScalar: home ? n : 0,
      weightScalar: home ? n : 0,
    });
    setMotion((n) => n + 1);
  }
  function statChange(name: string, index: number, n: number) {
    const values = [
      ...(val(
        name,
        name === "iVs" ? [31, 31, 31, 31, 31, 31] : [0, 0, 0, 0, 0, 0],
      ) as number[]),
    ];
    values[index] = n;
    if (
      name === "iVs" &&
      val("isAlpha", false) &&
      values.filter((v) => v === 31).length < 3
    ) {
      setError(
        t(
          "Alpha needs at least three perfect IVs. Keep three IVs at 31 before reducing another.",
        ),
      );
      return;
    }
    if (name === "iVs" && n === 31) {
      const flags = [
        ...(val("hyperTrained", [
          false,
          false,
          false,
          false,
          false,
          false,
        ]) as boolean[]),
      ];
      flags[index] = false;
      update({ [name]: values, hyperTrained: flags });
    } else set(name, values);
  }
  const options = () => ({
    species: num("species", 4),
    form: num("form"),
    level: num("level", 10),
    nature: num("nature", 15),
    shiny: ["Star", "Square"].includes(String(val("shiny", "NonShiny"))),
    ball: num("ball", 4),
    destination,
    preferredBox,
    ...(creationSeed ? { creationSeed } : {}),
    details: newPatch,
  });
  async function validate() {
    if (!session) return;
    validationRequest.current?.abort();
    const ac = new AbortController();
    validationRequest.current = ac;
    const expectedRevision = revision.current;
    setValidating(true);
    setError("");
    try {
      const p = await api<Details>(
        `/sessions/${session.id}/${creating ? "preview-add" : "preview"}`,
        creating
          ? { expectedHash: session.hash, options: options() }
          : {
              expectedHash: session.hash,
              party: selection?.party,
              slot: selection?.slot,
              patch,
            },
        ac.signal,
      );
      if (expectedRevision !== revision.current) return;
      setPreview(p);
      setMessage(t("Validated by engine."));
    } catch (e) {
      if (expectedRevision !== revision.current || ac.signal.aborted) return;
      fail(e);
    } finally {
      if (expectedRevision === revision.current) setValidating(false);
    }
  }
  async function commit() {
    if (!session || operation.current || conflict) return;
    if (alphaPlan || alphaPlanning) {
      setError(t("Apply or cancel the Alpha plan before saving."));
      return;
    }
    if (creating && draftCount) {
      setError(t("Resolve pending drafts before adding."));
      return;
    }
    clearValidationContext();
    boxRequest.current?.abort();
    operation.current = true;
    setBusy("Saving…");
    setError("");
    let writeConfirmed = false;
    const oldBox = session.boxIndex;
    try {
      const edits = (party: boolean) =>
        Object.entries(drafts)
          .filter(([key]) => key.startsWith(party ? "party:" : "box:"))
          .map(([key, details]) => ({
            slot: Number(key.split(":")[1]),
            details,
          }));
      const result = await api<any>(
        `/sessions/${session.id}/${creating ? "add" : "edits"}`,
        creating
          ? {
              expectedHash: session.hash,
              operationId: crypto.randomUUID(),
              options: options(),
            }
          : {
              expectedHash: session.hash,
              operationId: crypto.randomUUID(),
              teamEdits: edits(true),
              boxEdits: edits(false),
            },
      );
      if (result.backupDirectory) setBackup(result.backupDirectory);
      if (!result.committed) {
        setConflict(true);
        setError(
          t(
            result.outcomeUnknown
              ? "The write outcome is unknown. Reopen the save to reconcile; drafts have been kept."
              : "The write did not complete. Reopen the save and inspect the backup; drafts have been kept.",
          ),
        );
        return;
      }
      writeConfirmed = true;
      sessionHash.current = result.hash;
      setBackup(result.backupDirectory);
      setDrafts({});
      alphaUndo.current = {};
      mintIntent.current.clear();
      setCreating(false);
      setMessage(t("Saved with backup."));
      let refreshed = await api<Session>(`/sessions/${session.id}`);
      let target = selection;
      if (creating && result.result?.pokemon) {
        const party = refreshed.team.length > session.team.length;
        target = { party, slot: result.result.pokemon.index };
      }
      const nextBox =
        target && !target.party ? Math.floor(target.slot / 30) : oldBox;
      if (nextBox > 0 && nextBox < refreshed.summary.boxCount) {
        const box = await api<any>(`/sessions/${session.id}/boxes/${nextBox}`);
        if (box.hash !== refreshed.hash)
          throw new ApiError(409, "stale_hash", "");
        refreshed = { ...refreshed, box: box.entries, boxIndex: nextBox };
      }
      setSession(refreshed);
      setSelection(target);
      if (target) {
        const d = await api<Details>(
          `/sessions/${session.id}/pokemon/${target.party ? "party" : "box"}/${target.slot}`,
        );
        setDetails(d);
        loadedKey.current = keyOf(target.party, target.slot);
      }
      setPreview(null);
      if (result.result?.warnings?.length)
        setMessage(
          t("Saved with backup.") + " " + result.result.warnings.join(" "),
        );
    } catch (e) {
      if (writeConfirmed) {
        setConflict(true);
        setError(
          t(
            "Saved with backup, but refresh failed. Reopen the save before editing.",
          ),
        );
      } else if (!(e instanceof ApiError)) {
        setConflict(true);
        setError(
          t(
            "The write outcome is unknown. Reopen the save to reconcile; drafts have been kept.",
          ),
        );
      } else fail(e);
    } finally {
      operation.current = false;
      setBusy("");
    }
  }
  async function shutdown() {
    if (operation.current) return;
    guard(() => {
      operation.current = true;
      setBusy("Stopping…");
      void api("/shutdown", {})
        .then(() => {
          setSession(null);
          setDetails(null);
          setSelection(null);
          setCreating(false);
          setMessage(
            t(
              "Editor stopped. Close this tab or launch the portable EXE again.",
            ),
          );
        })
        .catch(fail)
        .finally(() => {
          operation.current = false;
          setBusy("");
        });
    });
  }
  function openApi() {
    guard(() => location.assign("/api-docs"));
  }
  async function optimize() {
    if (!session) return;
    setBusy("Loading…");
    setError("");
    try {
      const team = await Promise.all(
        session.team.map(async (p) => {
          const d = drafts[keyOf(true, p.slot)] || {};
          const species = Number(d.species ?? p.species),
            form = Number(d.form ?? p.form);
          const metadata =
            d.species != null || d.form != null
              ? await api<any>(`/catalogs/species/${species}/forms/${form}`)
              : null;
          return {
            slot: p.slot,
            name: p.name,
            baseStats: metadata?.baseStats ?? p.baseStats,
            moveCategories: p.moveCategories,
            species: Number(d.species ?? p.species),
            form: Number(d.form ?? p.form),
            level: Number(d.level ?? p.level),
            moves: d.moves ?? p.moves,
            nature: d.statNature ?? d.nature ?? p.nature,
            shiny: d.shiny ? d.shiny !== "NonShiny" : p.shiny,
          };
        }),
      );
      const suggestions = await api<any[]>(
        `/sessions/${session.id}/suggestions`,
        { expectedHash: session.hash, team, language: locale },
      );
      setDrafts((all) => {
        const next = { ...all };
        for (const s of suggestions) {
          const key = keyOf(true, s.slot);
          next[key] = { ...next[key], nature: s.nature, statNature: s.nature };
        }
        return next;
      });
      setSuggestionRows(
        suggestions.map((s) => {
          const pokemon = team.find((p) => p.slot === s.slot);
          return {
            slot: s.slot,
            name:
              catalogs.species?.find((c) => c.id === pokemon?.species)?.name ||
              pokemon?.name ||
              "#" + s.slot,
            nature: english(
              catalogs.natures?.find((c) => c.id === s.nature)?.name ||
                "#" + s.nature,
            ),
            reason: s.reason,
          };
        }),
      );
      setMessage(
        suggestions.length + " " + t("Nature suggestions added to drafts."),
      );
    } catch (e) {
      fail(e);
    } finally {
      setBusy("");
    }
  }
  const choiceName = (list: string, id: number) =>
    catalogs[list]?.find((c) => c.id === id)?.name || "#" + id;
  const display = (p: Pokemon, party: boolean) => {
    const d = drafts[keyOf(party, p.slot)] || {};
    return {
      ...p,
      ...d,
      name: d.species ? choiceName("species", Number(d.species)) : p.name,
      shiny: d.shiny ? d.shiny !== "NonShiny" : p.shiny,
    };
  };
  function card(p: Pokemon, party: boolean) {
    const d = display(p, party);
    const chosen =
      !creating && selection?.party === party && selection.slot === p.slot;
    return (
      <button
        key={p.slot}
        className={
          "pokemon-card " +
          (party ? "party-card" : "box-card") +
          (chosen ? " selected" : "") +
          (drafts[keyOf(party, p.slot)] ? " dirty" : "")
        }
        onClick={() => select(party, p.slot)}
        aria-label={d.name + ", " + t("Level") + " " + d.level}
        aria-pressed={chosen}
        title={d.name + " · " + t("Level") + " " + d.level}
      >
        <Sprite species={d.species} form={d.form} shiny={d.shiny} />
        <span className="pokemon-copy">
          <strong>{d.name}</strong>
          <small>Lv. {d.level}</small>
        </span>
        {d.shiny && <Sparkles className="shiny-mark" size={14} />}
      </button>
    );
  }
  function choices(
    name: string,
    label: string,
    list: Choice[],
    fallback = 0,
    englishNames = false,
  ) {
    return (
      <label className="field">
        <span>{t(label)}</span>
        <select
          value={num(name, fallback)}
          onChange={(e) => set(name, Number(e.target.value))}
        >
          {!list.some((c) => c.id === num(name, fallback)) && (
            <option value={num(name, fallback)}>#{num(name, fallback)}</option>
          )}
          {list.map((c) => (
            <option key={c.id} value={c.id}>
              {englishNames ? english(c.name) : c.name}
            </option>
          ))}
        </select>
      </label>
    );
  }
  function number(
    name: string,
    label: string,
    min: number,
    max: number,
    fallback = 0,
  ) {
    return (
      <label className="field">
        <span>{t(label)}</span>
        <input
          aria-label={t(label)}
          type="number"
          min={min}
          max={max}
          value={num(name, fallback)}
          onChange={(e) => {
            if (e.target.value !== "") set(name, Number(e.target.value));
          }}
        />
      </label>
    );
  }
  const hasPokemon = creating || !!details;
  return (
    <div className="app" ref={root}>
      <header className="glass topbar">
        <div className="brand">
          <img src="/assets/pokeball-mark.png" alt="" />
          <h1>
            ZA Recomp Editor{" "}
            {appVersion && <small className="app-version">v{appVersion}</small>}
          </h1>
        </div>
        <button className="save-picker" onClick={() => setOpenPath((v) => !v)}>
          <FolderOpen size={18} />
          <span>
            {t("Save")}:{" "}
            <strong>
              {session
                ? session.summary.path.split(/[\\/]/).slice(-3).join("/")
                : t("Choose a save")}
            </strong>
          </span>
          <ChevronDown size={15} />
        </button>
        <span className="trainer">
          ♙ {t("Trainer")}: <strong>{session?.summary.trainer || "—"}</strong>
        </span>
        <div className="languages" aria-label={t("Language")}>
          {(["pt-BR", "en", "es"] as Locale[]).map((l) => (
            <button
              key={l}
              className={locale === l ? "active" : ""}
              aria-pressed={locale === l}
              onClick={() => {
                setLocale(l);
                document.documentElement.lang = l;
                void api(
                  "/preferences",
                  { language: l, lastSavePath: session?.summary.path || null },
                  undefined,
                  "PUT",
                ).catch(fail);
                void api(
                  "/preferences",
                  { language: l, lastSavePath: session?.summary.path || null },
                  undefined,
                  "PUT",
                ).catch(fail);
              }}
            >
              {l === "pt-BR" ? "PT-BR" : l.toUpperCase()}
            </button>
          ))}
        </div>
        <button
          className="add-button"
          disabled={!session || !!busy}
          onClick={beginAdd}
        >
          <Plus size={19} />
          {t("Add Pokémon")}
        </button>
        <div className="save-meta">
          <span>
            <FolderOpen size={15} />
            {session?.summary.path || t("Local portable editor")}
          </span>
          <span className="backup-state">
            <RefreshCw size={15} />
            {t("Automatic backup on every write")}
          </span>
        </div>
      </header>
      {openPath && (
        <section className="glass path-panel">
          <h2>{t("Open a save")}</h2>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              void openSave({ path });
            }}
          >
            <label>
              {t("Save path")}
              <input
                value={path}
                onChange={(e) => setPath(e.target.value)}
                placeholder="C:\\…\\save\\save_0\\main"
                autoComplete="off"
                spellCheck={false}
              />
            </label>
            <button type="submit" disabled={!!busy || !path}>
              <FolderOpen size={16} />
              {t("Open")}
            </button>
            <button type="button" onClick={discover} disabled={!!busy}>
              <Search size={16} />
              {t("Find saves")}
            </button>
            <button
              type="button"
              className="icon-button"
              aria-label={t("Keep editing")}
              onClick={() => setOpenPath(false)}
            >
              <X size={18} />
            </button>
          </form>
          {candidates.length > 0 && (
            <div className="candidates">
              {candidates.map((c) => (
                <button
                  key={c.id}
                  onClick={() => void openSave({ candidateId: c.id })}
                >
                  <FolderOpen size={15} />
                  {c.label}
                  <small>{c.path}</small>
                </button>
              ))}
            </div>
          )}
          <small>{t("Personal data stays on this computer.")}</small>
        </section>
      )}
      {pending && (
        <div className="glass inline-confirm">
          <TriangleAlert size={19} />
          <span>{t("Discard unsaved changes?")}</span>
          <button onClick={() => setPending(null)}>{t("Keep editing")}</button>
          <button
            onClick={() => {
              setDrafts({});
              setCreating(false);
              const action = pending;
              setPending(null);
              action();
            }}
          >
            {t("Discard and continue")}
          </button>
        </div>
      )}
      <fieldset className="workspace-lock" disabled={!!busy}>
        <div className="workbench">
          <aside className="glass party">
            <h2>
              {t("Team")} <span>· {session?.team.length || 0}/6</span>
            </h2>
            <div className="party-list">
              {session?.team.map((p) => card(p, true))}
              {Array.from(
                { length: Math.max(0, 6 - (session?.team.length || 0)) },
                (_, i) => (
                  <button
                    key={"empty" + i}
                    className="pokemon-card party-card empty"
                    disabled={!session || !!busy}
                    onClick={beginAdd}
                    aria-label={t("Add Pokémon")}
                  >
                    <Plus size={24} />
                    <span>{t("Empty slot")}</span>
                  </button>
                ),
              )}
            </div>
            <button
              className="optimize"
              disabled={!session || !!busy}
              onClick={optimize}
            >
              <Sparkles size={19} />
              <span>
                <strong>{t("Optimize team")}</strong>
                <small>{t("Suggest natures")}</small>
              </span>
            </button>
          </aside>
          <section className={"glass storage " + (boxesOpen ? "expanded" : "")}>
            <div className="storage-heading">
              <h2>
                {t("Box")} {session ? session.boxIndex + 1 : "1"}
              </h2>
              <div className="box-navigation">
                <button
                  className="icon-button"
                  aria-label={t("Box") + " −1"}
                  disabled={!session || session.boxIndex === 0}
                  onClick={() => void loadBox((session?.boxIndex || 0) - 1)}
                >
                  <ChevronLeft size={20} />
                </button>
                <select
                  aria-label={t("Box")}
                  value={session?.boxIndex || 0}
                  onChange={(e) => void loadBox(Number(e.target.value))}
                >
                  {Array.from(
                    { length: session?.summary.boxCount || 1 },
                    (_, i) => (
                      <option key={i} value={i}>
                        {t("Box")} {i + 1}
                      </option>
                    ),
                  )}
                </select>
                <button
                  className="icon-button"
                  aria-label={t("Box") + " +1"}
                  disabled={
                    !session || session.boxIndex >= session.summary.boxCount - 1
                  }
                  onClick={() => void loadBox((session?.boxIndex || 0) + 1)}
                >
                  <ChevronRight size={20} />
                </button>
              </div>
              <label className="search">
                <Search size={17} />
                <input
                  aria-label={t("Search Pokémon")}
                  placeholder={t("Search Pokémon")}
                  value={filter}
                  onChange={(e) => setFilter(e.target.value)}
                />
              </label>
              <button
                className="mobile-box-toggle"
                aria-expanded={boxesOpen}
                onClick={() => setBoxesOpen((v) => !v)}
              >
                <Box size={18} />
                {t(boxesOpen ? "Hide boxes" : "Show 30 slots")}
                <ChevronDown size={16} />
              </button>
            </div>
            <div className="box-content">
              <div className="box-grid">
                {Array.from({ length: 30 }, (_, i) => {
                  const slot = (session?.boxIndex || 0) * 30 + i;
                  const p = session?.box.find((p) => p.slot === slot);
                  if (p) {
                    if (
                      filter &&
                      !display(p, false)
                        .name.toLowerCase()
                        .includes(filter.toLowerCase())
                    )
                      return <div key={slot} className="filtered-slot" />;
                    return card(p, false);
                  }
                  return (
                    <button
                      key={slot}
                      className="pokemon-card box-card empty"
                      disabled={!session || !!busy}
                      onClick={() => {
                        beginAdd();
                        setDestination("Box");
                        setPreferredBox(session?.boxIndex || 0);
                      }}
                      aria-label={t("Empty slot") + " " + (i + 1)}
                    >
                      <span className="ball-placeholder">◓</span>
                    </button>
                  );
                })}
              </div>
              <div className="storage-note">
                <span>
                  {session?.summary.emptyBoxSlots || 0}{" "}
                  {t("Empty slot").toLowerCase()}
                </span>
                <span>
                  {draftCount} {t("Pokémon with changes").toLowerCase()}
                </span>
              </div>
            </div>
          </section>
          <section
            className="glass inspector"
            aria-busy={!!busy || (!!selection && !details && !creating)}
          >
            {!hasPokemon ? (
              <div className="empty-inspector">
                <SlidersHorizontal size={42} />
                <h2>
                  {t(session ? "Select a Pokémon" : "Choose a save to begin.")}
                </h2>
                <p>
                  {busy
                    ? t(busy)
                    : session
                      ? t("Loading…")
                      : t("Open the portable EXE to start a local session.")}
                </p>
                {!session && (
                  <button onClick={() => setOpenPath(true)}>
                    <FolderOpen size={16} />
                    {t("Choose a save")}
                  </button>
                )}
              </div>
            ) : (
              <>
                {creating && (
                  <div className="creation-banner">
                    <strong>
                      <Plus size={18} />
                      {t("New Pokémon")}
                    </strong>
                    <button
                      className="icon-button"
                      aria-label={t("Cancel creation")}
                      onClick={cancelCreation}
                    >
                      <X size={18} />
                    </button>
                  </div>
                )}
                {creating && (
                  <SpeciesPicker
                    catalog={catalogs.species || []}
                    value={num("species", 4)}
                    open={speciesOpen}
                    setOpen={setSpeciesOpen}
                    onChoose={chooseSpecies}
                    renderSprite={(id) => <Sprite species={id} lazy />}
                    t={t}
                  />
                )}
                {creating && (
                  <div className="creation-placement">
                    <label className="field">
                      <span>{t("Destination")}</span>
                      <select
                        value={destination}
                        onChange={(e) => setDestination(e.target.value)}
                      >
                        <option value="Auto">
                          {t("Automatic: team, then boxes")}
                        </option>
                        <option value="Party">{t("Team")}</option>
                        <option value="Box">{t("Boxes")}</option>
                      </select>
                    </label>
                    {destination !== "Party" && (
                      <label className="field">
                        <span>{t("Box")}</span>
                        <select
                          value={preferredBox}
                          onChange={(e) =>
                            setPreferredBox(Number(e.target.value))
                          }
                        >
                          <option value={-1}>{t("First available")}</option>
                          {Array.from(
                            { length: session?.summary.boxCount || 0 },
                            (_, i) => (
                              <option key={i} value={i}>
                                {t("Box")} {i + 1}
                              </option>
                            ),
                          )}
                        </select>
                      </label>
                    )}
                  </div>
                )}
                <div className="inspector-summary">
                  <div className="preview-art">
                    <Sprite
                      species={num("species")}
                      form={num("form")}
                      shiny={val("shiny", "NonShiny") !== "NonShiny"}
                    />
                    {val("shiny", "NonShiny") !== "NonShiny" && (
                      <Sparkles className="preview-spark" size={28} />
                    )}
                  </div>
                  <div className="quick-fields">
                    <div className="name-row">
                      <div>
                        <h2>{choiceName("species", num("species"))}</h2>
                        <small>
                          #{String(num("species")).padStart(4, "0")} ·{" "}
                          {creating
                            ? t("New Pokémon")
                            : selection?.party
                              ? t("Team")
                              : t("Box") +
                                " " +
                                (Math.floor((selection?.slot || 0) / 30) + 1)}
                        </small>
                      </div>
                      <label
                        className={
                          "shiny-toggle " +
                          (val("shiny", "NonShiny") !== "NonShiny" ? "on" : "")
                        }
                      >
                        <Sparkles size={15} />
                        Shiny
                        <input
                          type="checkbox"
                          checked={val("shiny", "NonShiny") !== "NonShiny"}
                          onChange={(e) =>
                            set("shiny", e.target.checked ? "Star" : "NonShiny")
                          }
                        />
                      </label>
                    </div>
                    {number("level", "Level", 1, 100, 10)}
                    <NatureChoice
                      label="Nature"
                      value={num("nature", 15)}
                      choices={catalogs.natures || []}
                      onChange={(n) => set("nature", n)}
                      t={t}
                    />
                    <NatureChoice
                      label="Mint"
                      value={num("statNature", num("nature", 15))}
                      choices={mintChoices(
                        catalogs.natures || [],
                        num("nature", 15),
                      )}
                      onChange={(n) => set("statNature", n)}
                      t={t}
                    />
                    {creating && (
                      <button
                        className="mint-auto auto-button"
                        onClick={() =>
                          set(
                            "statNature",
                            newPatch.statNature == null
                              ? num("nature", 15)
                              : null,
                          )
                        }
                      >
                        {t(
                          newPatch.statNature == null
                            ? "Automatic"
                            : "Customize",
                        )}{" "}
                        · {t("Mint")}
                      </button>
                    )}
                    <BallPicker
                      value={num("ball", 4)}
                      choices={catalogs.balls || []}
                      onChange={(n) => set("ball", n)}
                      locale={locale}
                    />
                  </div>
                </div>
                <div className="nickname-row">
                  <label className="field">
                    <span>{t("Nickname")}</span>
                    <input
                      maxLength={12}
                      value={String(val("nickname"))}
                      onChange={(e) =>
                        update({
                          nickname: e.target.value,
                          isNicknamed: !!e.target.value,
                        })
                      }
                    />
                  </label>
                  <label className="toggle-label">
                    {t("Favorite")}
                    <input
                      type="checkbox"
                      checked={!!val("favorite", false)}
                      onChange={(e) => set("favorite", e.target.checked)}
                    />
                  </label>
                </div>
                <nav className="inspector-tabs" aria-label={t("Profile")}>
                  {["Profile", "Attributes", "Moves", "Origin", "Advanced"].map(
                    (name) => (
                      <button
                        key={name}
                        className={tab === name ? "active" : ""}
                        onClick={() => setTab(name)}
                      >
                        {t(name)}
                      </button>
                    ),
                  )}
                </nav>
                <div className="inspector-body">
                  <SecondaryFields
                    tab={tab}
                    values={values}
                    patch={patch}
                    creating={creating}
                    catalogs={catalogs}
                    metadata={metadata}
                    details={details}
                    t={t}
                    set={set}
                    update={update}
                    scale={scale}
                    error={error}
                    shutdown={shutdown}
                    openApi={openApi}
                  />
                  {tab === "Attributes" && (
                    <>
                      {creating && (
                        <div className="automatic-stats">
                          <p>
                            {t(
                              "Unchecked fields use generated values. Edit a value or enable customization to override it.",
                            )}
                          </p>
                          {["iVs", "eVs", "hyperTrained"].map((name, i) => (
                            <label key={name}>
                              <input
                                type="checkbox"
                                checked={newPatch[name] != null}
                                disabled={
                                  name === "iVs" && !!val("isAlpha", false)
                                }
                                onChange={(e) =>
                                  set(
                                    name,
                                    e.target.checked
                                      ? val(
                                          name,
                                          name === "iVs"
                                            ? [31, 31, 31, 31, 31, 31]
                                            : name === "eVs"
                                              ? [0, 0, 0, 0, 0, 0]
                                              : [
                                                  false,
                                                  false,
                                                  false,
                                                  false,
                                                  false,
                                                  false,
                                                ],
                                        )
                                      : null,
                                  )
                                }
                              />
                              {["IVs", "EVs", "Hyper Training"][i]}
                            </label>
                          ))}
                        </div>
                      )}
                      <table className="stats-table">
                        <thead>
                          <tr>
                            <th>{t("Stat")}</th>
                            <th>IV</th>
                            <th>EV</th>
                            <th title="Hyper Training">HT</th>
                          </tr>
                        </thead>
                        <tbody>
                          {stats.map((name, i) => (
                            <tr key={name} className="changed-field">
                              <th>{t(name)}</th>
                              <td>
                                <input
                                  aria-label={"IV " + t(name)}
                                  type="number"
                                  min={0}
                                  max={31}
                                  value={
                                    (
                                      val(
                                        "iVs",
                                        [31, 31, 31, 31, 31, 31],
                                      ) as number[]
                                    )[i]
                                  }
                                  onChange={(e) =>
                                    statChange("iVs", i, Number(e.target.value))
                                  }
                                />
                              </td>
                              <td>
                                <input
                                  aria-label={"EV " + t(name)}
                                  type="number"
                                  min={0}
                                  max={252}
                                  value={
                                    (
                                      val("eVs", [0, 0, 0, 0, 0, 0]) as number[]
                                    )[i]
                                  }
                                  onChange={(e) =>
                                    statChange("eVs", i, Number(e.target.value))
                                  }
                                />
                              </td>
                              <td>
                                <label className="ht-hit">
                                  <input
                                    aria-label={"Hyper Training " + t(name)}
                                    title={
                                      num("level") < 50
                                        ? t("Hyper Training requires level 50.")
                                        : "Hyper Training"
                                    }
                                    type="checkbox"
                                    disabled={
                                      (num("level") < 50 ||
                                        (
                                          val(
                                            "iVs",
                                            [31, 31, 31, 31, 31, 31],
                                          ) as number[]
                                        )[i] === 31) &&
                                      !(
                                        val("hyperTrained", [
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                        ]) as boolean[]
                                      )[i]
                                    }
                                    checked={
                                      (
                                        val("hyperTrained", [
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                        ]) as boolean[]
                                      )[i]
                                    }
                                    onChange={(e) => {
                                      const flags = [
                                        ...(val("hyperTrained", [
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                          false,
                                        ]) as boolean[]),
                                      ];
                                      flags[i] = e.target.checked;
                                      set("hyperTrained", flags);
                                    }}
                                  />
                                </label>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      <div className="iv-actions">
                        <div>
                          {[31, 0].map((n) => (
                            <button
                              key={n}
                              disabled={n === 0 && !!val("isAlpha", false)}
                              title={t(
                                n === 0 && val("isAlpha", false)
                                  ? "Alpha needs at least three perfect IVs. Keep three IVs at 31 before reducing another."
                                  : "IV presets clear Hyper Training.",
                              )}
                              onClick={() => {
                                update({
                                  iVs: Array(6).fill(n),
                                  hyperTrained: Array(6).fill(false),
                                });
                                setMotion((m) => m + 1);
                              }}
                            >
                              IVs {n}
                            </button>
                          ))}
                        </div>
                        <span
                          className={
                            (val("eVs", [0, 0, 0, 0, 0, 0]) as number[]).reduce(
                              (a, b) => a + b,
                              0,
                            ) > 510
                              ? "invalid"
                              : ""
                          }
                        >
                          EV:{" "}
                          {(val("eVs", [0, 0, 0, 0, 0, 0]) as number[]).reduce(
                            (a, b) => a + b,
                            0,
                          )}
                          /510
                        </span>
                      </div>
                    </>
                  )}
                  <div className="size-alpha changed-field">
                    <div className="alpha-control">
                      <label>
                        <strong>Alpha</strong>
                        <input
                          type="checkbox"
                          checked={
                            !!val("isAlpha", false) ||
                            !!alphaPlan ||
                            alphaPlanning
                          }
                          onChange={(e) => alpha(e.target.checked)}
                        />
                        <small>
                          {alphaPlanning
                            ? t("Loading…")
                            : val("isAlpha", false)
                              ? "ON"
                              : alphaPlan
                                ? t("Preview")
                                : "OFF"}
                        </small>
                      </label>
                      <label className="alpha-mode-label">
                        {t("Alpha mode")}
                        <select
                          aria-label={t("Alpha mode")}
                          value={alphaChoice}
                          onChange={(e) => {
                            const mode = e.target.value as AlphaMode;
                            setAlphaChoice(mode);
                            if (alphaPlan || val("isAlpha", false))
                              void requestAlphaPlan(mode);
                          }}
                        >
                          <option value="Compatible">
                            {t("Compatible encounter")}
                          </option>
                          <option value="Custom">
                            {t("Custom: keep encounter")}
                          </option>
                        </select>
                      </label>
                      <small>
                        <Info size={14} />
                        {t(
                          "Compatible previews encounter adjustments; Custom keeps origin and reports incompatibilities. Nothing is saved automatically.",
                        )}
                      </small>
                      {alphaUndo.current[editContext] && (
                        <button
                          className="alpha-revert"
                          onClick={revertAlphaPlan}
                        >
                          {t("Revert Alpha adjustments")}
                        </button>
                      )}
                    </div>
                    <div className="size-control">
                      <label>
                        {t("Scale")}
                        <input
                          aria-label={t("Scale")}
                          type="number"
                          min={0}
                          max={255}
                          disabled={!!val("isAlpha", false)}
                          value={num("scale", 128)}
                          onChange={(e) => scale(Number(e.target.value))}
                        />
                      </label>
                      <input
                        aria-label={t("Size")}
                        type="range"
                        min={0}
                        max={255}
                        disabled={!!val("isAlpha", false)}
                        value={num("scale", 128)}
                        onChange={(e) => scale(Number(e.target.value))}
                      />
                      <div>
                        <span>0</span>
                        <span>255</span>
                      </div>
                    </div>
                  </div>
                  {alphaPlan && (
                    <AlphaPlanReview
                      plan={alphaPlan}
                      catalogs={catalogs}
                      locale={locale}
                      t={t}
                      apply={applyAlphaPlan}
                      cancel={() => {
                        clearValidationContext();
                        setMessage(
                          t("Alpha plan cancelled. The draft was not changed."),
                        );
                      }}
                    />
                  )}
                  <div className="validation-bar">
                    <button
                      onClick={validate}
                      disabled={
                        validating ||
                        !!busy ||
                        conflict ||
                        alphaPlanning ||
                        !!alphaPlan
                      }
                    >
                      {validating ? (
                        <LoaderCircle size={16} className="spinning" />
                      ) : (
                        <Check size={16} />
                      )}{" "}
                      {t("Validate draft")}
                    </button>
                    {preview && (
                      <span className={preview.legal ? "good" : "warning"}>
                        {preview.legal ? "✓" : "!"} PKHeX
                      </span>
                    )}
                  </div>
                  {preview && (
                    <details className="report">
                      <summary>
                        <FileText size={16} />
                        {t("Technical data and report")}
                      </summary>
                      <pre>{preview.legalityReport}</pre>
                      {preview.warnings?.map((w, i) => (
                        <p key={i}>{w}</p>
                      ))}
                    </details>
                  )}
                </div>
              </>
            )}
          </section>
        </div>
      </fieldset>
      {(draftCount > 0 || creating) && (
        <p className="draft-warning">
          <Info size={14} />
          {t(
            "Reloading this page discards unsaved drafts. Save or discard them before leaving.",
          )}
        </p>
      )}
      <footer className="glass savebar">
        <div className="draft-summary">
          <SlidersHorizontal size={20} />
          <strong>
            {creating
              ? t("New Pokémon")
              : draftCount
                ? draftCount + " " + t("Pokémon with changes")
                : t("No pending changes")}
          </strong>
        </div>
        <span className="footer-hint">
          {t("Close the game before saving.")}
        </span>
        <div className="save-actions">
          <button
            disabled={!draftCount || !!busy}
            onClick={() =>
              setPending(() => () => {
                setDrafts({});
                setMessage(t("No pending changes"));
              })
            }
          >
            <Trash2 size={17} />
            <span>{t("Discard drafts")}</span>
          </button>
          <button
            className="primary"
            disabled={
              (!creating && !draftCount) ||
              !!busy ||
              conflict ||
              !session ||
              !!alphaPlan ||
              alphaPlanning
            }
            onClick={commit}
          >
            {busy ? (
              <LoaderCircle size={18} className="spinning" />
            ) : creating ? (
              <Plus size={18} />
            ) : (
              <Save size={18} />
            )}
            <span>
              {t(busy || (creating ? "Add with backup" : "Save with backup"))}
            </span>
          </button>
        </div>
      </footer>
      {(error || message || backup) && (
        <div
          className={"glass receipt " + (error ? "error" : "")}
          role={error ? "alert" : "status"}
        >
          {error ? <TriangleAlert size={17} /> : <Info size={17} />}
          <div className="receipt-copy">
            {error || message}
            {!error && suggestionRows.length > 0 && draftCount > 0 && (
              <details className="nature-suggestions">
                <summary>
                  {t("View nature suggestions")} ({suggestionRows.length})
                </summary>
                <ul>
                  {suggestionRows.map((row) => (
                    <li key={row.slot}>
                      <strong>
                        {row.name} <span aria-hidden="true">→</span>{" "}
                        {row.nature}
                      </strong>
                      <p>{row.reason}</p>
                    </li>
                  ))}
                </ul>
                <small>
                  {t("Suggestion only; no universally best nature.")}
                </small>
              </details>
            )}
            {backup && (
              <small>
                {t("Backup")}: <code>{backup}</code>
                <button
                  className="copy-backup"
                  onClick={() =>
                    void navigator.clipboard
                      .writeText(backup)
                      .then(() => setMessage(t("Copied")))
                      .catch(() =>
                        setError(
                          t(
                            "Copy failed. Select and copy the backup path manually.",
                          ),
                        ),
                      )
                  }
                >
                  {t("Copy path")}
                </button>
              </small>
            )}
          </div>
          {conflict && session && (
            <button
              onClick={() => void openSave({ path: session.summary.path })}
            >
              {t("Reopen save")}
            </button>
          )}
          <button
            className="icon-button"
            aria-label={t("Keep editing")}
            onClick={() => {
              setError("");
              setMessage("");
              setBackup("");
              setSuggestionRows([]);
            }}
          >
            <X size={16} />
          </button>
        </div>
      )}
    </div>
  );
}
createRoot(document.getElementById("root")!).render(<App />);
