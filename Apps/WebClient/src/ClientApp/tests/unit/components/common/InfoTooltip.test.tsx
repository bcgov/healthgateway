import { describe, expect, it } from "@jest/globals";
import { fireEvent } from "@testing-library/react";
import { Platform } from "react-native";

import { InfoTooltip } from "@/components/common/InfoTooltip";
import { tooltipPosition } from "@/components/common/infoTooltipPosition";
import { expectNoAxeViolations } from "@/test/axe";
import { inAct, press, render, screen, settle, waitFor } from "@/test/ui";

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

    it("web: focus shows it, Escape dismisses it, and the trigger is described by it while it shows", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <InfoTooltip
                testID="tip"
                tooltipTestID="tip-popover"
                text="Ranges differ"
            />
        );
        const trigger = screen.getByTestId("tip") as unknown as HTMLElement;
        expect(trigger.getAttribute("aria-expanded")).toBe("false");
        await inAct(() => trigger.focus());
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

describe("tooltipPosition (location end center, offset 10)", () => {
    it("right of the anchor and centred; flips left at the window edge", () => {
        expect(
            tooltipPosition(
                { x: 100, y: 100, width: 20, height: 20 },
                { width: 200, height: 40 },
                1000,
                800
            )
        ).toEqual({
            left: 130,
            top: 90,
        });
        expect(
            tooltipPosition(
                { x: 900, y: 100, width: 20, height: 20 },
                { width: 200, height: 40 },
                1000,
                800
            ).left
        ).toBe(690);
    });
});
