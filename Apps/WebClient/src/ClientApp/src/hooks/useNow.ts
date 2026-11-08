import { useSyncExternalStore } from "react";

const listeners = new Set<() => void>();
let timer: ReturnType<typeof setInterval> | undefined;
let now = Date.now();

function subscribe(listener: () => void) {
    listeners.add(listener);
    if (!timer) {
        // the clock stood still while nothing listened: catch up before the first tick
        now = Date.now();
        timer = setInterval(() => {
            now = Date.now();
            listeners.forEach((l) => l());
        }, 1000);
    }
    return () => {
        listeners.delete(listener);
        if (listeners.size === 0 && timer) {
            clearInterval(timer);
            timer = undefined;
        }
    };
}

const noSubscribe = () => () => undefined;

/**
 * Current time in ms, re-rendering once per second (pure during render, unlike Date.now()).
 * `active = false` stops the ticking (e.g. a hidden dialog) without breaking the hook order.
 */
export function useNow(active = true): number {
    return useSyncExternalStore(
        active ? subscribe : noSubscribe,
        () => now,
        () => now
    );
}
