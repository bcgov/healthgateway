import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { idleCountdown, IdleDetector } from "@/utils/idleDetector";
import { ReliableTimer } from "@/utils/reliableTimer";

describe("ReliableTimer", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });
    afterEach(() => {
        jest.useRealTimers();
    });

    it("fires once the duration has elapsed and reports elapsed / remaining time", () => {
        const cb = jest.fn();
        const t = new ReliableTimer(cb, 3000);
        t.start();
        jest.advanceTimersByTime(2000);
        expect(cb).not.toHaveBeenCalled();
        expect(t.remainingTime).toBe(1000);
        jest.advanceTimersByTime(1000);
        expect(cb).toHaveBeenCalledTimes(1);
        expect(t.elapsedTime).toBe(3000);
    });

    it("restart begins again and cancel stops it", () => {
        const cb = jest.fn();
        const t = new ReliableTimer(cb, 2000);
        t.start();
        jest.advanceTimersByTime(1500);
        t.restart();
        jest.advanceTimersByTime(1500);
        expect(cb).not.toHaveBeenCalled();
        t.cancel();
        jest.advanceTimersByTime(5000);
        expect(cb).not.toHaveBeenCalled();
        expect(t.elapsedTime).toBe(0);
    });

    it("rejects durations under a second", () => {
        expect(() => new ReliableTimer(() => undefined, 500)).toThrow();
    });
});

describe("IdleDetector", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });
    afterEach(() => {
        jest.useRealTimers();
    });

    const source = () => {
        let notify: (() => void) | undefined;
        const activity: (n: () => void) => () => void = (n) => {
            notify = n;
            return () => {
                notify = undefined;
            };
        };
        return { activity, act: () => notify?.(), subscribed: () => !!notify };
    };

    it("calls back after the idle time with no activity, then disables itself", () => {
        const cb = jest.fn();
        const s = source();
        const d = new IdleDetector(cb, 5000, s.activity);
        d.enable();
        expect(s.subscribed()).toBe(true);
        jest.advanceTimersByTime(5000);
        expect(cb).toHaveBeenCalledWith(0);
        expect(d.isEnabled).toBe(false);
        expect(s.subscribed()).toBe(false);
    });

    it("activity restarts the idle timer, throttled to once a second", () => {
        const cb = jest.fn();
        const s = source();
        const d = new IdleDetector(cb, 5000, s.activity);
        d.enable();
        jest.advanceTimersByTime(4000);
        s.act();
        jest.advanceTimersByTime(4000);
        expect(cb).not.toHaveBeenCalled();
        jest.advanceTimersByTime(1000);
        expect(cb).toHaveBeenCalledTimes(1);
    });

    it("reports how long past the idle time the timer fired (suspended app)", () => {
        const cb = jest.fn();
        const s = source();
        const d = new IdleDetector(cb, 5000, s.activity);
        d.enable();
        // the JS timer wakes 70 s late, as after the app was in the background
        jest.setSystemTime(Date.now() + 75000);
        jest.advanceTimersByTime(1000);
        expect(cb).toHaveBeenCalledTimes(1);
        const timeIdle = cb.mock.calls[0][0] as number;
        expect(timeIdle).toBeGreaterThanOrEqual(70000);
        expect(idleCountdown(70000)).toBeLessThan(0);
        expect(idleCountdown(0)).toBe(60000);
    });

    it("enable and disable are idempotent", () => {
        const s = source();
        const d = new IdleDetector(() => undefined, 5000, s.activity);
        d.disable();
        d.enable();
        d.enable();
        expect(d.isEnabled).toBe(true);
        d.disable();
        d.disable();
        expect(d.isEnabled).toBe(false);
    });
});
