import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { HgChip } from "@/components/common/HgChip";
import { isChecked, press, pressKey, render, screen } from "@/test/ui";

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

    it("tints a selectable chip on hover, as v-chip's overlay does: 4 % of the text colour, doubled when selected", async () => {
        await render(
            <>
                <HgChip selected={false} onPress={() => undefined}>
                    Medications
                </HgChip>
                <HgChip selected onPress={() => undefined}>
                    Immunizations
                </HgChip>
                <HgChip onClose={() => undefined}>Keyword</HgChip>
            </>
        );
        // the closable label takes no press of its own, so it has no tint
        const overlays = screen.getAllByTestId("chip-hover-overlay");
        expect(overlays).toHaveLength(2);
        // NativeWind classes compile away on web; the native test renderer keeps them
        if (Platform.OS === "web") return;
        const [outlined, selected] = overlays.map((el) =>
            String((el as { props: { className?: string } }).props.className)
        );
        expect(outlined).toContain("bg-primary group-hover:opacity-[.04]");
        expect(selected).toContain("bg-on-primary group-hover:opacity-[.08]");
    });
});
