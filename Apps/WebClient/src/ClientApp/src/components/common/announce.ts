import { useSyncExternalStore } from "react";
import { AccessibilityInfo, Platform } from "react-native";

export type Politeness = "polite" | "assertive";

interface Announcement {
    text: string;
    politeness: Politeness;
    /** counts up on every call, so the same text said twice still reaches the screen reader */
    seq: number;
}

let current: Announcement = { text: "", politeness: "polite", seq: 0 };
const listeners = new Set<() => void>();
const subscribe = (listener: () => void) => {
    listeners.add(listener);
    return () => listeners.delete(listener);
};
const read = () => current;

/**
 * Say something to the screen reader when there is no element to attach it to (a page change, a dialog opening).
 * Native: the platform's own announcement. Web: the hidden live regions `Announcer` keeps in the root layout.
 */
export function announce(text: string, politeness: Politeness = "polite") {
    if (!text) return;
    if (Platform.OS !== "web") {
        AccessibilityInfo.announceForAccessibility(text);
        return;
    }
    current = { text, politeness, seq: current.seq + 1 };
    listeners.forEach((listener) => listener());
}

/** The latest announcement (web: what `Announcer` renders). */
export function useAnnouncement(): Announcement {
    return useSyncExternalStore(subscribe, read, read);
}

/** Test support: forget the last announcement. */
export function resetAnnouncements() {
    current = { text: "", politeness: "polite", seq: 0 };
    listeners.forEach((listener) => listener());
}
