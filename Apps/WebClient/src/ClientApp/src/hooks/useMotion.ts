import { useReducedMotion } from "react-native-reanimated";

/**
 * Motion that honours the system's "reduce motion" setting (WCAG 2.3.3, 2.2.2): a duration becomes 0 and a Modal's
 * animation "none" while it is on. Reanimated reads the setting on iOS and Android and `prefers-reduced-motion` on
 * web. Movement the user drives (a swipe, a pull, paging a carousel) is exempt and keeps its own timing.
 */
export function useMotion() {
    const reduced = useReducedMotion();
    return {
        reduced,
        /** a duration in ms, or 0 while motion is reduced */
        ms: (duration: number) => (reduced ? 0 : duration),
        /** a Modal's `animationType`, or "none" while motion is reduced */
        modal: <T extends "fade" | "slide" | "none" | undefined>(type: T) =>
            reduced ? ("none" as const) : type,
    } as const;
}
