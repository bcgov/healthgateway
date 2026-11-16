import { afterEach, beforeEach, describe, expect, it } from "@jest/globals";
import { createElement } from "react";

// the Jest native project is iOS: this is `NativeSettingsMenu.ios.tsx`
import { NativeSettingsMenu } from "@/components/site/NativeSettingsMenu";
import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useConfigStore } from "@/stores/config.store";
import { useThemeStore } from "@/stores/theme.store";
import { resetRouter } from "@/test/expoRouterMock";
import { configFixture, resetStores, updateStores } from "@/test/fixtures";
import { inAct, render, screen } from "@/test/ui";

// native only (a .ts test): web has no screens over the app

interface NativeProps {
    selection?: string | null;
    onSelectionChange?: (event: { nativeEvent: { selection: string } }) => void;
}

const nativeProps = (testID: string) =>
    screen.getByTestId(testID).props as NativeProps;

describe("NativeSettingsMenu", () => {
    beforeEach(() => {
        resetRouter(Path.Root);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });

    afterEach(() => {
        updateStores(() => useAppLockStore.setState({ locked: false }));
        resetStores();
    });

    it("is SwiftUI's menu under a gear, with Dark Mode, Security, Support and the version", async () => {
        await render(createElement(NativeSettingsMenu));
        expect(screen.getByTestId("settingsMenuBtn")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-security-link")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-support-link")).toBeTruthy();
        expect(screen.getByTestId("footer-terms-of-service-link")).toBeTruthy();
    });

    it("over the app: no Security, and only the links that leave the app", async () => {
        await render(createElement(NativeSettingsMenu, { overApp: true }));
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        expect(screen.queryByTestId("menu-btn-security-link")).toBeNull();
        expect(screen.getByTestId("footer-faq-link")).toBeTruthy();
        expect(screen.queryByTestId("footer-terms-of-service-link")).toBeNull();
    });

    it("Dark Mode is a picker on the appearance in use, and switches to the one picked", async () => {
        await render(createElement(NativeSettingsMenu, { overApp: true }));
        const before = useThemeStore.getState().preference;
        expect(nativeProps("menu-btn-dark-mode-link").selection).toBe(
            `dark-mode-${{ dark: "on", light: "off", system: "system" }[before]}`
        );
        await inAct(() =>
            nativeProps("menu-btn-dark-mode-link").onSelectionChange?.({
                nativeEvent: { selection: "dark-mode-on" },
            })
        );
        expect(useThemeStore.getState().preference).toBe("dark");
        expect(nativeProps("menu-btn-dark-mode-link").selection).toBe(
            "dark-mode-on"
        );
    });

    it("stays while the app is locked: it is on the lock screen itself", async () => {
        useAppLockStore.setState({ locked: true });
        await render(createElement(NativeSettingsMenu, { overApp: true }));
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
    });
});
