import { describe, expect, it, jest } from "@jest/globals";
import { fireEvent } from "@testing-library/react";
import { Platform } from "react-native";

import { HgChip } from "@/components/ui/HgChip";
import { isChecked, press, pressKey, render, screen, settle } from "@/test/ui";

describe("HgChip (v-chip port)", () => {
    it("a selectable chip is a checkbox that toggles on press", async () => {
        const onPress = jest.fn();
        await render(
            <HgChip selected={false} onPress={onPress} testID="chip">
                Medications
            </HgChip>
        );
        expect(isChecked(screen.getByTestId("chip"))).toBe(false);
        await press(screen.getByTestId("chip"));
        expect(onPress).toHaveBeenCalledTimes(1);
    });

    it("web: Space toggles a selectable chip once, as Enter does", async () => {
        if (Platform.OS !== "web") return;
        const onPress = jest.fn();
        await render(
            <HgChip selected onPress={onPress} testID="chip">
                Medications
            </HgChip>
        );
        await pressKey(screen.getByTestId("chip"), " ");
        expect(onPress).toHaveBeenCalledTimes(1);
        await pressKey(screen.getByTestId("chip"), "Enter");
        expect(onPress).toHaveBeenCalledTimes(2);
    });

    it("a closable chip runs onClose from its Close button", async () => {
        const onClose = jest.fn();
        await render(<HgChip onClose={onClose}>Immunization</HgChip>);
        await press(screen.getByLabelText("Close"));
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it("web: hovering a selectable chip tints it, as v-chip's overlay does, and leaving clears it", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgChip selected={false} onPress={() => undefined} testID="chip">
                Medications
            </HgChip>
        );
        const chip = screen.getByTestId("chip") as unknown as HTMLElement;
        expect(screen.queryByTestId("chip-hover-overlay")).toBeNull();
        fireEvent.mouseEnter(chip);
        await settle();
        const overlay = screen.getByTestId(
            "chip-hover-overlay"
        ) as unknown as HTMLElement;
        // 4 % of the primary text colour over the light surface
        expect(overlay.style.opacity).toBe("0.04");
        fireEvent.mouseLeave(chip);
        await settle();
        expect(screen.queryByTestId("chip-hover-overlay")).toBeNull();
    });
});
