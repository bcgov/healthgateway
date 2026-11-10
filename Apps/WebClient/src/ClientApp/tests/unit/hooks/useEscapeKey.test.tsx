import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { useEscapeKey } from "@/hooks/useEscapeKey";
import { renderHook } from "@/test/ui";

const keydown = (key: string) =>
    globalThis.dispatchEvent(new KeyboardEvent("keydown", { key }));

describe("useEscapeKey", () => {
    if (Platform.OS !== "web") {
        it("does nothing on native", async () => {
            const onEscape = jest.fn();
            const hook = await renderHook(() => useEscapeKey(true, onEscape));
            await hook.unmount();
            expect(onEscape).not.toHaveBeenCalled();
        });
        return;
    }

    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("runs the callback on Escape while open, and only on Escape", async () => {
        const onEscape = jest.fn();
        await renderHook(() => useEscapeKey(true, onEscape));
        keydown("Enter");
        expect(onEscape).not.toHaveBeenCalled();
        keydown("Escape");
        expect(onEscape).toHaveBeenCalledTimes(1);
    });

    it("ignores Escape while closed and listens once opened", async () => {
        const onEscape = jest.fn();
        let open = false;
        const hook = await renderHook(() => useEscapeKey(open, onEscape));
        keydown("Escape");
        expect(onEscape).not.toHaveBeenCalled();
        open = true;
        await hook.rerender();
        keydown("Escape");
        expect(onEscape).toHaveBeenCalledTimes(1);
    });

    it("leaves Escape to a modal open over it, so only the top-most layer closes", async () => {
        const onEscape = jest.fn();
        await renderHook(() => useEscapeKey(true, onEscape));
        const modal = document.createElement("div");
        modal.setAttribute("aria-modal", "true");
        document.body.appendChild(modal);
        keydown("Escape");
        expect(onEscape).not.toHaveBeenCalled();
        modal.remove();
        keydown("Escape");
        expect(onEscape).toHaveBeenCalledTimes(1);
    });

    it("calls the latest callback and stops listening when unmounted", async () => {
        const first = jest.fn();
        const second = jest.fn();
        let onEscape = first;
        const hook = await renderHook(() => useEscapeKey(true, onEscape));
        onEscape = second;
        await hook.rerender();
        keydown("Escape");
        expect(first).not.toHaveBeenCalled();
        expect(second).toHaveBeenCalledTimes(1);
        await hook.unmount();
        keydown("Escape");
        expect(second).toHaveBeenCalledTimes(1);
    });
});
