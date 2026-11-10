import type { ActivitySource } from "@/utils/idleDetector";

const listeners = new Set<() => void>();

/** Native: the shell reports every touch through `notifyIdleActivity` (see AppShell). */
export const idleActivitySource: ActivitySource = (notify) => {
    listeners.add(notify);
    return () => {
        listeners.delete(notify);
    };
};

export const notifyIdleActivity = () => {
    listeners.forEach((l) => l());
};
