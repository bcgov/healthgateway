import { useSyncExternalStore } from "react";
import { Platform } from "react-native";

/** Controls `useBreakpoint()` in component tests (jest.mock("@/hooks/useBreakpoint") pointing here). */
export const breakpoint = {
    width: 1366,
    xs: false,
    smAndDown: false,
    mdAndUp: true,
    lgAndUp: true,
    isMobile: false,
    inlineEntryDetails: true,
};

const listeners = new Set<() => void>();
let version = 0;
const subscribe = (listener: () => void) => {
    listeners.add(listener);
    return () => listeners.delete(listener);
};

/**
 * Mounted components re-render, as they do when the real window changes, so wrap a call made after render in
 * `updateStores`. `height` matters only to `inlineEntryDetails` on native; by default the window is taller than it is wide.
 */
export function setBreakpoint(width: number, height = Math.max(width, 900)) {
    breakpoint.width = width;
    breakpoint.xs = width < 600;
    breakpoint.smAndDown = width < 960;
    breakpoint.mdAndUp = width >= 960;
    breakpoint.lgAndUp = width >= 1280;
    breakpoint.isMobile = width < 960;
    breakpoint.inlineEntryDetails =
        width >= 960 ||
        (Platform.OS !== "web" && Math.min(width, height) >= 600);
    version += 1;
    listeners.forEach((listener) => listener());
}

export const useBreakpoint = () => {
    useSyncExternalStore(subscribe, () => version);
    return { ...breakpoint };
};
