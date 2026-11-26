import { afterEach, describe, expect, it, jest } from "@jest/globals";
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
    afterEach(() => {
        jest.restoreAllMocks();
    });
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

    it.each([
        { x: 100, y: 300, left: "10px", top: "320px" },
        { x: 1000, y: 740, left: "824px", top: "690px" },
    ])(
        "web: without anchor positioning it sits below the icon, above when there is no room below, inside the window (icon at $x, $y)",
        async ({ x, y, left, top }) => {
            if (Platform.OS !== "web") return;
            jest.spyOn(
                HTMLElement.prototype,
                "offsetWidth",
                "get"
            ).mockReturnValue(200);
            jest.spyOn(
                HTMLElement.prototype,
                "offsetHeight",
                "get"
            ).mockReturnValue(50);
            await render(
                <InfoTooltip
                    testID="tip"
                    tooltipTestID="tip-popover"
                    text="Ranges differ"
                />
            );
            const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
            jest.spyOn(trigger, "getBoundingClientRect").mockReturnValue(
                new DOMRect(x, y, 20, 20)
            );
            await click(trigger);
            const popover = (
                screen.getByTestId("tip-popover") as unknown as HTMLElement
            ).parentElement as HTMLElement;
            expect(popover.style.left).toBe(left);
            expect(popover.style.top).toBe(top);
        }
    );

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

    // Enter / Space on the icon (a native button) makes the browser dispatch a click, so this covers the keyboard too
    it("web: a click on another tooltip's icon hides a pinned one", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <>
                <InfoTooltip testID="tip-a" text="First text" />
                <InfoTooltip testID="tip-b" text="Second text" />
            </>
        );
        await click(screen.getByTestId("tip-a") as unknown as HTMLElement);
        await nextTick();
        expect(screen.getByText("First text")).toBeTruthy();
        await click(screen.getByTestId("tip-b") as unknown as HTMLElement);
        expect(screen.queryByText("First text")).toBeNull();
        expect(screen.getByText("Second text")).toBeTruthy();
    });

    it("web: hovering another tooltip hides a pinned one", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <>
                <InfoTooltip testID="tip-a" text="First text" />
                <InfoTooltip testID="tip-b" text="Second text" />
            </>
        );
        await click(screen.getByTestId("tip-a") as unknown as HTMLElement);
        fireEvent.mouseEnter(
            screen.getByTestId("tip-b") as unknown as HTMLElement
        );
        await settle();
        expect(screen.queryByText("First text")).toBeNull();
        expect(screen.getByText("Second text")).toBeTruthy();
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
