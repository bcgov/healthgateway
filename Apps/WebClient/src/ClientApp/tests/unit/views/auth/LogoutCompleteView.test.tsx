import { afterEach, beforeEach, describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { press, render, screen, unmount } from "@/test/ui";
import { LogoutCompleteView } from "@/views/auth/LogoutCompleteView";

describe("LogoutCompleteView", () => {
    beforeEach(() => {
        resetRouter(Path.LogoutComplete);
        services.initialize(configFixture, fakeAuth());
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });

    afterEach(async () => {
        await unmount();
        resetStores();
    });

    it("says the user signed out", async () => {
        await render(<LogoutCompleteView />);
        expect(screen.getByTestId("logout-complete-msg")).toBeTruthy();
    });

    if (Platform.OS === "web") {
        it("leaves logging in to the header on web", async () => {
            await render(<LogoutCompleteView />);
            expect(screen.queryByTestId("logoutCompleteLoginBtn")).toBeNull();
        });
        return;
    }

    it("offers Log in on the page on native, which routes to login", async () => {
        await render(<LogoutCompleteView />);
        await press(screen.getByTestId("logoutCompleteLoginBtn"));
        expect(router.push).toHaveBeenCalledWith(Path.Login);
    });

    it("has no Log in while still signed in", async () => {
        useAuthStore.setState({ isAuthenticated: true });
        await render(<LogoutCompleteView />);
        expect(screen.queryByTestId("logoutCompleteLoginBtn")).toBeNull();
    });
});
