import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { useNow } from "@/hooks/useNow";
import { advanceTimers, renderHook } from "@/test/ui";

const secondTimers = (spy: jest.SpiedFunction<typeof setInterval>) =>
    spy.mock.calls.filter((c) => c[1] === 1000).length;

describe("useNow", () => {
    let setIntervalSpy: jest.SpiedFunction<typeof setInterval>;
    let clearIntervalSpy: jest.SpiedFunction<typeof clearInterval>;
    beforeEach(() => {
        jest.useFakeTimers();
        setIntervalSpy = jest.spyOn(global, "setInterval");
        clearIntervalSpy = jest.spyOn(global, "clearInterval");
    });
    afterEach(() => {
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    it("re-renders with the current time once per second", async () => {
        const hook = await renderHook(() => useNow());
        const start = hook.result.current;
        await advanceTimers(999);
        expect(hook.result.current).toBe(start);
        await advanceTimers(1);
        expect(hook.result.current).toBeGreaterThanOrEqual(start + 1000);
        await hook.unmount();
    });

    it("shares one interval between subscribers and clears it when they unmount", async () => {
        const hook = await renderHook(() => [useNow(), useNow(), useNow()]);
        expect(secondTimers(setIntervalSpy)).toBe(1);
        const [a, b, c] = hook.result.current;
        expect(a).toBe(b);
        expect(b).toBe(c);
        await hook.unmount();
        expect(clearIntervalSpy).toHaveBeenCalledTimes(1);
    });

    it("starts from the current time when ticking resumes after a pause", async () => {
        const first = await renderHook(() => useNow());
        await first.unmount();
        const resumedAt = Date.now() + 10 * 60 * 1000;
        jest.setSystemTime(resumedAt);
        const second = await renderHook(() => useNow());
        expect(second.result.current).toBeGreaterThanOrEqual(resumedAt);
        await second.unmount();
    });

    it("does not tick or start a timer while inactive", async () => {
        const hook = await renderHook(() => useNow(false));
        const start = hook.result.current;
        expect(secondTimers(setIntervalSpy)).toBe(0);
        await advanceTimers(5000);
        expect(hook.result.current).toBe(start);
        await hook.unmount();
    });
});
