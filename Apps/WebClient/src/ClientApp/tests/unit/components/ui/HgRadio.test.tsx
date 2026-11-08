import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { HgRadio } from "@/components/ui/HgRadio";
import { isChecked, press, pressKey, render, screen } from "@/test/ui";

describe("HgRadio", () => {
    it("exposes the radio role with its state and description, and selects on press", async () => {
        const onSelect = jest.fn();
        await render(
            <HgRadio
                selected={false}
                onSelect={onSelect}
                label="System"
                description="Follows the device"
                testID="radio"
            />
        );
        expect(isChecked(screen.getByTestId("radio"))).toBe(false);
        expect(
            screen.getByLabelText("System. Follows the device")
        ).toBeTruthy();
        await press(screen.getByTestId("radio"));
        expect(onSelect).toHaveBeenCalledTimes(1);
    });

    it("web: Space selects it once, as Enter does", async () => {
        if (Platform.OS !== "web") return;
        const onSelect = jest.fn();
        await render(
            <HgRadio
                selected={false}
                onSelect={onSelect}
                label="On"
                testID="radio"
            />
        );
        await pressKey(screen.getByTestId("radio"), " ");
        expect(onSelect).toHaveBeenCalledTimes(1);
        await pressKey(screen.getByTestId("radio"), "Enter");
        expect(onSelect).toHaveBeenCalledTimes(2);
    });
});
