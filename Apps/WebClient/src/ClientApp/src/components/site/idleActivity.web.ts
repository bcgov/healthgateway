import type { ActivitySource } from "@/utils/idleDetector";

const events = ["mousemove", "keydown", "mousedown", "touchstart"] as const;

/** IdleDetector's document listeners from the web client. */
export const idleActivitySource: ActivitySource = (notify) => {
    for (const e of events) document.addEventListener(e, notify);
    return () => {
        for (const e of events) document.removeEventListener(e, notify);
    };
};

/** Native only; nothing to report on web (document events cover it). */
export const notifyIdleActivity = () => undefined;
