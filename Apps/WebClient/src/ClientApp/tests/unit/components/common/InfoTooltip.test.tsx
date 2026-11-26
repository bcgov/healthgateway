import { describe, expect, it } from "@jest/globals";
import { fireEvent } from "@testing-library/react";
import { Platform } from "react-native";

import { InfoTooltip } from "@/components/common/InfoTooltip";
import { useAppLockStore } from "@/stores/appLock.store";
import { expectNoAxeViolations } from "@/test/axe";
import { updateStores } from "@/test/fixtures";
import { inAct, press, render, screen, settle, waitFor } from "@/test/ui";

// the click that pins it starts listening for the next click on the next tick
const nextTick = () => new Promise((resolve) => setTimeout(resolve, 0));
const click = async (el: HTMLElement) => {
    fireEvent.click(el);
    await settle();
};

describe("InfoTooltip (InfoTooltipComponent.vue port)", () => {
    it("a press opens the tooltip text and a second press closes it", async () => {
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Ranges differ"
            />
        );
        expect(screen.queryByTestId("tip-popover")).toBeNull();
        await press(screen.getByTestId("tip"));
        await waitFor(() =>
            expect(screen.getByText("Ranges differ")).toBeTruthy()
        );
        if (Platform.OS === "web") {
            await press(screen.getByTestId("tip"));
            expect(screen.queryByTestId("tip-popover")).toBeNull();
        }
    });

    it("ios: locking closes it, and it stays closed after unlocking", async () => {
        if (Platform.OS !== "ios") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Ranges differ"
            />
        );
        await press(screen.getByTestId("tip"));
        expect(screen.getByTestId("tip-popover")).toBeTruthy();
        updateStores(() => useAppLockStore.setState({ locked: true }));
        await settle();
        expect(screen.queryByTestId("tip-popover")).toBeNull();
        updateStores(() => useAppLockStore.setState({ locked: false }));
        await settle();
        expect(screen.queryByTestId("tip-popover")).toBeNull();
    });

    it("web: hovering shows it, leaving hides it", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Hover text"
            />
        );
        const icon = screen.getByTestId("tip") as unknown as HTMLElement;
        fireEvent.mouseEnter(icon);
        await settle();
        expect(screen.getByText("Hover text")).toBeTruthy();
        fireEvent.mouseLeave(icon);
        await settle();
        expect(screen.queryByText("Hover text")).toBeNull();
    });

    it("web: focus alone does not show it; a press shows it as the trigger's description and Escape dismisses it", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Ranges differ"
            />
        );
        const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
        await inAct(() => trigger.focus());
        expect(screen.queryByTestId("tip-popover")).toBeNull();
        expect(trigger.getAttribute("aria-expanded")).toBe("false");
        await click(trigger);
        const bubble = screen.getByTestId(
            "tip-popover"
        ) as unknown as HTMLElement;
        expect(bubble.getAttribute("role")).toBe("tooltip");
        expect(trigger.getAttribute("aria-expanded")).toBe("true");
        expect(trigger.getAttribute("aria-describedby")).toBe(bubble.id);
        expect(bubble.id).toBeTruthy();
        await expectNoAxeViolations(document.body);
        await inAct(() =>
            globalThis.dispatchEvent(
                new KeyboardEvent("keydown", { key: "Escape" })
            )
        );
        expect(screen.queryByTestId("tip-popover")).toBeNull();
        expect(trigger.getAttribute("aria-describedby")).toBeNull();
    });

    it("web: a click pins it after the pointer leaves; the next click anywhere hides it", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Pinned text"
            />
        );
        const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
        fireEvent.mouseEnter(trigger);
        await click(trigger);
        fireEvent.mouseLeave(trigger);
        await nextTick();
        expect(screen.getByText("Pinned text")).toBeTruthy();
        await click(document.body);
        expect(screen.queryByText("Pinned text")).toBeNull();
    });

    it("web: after a click, hovering is ignored until the pointer leaves and returns", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Hover text"
            />
        );
        const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
        fireEvent.mouseEnter(trigger);
        await click(trigger);
        await nextTick();
        await click(trigger);
        expect(screen.queryByText("Hover text")).toBeNull();
        fireEvent.mouseLeave(trigger);
        fireEvent.mouseEnter(trigger);
        await settle();
        expect(screen.getByText("Hover text")).toBeTruthy();
    });

    it("web: the pointer can move onto the bubble and read it; leaving the bubble hides it", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Hover text"
            />
        );
        const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
        fireEvent.mouseEnter(trigger);
        await settle();
        const bubble = screen.getByTestId(
            "tip-popover"
        ) as unknown as HTMLElement;
        const hoverable = bubble.parentElement as HTMLElement;
        fireEvent.mouseLeave(trigger, { relatedTarget: bubble });
        fireEvent.mouseEnter(hoverable);
        await settle();
        expect(screen.getByText("Hover text")).toBeTruthy();
        fireEvent.mouseLeave(hoverable);
        await settle();
        expect(screen.queryByText("Hover text")).toBeNull();
    });
});
