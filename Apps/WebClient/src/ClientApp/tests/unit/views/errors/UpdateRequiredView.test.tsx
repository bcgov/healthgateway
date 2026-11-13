import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Linking, Platform } from "react-native";

import { APP_STORE_URL } from "@/config/storeListings";
import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import { UpdateRequiredView } from "@/views/errors/UpdateRequiredView";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

describe("UpdateRequiredView", () => {
    beforeEach(() => {
        resetRouter(Path.Root);
    });

    afterEach(() => {
        jest.restoreAllMocks();
        updateStores(() => {
            useAppLockStore.setState({ locked: false });
            useAuthStore.setState({ isAuthenticated: false });
        });
    });

    it("says an update must be installed, with no way past it", async () => {
        await render(<UpdateRequiredView />);
        expect(screen.getByText("Update required")).toBeTruthy();
        expect(
            screen.getByText(
                "A new version of Health Gateway is available and must be installed to continue."
            )
        ).toBeTruthy();
        expect(screen.queryByLabelText("Close")).toBeNull();
    });

    it("native: Update opens this platform's store listing", async () => {
        if (Platform.OS === "web") return;
        const openURL = jest
            .spyOn(Linking, "openURL")
            .mockResolvedValue(undefined);
        await render(<UpdateRequiredView />);
        await press(screen.getByTestId("updateRequiredButton"));
        expect(openURL).toHaveBeenCalledWith(APP_STORE_URL);
        expect(screen.getByText("Update required")).toBeTruthy();
    });

    it("web: no store to send the user to", async () => {
        if (Platform.OS !== "web") return;
        await render(<UpdateRequiredView />);
        expect(screen.queryByTestId("updateRequiredButton")).toBeNull();
    });

    describe("the page behind it", () => {
        it("stays on the landing page", async () => {
            await render(<UpdateRequiredView />);
            expect(router.replace).not.toHaveBeenCalled();
        });

        it("signed out on another page (a link opened the app there): the landing page instead", async () => {
            resetRouter(Path.Home);
            await render(<UpdateRequiredView />);
            expect(router.replace).toHaveBeenCalledWith(Path.Root);
        });

        it("signed in (the minimum rose while the app was open): the page the user is on", async () => {
            resetRouter(Path.Home);
            useAuthStore.setState({ isAuthenticated: true });
            await render(<UpdateRequiredView />);
            expect(router.replace).not.toHaveBeenCalled();
        });
    });

    describe("the device lock", () => {
        it("signed out: shows even if the app was locked, as nothing is behind it to protect", async () => {
            useAppLockStore.setState({ locked: true });
            await render(<UpdateRequiredView />);
            expect(screen.getByText("Update required")).toBeTruthy();
            expect(useAppLockStore.getState().locked).toBe(false);
        });

        it("signed out: stays shown when the app locks while it is up", async () => {
            await render(<UpdateRequiredView />);
            // back from the background after the idle timeout
            updateStores(() => useAppLockStore.setState({ locked: true }));
            expect(useAppLockStore.getState().locked).toBe(false);
            expect(screen.getByText("Update required")).toBeTruthy();
        });

        it("signed in: the lock stays, covering the user's page, and the dialog shows once unlocked", async () => {
            useAuthStore.setState({ isAuthenticated: true });
            useAppLockStore.setState({ locked: true });
            await render(<UpdateRequiredView />);
            expect(useAppLockStore.getState().locked).toBe(true);
            expect(screen.queryByText("Update required")).toBeNull();
            updateStores(() => useAppLockStore.setState({ locked: false }));
            expect(screen.getByText("Update required")).toBeTruthy();
        });
    });
});
