import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { SessionBanner } from "@/components/site/SessionBanner";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { resetRouter } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen, textOf } from "@/test/ui";

describe("SessionBanner", () => {
    beforeEach(() => {
        resetRouter("/timeline");
        services.initialize(configFixture, fakeAuth());
        useConnectivityStore.setState({
            isOnline: true,
            sessionStatus: "live",
            savedAt: undefined,
        });
    });

    afterEach(() => {
        updateStores(() =>
            useConnectivityStore.setState({
                isOnline: true,
                serviceUnavailable: false,
                sessionStatus: "live",
                savedAt: undefined,
            })
        );
        useAuthStore.setState({ loginRequest: undefined });
        resetStores();
    });

    it("shows nothing when all is well", async () => {
        await render(<SessionBanner />);
        expect(screen.queryByTestId("offlineBanner")).toBeNull();
        expect(screen.queryByTestId("sessionExpiredBanner")).toBeNull();
    });

    if (Platform.OS === "web") {
        it("is not on web", async () => {
            useConnectivityStore.setState({ isOnline: false });
            await render(<SessionBanner />);
            expect(screen.queryByTestId("offlineBanner")).toBeNull();
        });
        return;
    }

    it("says the app is offline", async () => {
        useConnectivityStore.setState({ isOnline: false });
        await render(<SessionBanner />);
        expect(textOf(screen.getByTestId("offlineBanner"))).toContain(
            "You are offline. Changes can't be saved until you reconnect."
        );
    });

    it("says Health Gateway is unavailable when the device has a connection", async () => {
        useConnectivityStore.setState({ serviceUnavailable: true });
        await render(<SessionBanner />);
        expect(
            textOf(screen.getByTestId("serviceUnavailableBanner"))
        ).toContain("Health Gateway is currently unavailable.");
        expect(screen.queryByTestId("offlineBanner")).toBeNull();
    });

    it("says offline, not unavailable, when the device has no connection", async () => {
        useConnectivityStore.setState({
            isOnline: false,
            serviceUnavailable: true,
        });
        await render(<SessionBanner />);
        expect(screen.getByTestId("offlineBanner")).toBeTruthy();
        expect(screen.queryByTestId("serviceUnavailableBanner")).toBeNull();
    });

    it("a saved copy opened with a connection but no service says unavailable, with its date", async () => {
        useConnectivityStore.setState({
            sessionStatus: "offline",
            savedAt: new Date(2026, 9, 30, 14, 5).getTime(),
        });
        await render(<SessionBanner />);
        expect(
            textOf(screen.getByTestId("serviceUnavailableBanner"))
        ).toContain("Showing records saved 2026-Oct-30, 2:05 PM.");
    });

    it("says when the records on screen were saved", async () => {
        useConnectivityStore.setState({
            isOnline: false,
            sessionStatus: "offline",
            savedAt: new Date(2026, 9, 30, 14, 5).getTime(),
        });
        await render(<SessionBanner />);
        expect(textOf(screen.getByTestId("offlineBanner"))).toContain(
            "Showing records saved 2026-Oct-30, 2:05 PM."
        );
    });

    it("an expired login offers a link that starts the login", async () => {
        useConnectivityStore.setState({ sessionStatus: "expired" });
        useAuthStore.setState({ isAuthenticated: true });
        await render(<SessionBanner />);
        expect(
            screen.getByText(
                "Your login has expired. Log in again to see your latest records."
            )
        ).toBeTruthy();

        await press(screen.getByTestId("sessionExpiredLoginBtn"));
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useAuthStore.getState().loginRequest).toEqual({ idp: "bcsc" });
    });

    it("offline comes first: logging in could not work anyway", async () => {
        useConnectivityStore.setState({
            isOnline: false,
            sessionStatus: "expired",
        });
        await render(<SessionBanner />);
        expect(screen.getByTestId("offlineBanner")).toBeTruthy();
        expect(screen.queryByTestId("sessionExpiredLoginBtn")).toBeNull();
    });

    it("the banners are assertive live regions: the reader hears them as they appear", async () => {
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        useConnectivityStore.setState({ isOnline: false });
        await render(<SessionBanner />);
        expect(say).toHaveBeenCalledWith(
            expect.stringContaining("You are offline.")
        );
        say.mockRestore();
    });
});
