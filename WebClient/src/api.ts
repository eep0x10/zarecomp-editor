export type Choice = { id: number; name: string };
export type Pokemon = {
  slot: number;
  species: number;
  form: number;
  name: string;
  level: number;
  shiny: boolean;
  nature: number;
  moves: number[];
  baseStats: number[];
  moveCategories: number[];
};
export type Patch = Record<
  string,
  number | string | boolean | number[] | boolean[] | null
>;
export type Details = {
  slot: number;
  party: boolean;
  hash: string;
  values: Patch;
  readOnly: Record<string, any>;
  legal: boolean;
  legalityReport: string;
  warnings: string[];
  stats: number[];
};
export type Session = {
  id: string;
  summary: {
    path: string;
    trainer: string;
    partyCount: number;
    boxCount: number;
    emptyBoxSlots: number;
    hash: string;
  };
  team: Pokemon[];
  box: Pokemon[];
  boxIndex: number;
  hash: string;
};
export type Catalogs = Record<string, Choice[]>;
const fragment = new URLSearchParams(location.hash.slice(1));
const token = fragment.get("token") || "";
if (location.hash)
  history.replaceState(null, "", location.pathname + location.search);
export class ApiError extends Error {
  status: number;
  code: string;
  constructor(status: number, code: string, message: string) {
    super(message);
    this.status = status;
    this.code = code;
  }
}
export async function api<T>(
  path: string,
  body?: unknown,
  signal?: AbortSignal,
  method?: "PUT" | "DELETE",
): Promise<T> {
  const response = await fetch("/api/v1" + path, {
    method: method || (body === undefined ? "GET" : "POST"),
    headers: {
      ...(token ? { Authorization: "Bearer " + token } : {}),
      ...(body === undefined ? {} : { "Content-Type": "application/json" }),
    },
    body: body === undefined ? undefined : JSON.stringify(body),
    signal,
    cache: "no-store",
  });
  if (!response.ok) {
    const problem = await response.json().catch(() => ({}));
    throw new ApiError(
      response.status,
      problem.code || "",
      problem.detail || problem.title || response.statusText,
    );
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}
export const sprite = (species: number, form = 0, shiny = false) =>
  `/api/v1/sprites/${species}/${form}/${shiny ? "shiny" : "normal"}`;
// Images cannot attach bearer headers. Fetch them using the same authenticated client instead.
const images = new Map<string, Promise<string>>();
export function spriteUrl(
  species: number,
  form = 0,
  shiny = false,
): Promise<string> {
  const key = sprite(species, form, shiny);
  if (!images.has(key))
    images.set(
      key,
      fetch(key, { headers: { Authorization: "Bearer " + token } })
        .then((r) => {
          if (!r.ok) throw new Error("Sprite unavailable");
          return r.blob();
        })
        .then(URL.createObjectURL)
        .catch(() => ""),
    );
  return images.get(key)!;
}
window.addEventListener("pagehide", () => {
  images.forEach(async (p) => {
    const url = await p;
    if (url) URL.revokeObjectURL(url);
  });
  images.clear();
});
