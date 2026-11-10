import { afterEach, beforeEach, describe, expect, it } from "@jest/globals";

import { SettingsMenu } from "@/components/site/SettingsMenu";
import { Path } from "@/constants/path";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter } from "@/test/expoRouterMock";
import { configFixture, resetStores } from "@/test/fixtures";
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

    it("opens to Dark Mode and Support", async () => {
        await render(<SettingsMenu />);
        await press(screen.getByTestId("settingsMenuBtn"));
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-support-link")).toBeTruthy();
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
