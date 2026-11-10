import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import {
    MaxCancelledTouchHold,
    MinPressDuration,
    notifyScrollDragEnd,
    usePressHighlight,
} from "@/hooks/usePressHighlight";
import { advanceTimers, inAct, renderHook } from "@/test/ui";

describe("usePressHighlight", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });
    afterEach(() => {
        jest.useRealTimers();
    });

    it("shades while the pointer is over the card", async () => {
        const hook = await renderHook(() => usePressHighlight());
        expect(hook.result.current.highlighted).toBe(false);
        await inAct(() => hook.result.current.handlers.onHoverIn());
        expect(hook.result.current.highlighted).toBe(true);
        expect(hook.result.current.pressed).toBe(false);
        await inAct(() => hook.result.current.handlers.onHoverOut());
        expect(hook.result.current.highlighted).toBe(false);
        await hook.unmount();
    });

    it("keeps a quick tap shaded for the minimum press time", async () => {
        const hook = await renderHook(() => usePressHighlight());
        // react-native-web starts and ends a quick tap in the same tick
        await inAct(() => {
            hook.result.current.handlers.onPressIn();
            hook.result.current.handlers.onPressOut();
        });
        expect(hook.result.current.highlighted).toBe(true);
        expect(hook.result.current.pressed).toBe(true);
        await advanceTimers(MinPressDuration - 1);
        expect(hook.result.current.highlighted).toBe(true);
        await advanceTimers(1);
        expect(hook.result.current.highlighted).toBe(false);
        expect(hook.result.current.pressed).toBe(false);
        await hook.unmount();
    });

    it("ends a long press at once on release", async () => {
        const hook = await renderHook(() => usePressHighlight());
        await inAct(() => hook.result.current.handlers.onPressIn());
        await advanceTimers(MinPressDuration + 200);
        expect(hook.result.current.highlighted).toBe(true);
        await inAct(() => hook.result.current.handlers.onPressOut());
        expect(hook.result.current.highlighted).toBe(false);
        await hook.unmount();
    });

    it("a touch stays shaded until the finger lifts, though the press ends as it moves off", async () => {
        const hook = await renderHook(() => usePressHighlight());
        const h = () => hook.result.current.handlers;
        await inAct(() => {
            h().onTouchStart();
            h().onPressIn();
        });
        await advanceTimers(MinPressDuration + 200);
        await inAct(() => h().onPressOut());
        expect(hook.result.current.highlighted).toBe(true);
        await advanceTimers(1000);
        expect(hook.result.current.highlighted).toBe(true);
        await inAct(() => h().onTouchEnd());
        expect(hook.result.current.highlighted).toBe(false);
        await hook.unmount();
    });

    it("a touch taken over by a scroll stays shaded until the drag ends (native) or ends with the cancel (web)", async () => {
        const hook = await renderHook(() => usePressHighlight());
        const h = () => hook.result.current.handlers;
        await inAct(() => {
            h().onTouchStart();
            h().onPressIn();
        });
        await advanceTimers(MinPressDuration + 200);
        // native sends the cancel and the end of the press in either order
        await inAct(() => {
            h().onTouchCancel();
            h().onPressOut();
        });
        if (Platform.OS === "web") {
            expect(hook.result.current.highlighted).toBe(false);
        } else {
            expect(hook.result.current.highlighted).toBe(true);
            await inAct(() => notifyScrollDragEnd());
            expect(hook.result.current.highlighted).toBe(false);
        }
        await hook.unmount();
    });

    it("native: a cancelled touch with no drag end clears after the longest hold", async () => {
        if (Platform.OS === "web") return;
        const hook = await renderHook(() => usePressHighlight());
        await inAct(() => {
            hook.result.current.handlers.onTouchStart();
            hook.result.current.handlers.onTouchCancel();
        });
        await advanceTimers(MaxCancelledTouchHold - 1);
        expect(hook.result.current.highlighted).toBe(true);
        await advanceTimers(1);
        expect(hook.result.current.highlighted).toBe(false);
        await hook.unmount();
    });

    it("a drag end leaves a card no touch is waiting on alone", async () => {
        const hook = await renderHook(() => usePressHighlight());
        await inAct(() => hook.result.current.handlers.onTouchStart());
        await inAct(() => notifyScrollDragEnd());
        expect(hook.result.current.highlighted).toBe(true);
        await hook.unmount();
    });

    it("a new press keeps the shade the last one was about to clear", async () => {
        const hook = await renderHook(() => usePressHighlight());
        await inAct(() => {
            hook.result.current.handlers.onPressIn();
            hook.result.current.handlers.onPressOut();
        });
        await advanceTimers(MinPressDuration - 10);
        await inAct(() => hook.result.current.handlers.onPressIn());
        await advanceTimers(MinPressDuration);
        expect(hook.result.current.pressed).toBe(true);
        await hook.unmount();
    });
});
