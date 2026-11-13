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
// Shared authenticated image requests are cached only after a real decoded image.
const images = new Map<string, Promise<string>>();
const spriteListeners = new Map<string, Set<(url: string) => void>>();
export function subscribeSprite(
  species: number,
  form: number,
  shiny: boolean,
  listener: (url: string) => void,
) {
  const key = sprite(species, form, shiny);
  let listeners = spriteListeners.get(key);
  if (!listeners) spriteListeners.set(key, (listeners = new Set()));
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
    if (!listeners.size) spriteListeners.delete(key);
  };
}
export function invalidateSprite(species: number, form = 0, shiny = false) {
  const key = sprite(species, form, shiny);
  const entry = images.get(key);
  images.delete(key);
  entry?.then((url) => URL.revokeObjectURL(url)).catch(() => {});
}
export function spriteUrl(
  species: number,
  form = 0,
  shiny = false,
): Promise<string> {
  const key = sprite(species, form, shiny);
  const cached = images.get(key);
  if (cached) return cached;
  const request = fetch(key, {
    headers: token ? { Authorization: "Bearer " + token } : {},
    cache: "no-store",
  }).then(async (response) => {
    if (!response.ok) throw new Error("Sprite unavailable");
    const url = URL.createObjectURL(await response.blob());
    try {
      const decoded = new Image();
      decoded.src = url;
      await decoded.decode();
      if (decoded.naturalWidth <= 1 || decoded.naturalHeight <= 1)
        throw new Error("Empty sprite");
      return url;
    } catch (e) {
      URL.revokeObjectURL(url);
      throw e;
    }
  });
  images.set(key, request);
  request.then(
    (url) => {
      if (images.get(key) === request)
        spriteListeners.get(key)?.forEach((listener) => listener(url));
    },
    () => {
      if (images.get(key) === request) images.delete(key);
    },
  );
  return request;
}
window.addEventListener("pagehide", (event) => {
  if (event.persisted) return;
  images.forEach((p) => {
    p.then((url) => URL.revokeObjectURL(url)).catch(() => {});
  });
  images.clear();
});
