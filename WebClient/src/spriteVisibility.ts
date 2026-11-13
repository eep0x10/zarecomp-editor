// A shared observer avoids one observer/request loop per catalog card.
let observer: IntersectionObserver | null = null;
const pending = new Map<Element, () => void>();
export function observeSpriteVisibility(element: Element, visible: () => void) {
  if (typeof IntersectionObserver === "undefined") {
    visible();
    return () => {};
  }
  if (!observer)
    observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          const callback = pending.get(entry.target);
          pending.delete(entry.target);
          observer?.unobserve(entry.target);
          callback?.();
        }
        if (!pending.size) {
          observer?.disconnect();
          observer = null;
        }
      },
      { rootMargin: "80px", threshold: 0.01 },
    );
  pending.set(element, visible);
  observer.observe(element);
  return () => {
    pending.delete(element);
    observer?.unobserve(element);
    if (!pending.size) {
      observer?.disconnect();
      observer = null;
    }
  };
}
