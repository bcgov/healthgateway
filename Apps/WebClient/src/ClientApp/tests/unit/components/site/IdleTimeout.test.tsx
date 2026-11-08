import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { IdleTimeout } from "@/components/site/IdleTimeout";
import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useConfigStore } from "@/stores/config.store";
import { router } from "@/test/expoRouterMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { advanceTimers, render, screen } from "@/test/ui";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

const IDLE = configFixture.webClient.timeouts.idle;
const config = { ...configFixture, mobile: { timeouts: { idle: IDLE } } };
const COUNTDOWN = 60000;

describe("IdleTimeout", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        jest.mocked(router.push).mockClear();
        signInRegistered();
        useConfigStore.setState({ config });
        useAppLockStore.setState({ enabled: false, locked: false });
    });

    afterEach(() => {
        updateStores(() =>
            useAppLockStore.setState({ enabled: false, locked: false })
        );
        resetStores();
        jest.useRealTimers();
    });

    it("without device security: warns, then logs out, which removes the tokens", async () => {
        await render(<IdleTimeout />);
        await advanceTimers(IDLE + 1000);
        expect(screen.getByText("Are you still there?")).toBeTruthy();
        expect(useAppLockStore.getState().locked).toBe(false);

        await advanceTimers(COUNTDOWN + 1000);
        expect(router.push).toHaveBeenCalledWith(Path.Logout);
    });

    it("with device security: locks instead, with no warning and no logout", async () => {
        useAppLockStore.setState({ enabled: true });
        await render(<IdleTimeout />);
        await advanceTimers(IDLE + 1000);
        expect(useAppLockStore.getState().locked).toBe(true);
        expect(screen.queryByText("Are you still there?")).toBeNull();

        await advanceTimers(COUNTDOWN + 1000);
        expect(router.push).not.toHaveBeenCalled();
    });

    it("native uses mobile.timeouts.idle, web the web client's", async () => {
        useConfigStore.setState({
            config: { ...configFixture, mobile: { timeouts: { idle: 2000 } } },
        });
        await render(<IdleTimeout />);
        await advanceTimers(3000);
        const dialog = screen.queryByText("Are you still there?");
        if (Platform.OS === "web") expect(dialog).toBeNull();
        else expect(dialog).toBeTruthy();
    });

    it("does not run behind the lock, and starts again after unlocking", async () => {
        useAppLockStore.setState({ enabled: true, locked: true });
        await render(<IdleTimeout />);
        await advanceTimers(IDLE * 3);
        expect(router.push).not.toHaveBeenCalled();

        updateStores(() => useAppLockStore.setState({ locked: false }));
        await advanceTimers(IDLE - 1000);
        expect(useAppLockStore.getState().locked).toBe(false);
        await advanceTimers(2000);
        expect(useAppLockStore.getState().locked).toBe(true);
    });
});
