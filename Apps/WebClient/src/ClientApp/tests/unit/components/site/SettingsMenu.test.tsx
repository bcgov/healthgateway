import { afterEach, beforeEach, describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { SettingsMenu } from "@/components/site/SettingsMenu";
import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter } from "@/test/expoRouterMock";
import { configFixture, resetStores, updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";

describe("SettingsMenu", () => {
    beforeEach(() => {
        resetRouter(Path.Root);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });

    afterEach(() => {
        resetStores();
    });

    it("shows only the gear until pressed", async () => {
        await render(<SettingsMenu />);
        expect(screen.getByLabelText("Settings")).toBeTruthy();
        expect(screen.queryByTestId("menu-btn-dark-mode-link")).toBeNull();
    });

    it("opens to Dark Mode, Security and Support", async () => {
        await render(<SettingsMenu />);
        await press(screen.getByTestId("settingsMenuBtn"));
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        // Security is native only (the Header shows this menu on native only)
        if (Platform.OS !== "web")
            expect(screen.getByTestId("menu-btn-security-link")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-support-link")).toBeTruthy();
    });

    // native only: the intro and the lock screen have no web counterpart
    describe("over the app (the intro and the lock screen)", () => {
        afterEach(() => {
            updateStores(() => useAppLockStore.setState({ locked: false }));
        });

        it("leaves out the app's own pages, which would open behind the screen", async () => {
            if (Platform.OS === "web") return;
            await render(<SettingsMenu overApp />);
            await press(screen.getByTestId("settingsMenuBtn"));
            expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
            expect(screen.queryByTestId("menu-btn-security-link")).toBeNull();
            await press(screen.getByTestId("menu-btn-support-link"));
            expect(screen.getByTestId("footer-faq-link")).toBeTruthy();
            expect(
                screen.queryByTestId("footer-terms-of-service-link")
            ).toBeNull();
        });

        it("opens while the app is locked", async () => {
            if (Platform.OS === "web") return;
            useAppLockStore.setState({ locked: true });
            await render(<SettingsMenu overApp />);
            await press(screen.getByTestId("settingsMenuBtn"));
            expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        });

        it("the app's own menu still hides while locked", async () => {
            if (Platform.OS === "web") return;
            useAppLockStore.setState({ locked: true });
            await render(<SettingsMenu />);
            await press(screen.getByTestId("settingsMenuBtn"));
            expect(screen.queryByTestId("menu-btn-dark-mode-link")).toBeNull();
        });
    });

    it("leaves Dark Mode alone when the links are hidden for maintenance", async () => {
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    offlineMode: {
                        startDateTime: "2000-01-01T00:00:00Z",
                        endDateTime: "2999-01-01T00:00:00Z",
                        message: "Down",
                        whitelist: [],
                    },
                },
            },
            status: "loaded",
        });
        await render(<SettingsMenu />);
        await press(screen.getByTestId("settingsMenuBtn"));
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        expect(screen.queryByTestId("menu-btn-support-link")).toBeNull();
    });
});
