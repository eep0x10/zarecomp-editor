import type { Details, Patch } from "./api";
import type { AlphaMode } from "./alphaDraft";
export type OptimizeOptions = {
  bestNatures: boolean;
  alpha: boolean;
  maxIVs: boolean;
  shiny: boolean;
  maxSize: boolean;
  maxFriendship: boolean;
  level100: boolean;
};
export const optimizeDefaults: OptimizeOptions = {
  bestNatures: true,
  alpha: false,
  maxIVs: false,
  shiny: false,
  maxSize: false,
  maxFriendship: false,
  level100: false,
};
export const optimizeOptionLabels: Record<keyof OptimizeOptions, string> = {
  bestNatures: "Recommended nature (Mint)",
  alpha: "Alpha with at least 3 perfect IVs",
  maxIVs: "All IVs at 31",
  shiny: "All shiny",
  maxSize: "Maximum size",
  maxFriendship: "Maximum friendship (255)",
  level100: "Level 100",
};
export type TeamPlanEdit = { slot: number; details: Patch };
export type TeamPlanItem = {
  slot: number;
  name: string;
  species: number;
  form: number;
  available: boolean;
  before: Details | null;
  preview: Details | null;
  patch: Patch | null;
  changes: { field: string; before: string; after: string }[];
  warnings: string[];
  natureReason: string | null;
  nature: number | null;
  code: string;
};
export type TeamOptimizePlan = {
  available: boolean;
  code: string;
  hash: string;
  teamEdits: TeamPlanEdit[];
  items: TeamPlanItem[];
  warnings: string[];
  natures: { slot: number; nature: number; reason: string }[];
};
export type Drafts = Record<string, Patch>;
export function specifiedPatch(patch: Patch): Patch {
  return Object.fromEntries(
    Object.entries(patch).filter(([, value]) => value != null),
  ) as Patch;
}
export function partyEdits(drafts: Drafts): TeamPlanEdit[] {
  return Object.entries(drafts)
    .filter(([key]) => /^party:\d+$/.test(key))
    .map(([key, details]) => ({
      slot: Number(key.split(":")[1]),
      details: specifiedPatch(details),
    }))
    .sort((a, b) => a.slot - b.slot);
}
export function draftSignature(drafts: Drafts): string {
  return JSON.stringify(
    Object.entries(drafts)
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([key, patch]) => [
        key,
        Object.entries(patch).sort(([a], [b]) => a.localeCompare(b)),
      ]),
  );
}
export type TeamPlanSnapshot = {
  sessionId: string;
  hash: string;
  revision: number;
  signature: string;
  slots: number[];
  options: OptimizeOptions;
  alphaMode: AlphaMode;
};
export function applyTeamPlan(
  drafts: Drafts,
  plan: TeamOptimizePlan,
  snapshot: TeamPlanSnapshot,
  current: { sessionId: string; hash: string; revision: number },
): Drafts {
  if (
    current.sessionId !== snapshot.sessionId ||
    current.hash !== snapshot.hash ||
    plan.hash !== current.hash ||
    current.revision !== snapshot.revision ||
    draftSignature(drafts) !== snapshot.signature
  )
    throw new Error("stale_team_plan");
  const expected = new Set(snapshot.slots);
  const returned = new Set(plan.items.map((item) => item.slot));
  if (
    !plan.available ||
    plan.items.some((item) => !item.available) ||
    returned.size !== plan.items.length ||
    returned.size !== expected.size ||
    [...returned].some((slot) => !expected.has(slot))
  )
    throw new Error("unavailable_team_plan");
  const edits = new Set<number>();
  for (const edit of plan.teamEdits) {
    if (!expected.has(edit.slot) || edits.has(edit.slot) || !edit.details)
      throw new Error("invalid_team_plan");
    edits.add(edit.slot);
  }
  if (edits.size !== expected.size) throw new Error("invalid_team_plan");
  const next = { ...drafts };
  for (const edit of plan.teamEdits) {
    const key = "party:" + edit.slot;
    // Server returns the complete delta from disk, including an empty delta
    // when optimization restores a field to its saved value.
    const canonical = specifiedPatch(edit.details);
    if (Object.keys(canonical).length) next[key] = canonical;
    else delete next[key];
  }
  return next;
}
