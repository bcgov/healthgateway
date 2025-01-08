import { throttle } from "throttle-debounce";

import { ReliableTimer } from "@/utility/reliableTimer";

type Event = keyof DocumentEventMap;

export class IdleDetector {
    private readonly events: Event[] = [
        "mousemove",
        "keydown",
        "mousedown",
        "touchstart",
    ];
    private readonly idleTimer: ReliableTimer;
    private enabled = false;
    private readonly notifyActiveThrottled = throttle(1000, () =>
        this.notifyActive()
    );

    constructor(
        private readonly isIdleCallback: (timeIdle: number) => void,
        private readonly timeBeforeIdle: number,
        startEnabled: boolean
    ) {
        this.idleTimer = new ReliableTimer(
            () => this.timerCallback(),
            this.timeBeforeIdle
        );

        if (startEnabled) {
            this.enable();
        }
    }

    public enable() {
        if (this.enabled) {
            return;
        }

        this.idleTimer.start();
        for (const event of this.events) {
            document.addEventListener(event, this.notifyActiveThrottled);
        }

        this.enabled = true;
    }

    public disable() {
        if (!this.enabled) {
            return;
        }

        this.idleTimer.cancel();
        for (const event of this.events) {
            document.removeEventListener(event, this.notifyActiveThrottled);
        }

        this.enabled = false;
    }

    public restart() {
        this.enable();
    }

    private notifyActive() {
        if (this.enabled) {
            this.idleTimer.restart();
        }
    }

    private timerCallback(): void {
        const timeIdle = this.idleTimer.elapsedTime - this.timeBeforeIdle;
        this.isIdleCallback(timeIdle);
        this.disable();
    }
}
