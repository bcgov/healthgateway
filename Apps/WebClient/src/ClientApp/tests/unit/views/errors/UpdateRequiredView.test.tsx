import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { BackHandler, Linking, Platform, Text } from "react-native";

import { AppModal } from "@/components/common/AppModal";
import { APP_STORE_URL } from "@/config/storeListings";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import {
    UpdateRequiredLanding,
    UpdateRequiredView,
} from "@/views/errors/UpdateRequiredView";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));
// this build is 57; the configuration below asks for 300
jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "3.0.0", extra: { build: 57 } } },
}));

const requireUpdate = () =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                mobile: {
                    ...configFixture.webClient.mobile,
                    minimumBuild: 300,
                },
            },
        },
        status: "loaded",
    });

describe("UpdateRequiredView", () => {
    afterEach(() => {
        jest.restoreAllMocks();
        updateStores(() => {
            useAppLockStore.setState({ locked: false });
            useAuthStore.setState({ isAuthenticated: false });
            useConfigStore.setState({ config: undefined, status: "idle" });
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

    it("native: Android's back does nothing, so the page underneath stays put", async () => {
        if (Platform.OS === "web") return;
        const add = jest.spyOn(BackHandler, "addEventListener");
        await render(<UpdateRequiredView />);
        const handler = add.mock.calls.find(
            ([event]) => event === "hardwareBackPress"
        )?.[1] as (() => boolean | null | undefined) | undefined;
        expect(handler?.()).toBe(true);
    });

    describe("other dialogs (iOS shows one modal window at a time)", () => {
        beforeEach(() => {
            useAuthStore.setState({ isAuthenticated: true });
        });

        it("native: one left open hides while an update is required, so it cannot cover this one", async () => {
            if (Platform.OS === "web") return; // web never requires an update
            await render(
                <>
                    <AppModal visible>
                        <Text>filter</Text>
                    </AppModal>
                    <UpdateRequiredView />
                </>
            );
            expect(screen.getByText("filter")).toBeTruthy();
            updateStores(requireUpdate);
            expect(screen.queryByText("filter")).toBeNull();
            expect(screen.getByText("Update required")).toBeTruthy();
        });

        it("native: this one is drawn over the page, not in a modal window, so the lock does not hide it", async () => {
            if (Platform.OS === "web") return;
            useAppLockStore.setState({ locked: true });
            await render(<UpdateRequiredView />);
            // still there, under the lock screen (the root layout draws that above it)
            expect(screen.getByText("Update required")).toBeTruthy();
        });

        it("native: the lock screen's own menu still opens over the lock", async () => {
            if (Platform.OS === "web") return;
            requireUpdate();
            useAppLockStore.setState({ locked: true });
            await render(
                <AppModal visible overLock>
                    <Text>lock screen menu</Text>
                </AppModal>
            );
            expect(screen.getByText("lock screen menu")).toBeTruthy();
        });
    });

    describe("the device lock", () => {
        it("signed out: cleared, as nothing is behind the dialog to protect", async () => {
            useAppLockStore.setState({ locked: true });
            await render(<UpdateRequiredView />);
            expect(useAppLockStore.getState().locked).toBe(false);
        });

        it("signed out: cleared again when the app locks while it is up", async () => {
            await render(<UpdateRequiredView />);
            // back from the background after the idle timeout
            updateStores(() => useAppLockStore.setState({ locked: true }));
            expect(useAppLockStore.getState().locked).toBe(false);
            expect(screen.getByText("Update required")).toBeTruthy();
        });

        it("signed in: kept, so the lock screen covers the user's page", async () => {
            useAuthStore.setState({ isAuthenticated: true });
            useAppLockStore.setState({ locked: true });
            await render(<UpdateRequiredView />);
            expect(useAppLockStore.getState().locked).toBe(true);
        });
    });
});

describe("UpdateRequiredLanding", () => {
    it("native: the landing page's text and the app's header, with no way in", async () => {
        if (Platform.OS === "web") return;
        await render(<UpdateRequiredLanding />);
        expect(
            screen.getByText("Access your health information online")
        ).toBeTruthy();
        expect(screen.getByTestId("brandHeader")).toBeTruthy();
        expect(screen.queryByTestId("btnLogin")).toBeNull();
    });
});
