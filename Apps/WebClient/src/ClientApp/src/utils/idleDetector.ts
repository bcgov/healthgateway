import { ReliableTimer } from "./reliableTimer";

/** Subscribes to user activity; returns the unsubscribe. Web: document events; native: touches (see idleActivity). */
export type ActivitySource = (notify: () => void) => () => void;

/**
 * Port of utility/idleDetector.ts: calls back once no activity has been seen for `timeBeforeIdle` ms
 * (with how long past that the timer fired, for a suspended tab/app), then disables itself until re-enabled.
 * Activity notifications are throttled to one per second as on the web client.
 */
export class IdleDetector {
    private readonly idleTimer: ReliableTimer;
    private enabled = false;
    private unsubscribe?: () => void;
    private lastNotified = 0;

    constructor(
        private readonly isIdleCallback: (timeIdle: number) => void,
        private readonly timeBeforeIdle: number,
        private readonly activitySource: ActivitySource
    ) {
        this.idleTimer = new ReliableTimer(
            () => this.timerCallback(),
            this.timeBeforeIdle
        );
    }

    get isEnabled() {
        return this.enabled;
    }

    enable() {
        if (this.enabled) return;
        this.idleTimer.start();
        this.unsubscribe = this.activitySource(() => this.notifyActive());
        this.enabled = true;
    }

    disable() {
        if (!this.enabled) return;
        this.idleTimer.cancel();
        this.unsubscribe?.();
        this.unsubscribe = undefined;
        this.enabled = false;
    }

    private notifyActive() {
        if (!this.enabled) return;
        const now = Date.now();
        if (now - this.lastNotified < 1000) return;
        this.lastNotified = now;
        this.idleTimer.restart();
    }

    private timerCallback() {
        const timeIdle = this.idleTimer.elapsedTime - this.timeBeforeIdle;
        this.disable();
        this.isIdleCallback(timeIdle);
    }
}

/** IdleComponent.vue: the dialog counts down from 60 s minus however long the user has already been idle. */
export const MAX_IDLE_DIALOG_COUNTDOWN = 60000;

export const idleCountdown = (timeIdle: number) =>
    MAX_IDLE_DIALOG_COUNTDOWN - timeIdle;
