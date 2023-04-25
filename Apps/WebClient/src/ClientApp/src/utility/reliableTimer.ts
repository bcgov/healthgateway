export class ReliableTimer {
    private readonly interval = 1000;
    private timeoutId?: ReturnType<typeof setTimeout>;
    private startingTimestamp?: number;

    constructor(private callback: () => void, private duration: number) {
        if (duration < this.interval) {
            throw new Error(`Duration must be at least ${this.interval}ms`);
        }
    }

    get elapsedTime(): number {
        if (this.startingTimestamp === undefined) {
            return 0;
        }

        return Date.now() - this.startingTimestamp;
    }

    get remainingTime(): number {
        return Math.max(this.duration - this.elapsedTime, 0);
    }

    start() {
        this.startingTimestamp = Date.now();
        this.checkTime();
    }

    cancel() {
        clearTimeout(this.timeoutId);
        this.startingTimestamp = undefined;
    }

    restart() {
        this.cancel();
        this.start();
    }

    private checkTime() {
        const timeToNextCheck = Math.min(this.remainingTime, this.interval);
        if (timeToNextCheck === 0) {
            this.callback();
            return;
        }

        this.timeoutId = setTimeout(() => this.checkTime(), timeToNextCheck);
    }
}
