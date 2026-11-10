import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { AppState, Platform, Text } from "react-native";

import { AppModal } from "@/components/common/AppModal";
import { vault } from "@/services/vault";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, updateStores } from "@/test/fixtures";
import { advanceTimers, press, render, screen } from "@/test/ui";
import { LockGate, LockScreen } from "@/views/native/appLock/LockScreen";

jest.mock("@/services/vault", () => ({
    vault: {
        unlock: jest.fn(),
        lock: jest.fn(),
        destroy: jest.fn(async () => undefined),
    },
}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

const authenticate = jest.mocked(vault.unlock);
const isWeb = Platform.OS === "web";

/** Jest's React Native mock records the listener the screen registered. */
const comesToFront = () => {
    const listener = jest.mocked(AppState.addEventListener).mock.lastCall?.[1];
    listener?.("active");
};

describe("LockScreen", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        // undefined in Jest's React Native mock
        AppState.currentState = "active";
        authenticate.mockReset();
        authenticate.mockResolvedValue("unlocked");
        useAppLockStore.setState({ locked: true, authenticating: false });
        // loaded before the lock can show: its button names the only provider
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });

    afterEach(async () => {
        updateStores(() => {
            useAppLockStore.setState({ locked: false, authenticating: false });
            useConfigStore.setState({ config: undefined, status: "idle" });
        });
        jest.useRealTimers();
    });

    it("shows nothing while unlocked", async () => {
        useAppLockStore.setState({ locked: false });
        await render(<LockScreen />);
        expect(screen.queryByTestId("lockScreen")).toBeNull();
    });

    if (isWeb) {
        it("never locks on web", async () => {
            await render(<LockScreen />);
            await advanceTimers(1000);
            expect(screen.queryByTestId("lockScreen")).toBeNull();
            expect(authenticate).not.toHaveBeenCalled();
        });
        return;
    }

    it("asks by itself shortly after appearing and lifts on success", async () => {
        await render(<LockScreen />);
        expect(screen.getByTestId("lockScreen")).toBeTruthy();
        expect(authenticate).not.toHaveBeenCalled();

        await advanceTimers(500);
        expect(authenticate).toHaveBeenCalledTimes(1);
        expect(useAppLockStore.getState().locked).toBe(false);
        expect(screen.queryByTestId("lockScreen")).toBeNull();
    });

    it("waits until the app is in front to ask", async () => {
        AppState.currentState = "inactive";
        await render(<LockScreen />);
        await advanceTimers(500);
        expect(authenticate).not.toHaveBeenCalled();

        AppState.currentState = "active";
        comesToFront();
        await advanceTimers(500);
        expect(authenticate).toHaveBeenCalledTimes(1);
    });

    it("looks like the landing page, not the Protect page, with no button for the prompt", async () => {
        await render(<LockScreen />);
        expect(
            screen.getByText("Access your health information online")
        ).toBeTruthy();
        expect(screen.getByText("Log in with BC Services Card")).toBeTruthy();
        expect(screen.queryByTestId("localAuthBtn")).toBeNull();
        expect(
            screen.queryByText("Protect your personal information")
        ).toBeNull();
    });

    it("after a failed prompt stays locked and doesn't ask again by itself", async () => {
        authenticate.mockResolvedValue("cancelled");
        await render(<LockScreen />);
        await advanceTimers(500);
        expect(useAppLockStore.getState().locked).toBe(true);
        expect(screen.getByTestId("lockLogInAgainBtn")).toBeTruthy();

        // the system prompt closing brings the app back to the front
        comesToFront();
        await advanceTimers(500);
        expect(authenticate).toHaveBeenCalledTimes(1);
    });

    it("the full login signs out first", async () => {
        const signOut = jest.fn(async () => undefined);
        useAuthStore.setState({ signOut });
        authenticate.mockResolvedValue("cancelled");
        await render(<LockScreen />);
        await advanceTimers(500);

        await press(screen.getByTestId("lockLogInAgainBtn"));
        expect(signOut).toHaveBeenCalledTimes(1);
        // a tap on this button is a request to log in, unlike a session that just ended
        expect(useAuthStore.getState().loginRequest).toEqual({});
        useAuthStore.setState({ loginRequest: undefined });
    });

    it("hides the app from screen readers while locked", async () => {
        await render(
            <LockGate>
                <Text accessibilityLabel="app">app</Text>
            </LockGate>
        );
        expect(screen.queryByText("app")).toBeNull();
        updateStores(() => useAppLockStore.setState({ locked: false }));
        expect(screen.getByText("app")).toBeTruthy();
    });

    it("hides an open modal while locked and brings it back", async () => {
        await render(
            <AppModal visible>
                <Text>open dialog</Text>
            </AppModal>
        );
        expect(screen.queryByText("open dialog")).toBeNull();
        updateStores(() => useAppLockStore.setState({ locked: false }));
        expect(screen.getByText("open dialog")).toBeTruthy();
    });
});
