import { beforeEach, describe, expect, it } from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { DarkModeMenuItem } from "@/components/site/DarkModeMenuItem";
import { useThemeStore } from "@/stores/theme.store";
import { isChecked, isExpanded, press, render, screen } from "@/test/ui";

const SYSTEM_TEXT =
    "We'll adjust your appearance based on your device's system settings";

const selected = (id: string) => isChecked(screen.getByTestId(id));

const open = () => press(screen.getByTestId("menu-btn-dark-mode-link"));

describe("DarkModeMenuItem", () => {
    beforeEach(async () => {
        await AsyncStorage.clear();
        useThemeStore.setState({ preference: "light", hydrated: true });
    });

    it("starts closed, showing only the entry", async () => {
        await render(<DarkModeMenuItem />);
        expect(screen.getByText("Dark Mode")).toBeTruthy();
        expect(isExpanded(screen.getByTestId("menu-btn-dark-mode-link"))).toBe(
            false
        );
    });

    it("opens to On, Off and System, with the System explanation", async () => {
        await render(<DarkModeMenuItem />);
        await open();
        expect(screen.getByText("On")).toBeTruthy();
        expect(screen.getByText("Off")).toBeTruthy();
        expect(screen.getByText("System")).toBeTruthy();
        expect(screen.getByText(SYSTEM_TEXT)).toBeTruthy();
    });

    it("closes again", async () => {
        await render(<DarkModeMenuItem />);
        await open();
        await open();
        expect(isExpanded(screen.getByTestId("menu-btn-dark-mode-link"))).toBe(
            false
        );
    });

    it("has Off selected by default", async () => {
        await render(<DarkModeMenuItem />);
        await open();
        expect(selected("dark-mode-off")).toBe(true);
        expect(selected("dark-mode-on")).toBe(false);
        expect(selected("dark-mode-system")).toBe(false);
    });

    it.each([
        ["dark", "dark-mode-on"],
        ["system", "dark-mode-system"],
    ] as const)("shows a saved %s choice as selected", async (value, id) => {
        useThemeStore.setState({ preference: value });
        await render(<DarkModeMenuItem />);
        await open();
        expect(selected(id)).toBe(true);
        expect(selected("dark-mode-off")).toBe(false);
    });

    it.each([
        ["dark-mode-on", "dark"],
        ["dark-mode-system", "system"],
    ] as const)("%s selects and saves %s", async (id, value) => {
        await render(<DarkModeMenuItem />);
        await open();
        await press(screen.getByTestId(id));
        expect(useThemeStore.getState().preference).toBe(value);
        expect(await AsyncStorage.getItem("hg.theme.preference")).toBe(value);
        expect(selected(id)).toBe(true);
        expect(selected("dark-mode-off")).toBe(false);
        // stays open, so the result of the choice can be seen
        expect(isExpanded(screen.getByTestId("menu-btn-dark-mode-link"))).toBe(
            true
        );
    });

    it("Off goes back to light", async () => {
        useThemeStore.setState({ preference: "dark" });
        await render(<DarkModeMenuItem />);
        await open();
        await press(screen.getByTestId("dark-mode-off"));
        expect(useThemeStore.getState().preference).toBe("light");
    });
});
