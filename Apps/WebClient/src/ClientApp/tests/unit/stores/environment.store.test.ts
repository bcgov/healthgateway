import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import {
    defaultEnvironmentId,
    environments,
    selectableEnvironments,
} from "@/config/environments";
import { services } from "@/services/container";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { savedConfigKey } from "@/stores/config.store";
import {
    selectEnvironment,
    useEnvironmentStore,
} from "@/stores/environment.store";
import { useOnboardingStore } from "@/stores/onboarding.store";
import { switchEnvironment } from "@/stores/switchEnvironment";
import { useThemeStore } from "@/stores/theme.store";
import { configFixture, fakeAuth } from "@/test/fixtures";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(async () => undefined),
}));

const dev = globalThis as unknown as { __DEV__: boolean };
const KEY = "hg.environment";
const state = () => useEnvironmentStore.getState();

describe("environments", () => {
    beforeEach(async () => {
        await AsyncStorage.clear();
        useEnvironmentStore.setState({ selected: undefined, restarts: 0 });
    });

    afterEach(() => {
        dev.__DEV__ = true;
    });

    it("defaults to Development locally and Production otherwise", () => {
        expect(defaultEnvironmentId()).toBe("dev");
        dev.__DEV__ = false;
        expect(defaultEnvironmentId()).toBe("prod");
    });

    it("offers all four locally, and no Local in a release build", () => {
        expect(selectableEnvironments().map((e) => e.name)).toEqual([
            "Local",
            "Development",
            "Test",
            "Production",
        ]);
        dev.__DEV__ = false;
        expect(selectableEnvironments().map((e) => e.id)).toEqual([
            "dev",
            "test",
            "prod",
        ]);
    });

    it("has a banner name for all but Production", () => {
        expect(environments.map((e) => e.banner)).toEqual([
            "Local Development",
            "Development",
            "Test",
            undefined,
        ]);
    });

    it("restores a saved choice", async () => {
        await AsyncStorage.setItem(KEY, "test");
        await state().hydrate();
        expect(selectEnvironment(state()).id).toBe("test");
    });

    it("ignores an unknown saved choice", async () => {
        await AsyncStorage.setItem(KEY, "staging");
        await state().hydrate();
        expect(selectEnvironment(state()).id).toBe("dev");
    });

    it("a release build never honours a saved Local", async () => {
        await AsyncStorage.setItem(KEY, "local");
        dev.__DEV__ = false;
        await state().hydrate();
        expect(selectEnvironment(state()).id).toBe("prod");
    });

    describe("switchEnvironment", () => {
        const signOut = jest.fn(async () => undefined);
        const leavePage = jest.fn();

        beforeEach(() => {
            jest.clearAllMocks();
            services.initialize(configFixture, fakeAuth());
            useAuthStore.setState({ signOut });
        });

        it("resets the app like Reset App, then saves the choice and restarts", async () => {
            await AsyncStorage.setItem(savedConfigKey(), "{}");
            await AsyncStorage.setItem("hg.onboarding.securityOffered", "true");
            useThemeStore.setState({ preference: "dark" });
            await switchEnvironment("test", leavePage);
            expect(signOut).toHaveBeenCalledTimes(1);
            expect(await AsyncStorage.getItem(savedConfigKey())).toBeNull();
            expect(useThemeStore.getState().preference).toBe("light");
            // the intro comes back
            expect(
                useOnboardingStore.getState().screens.length
            ).toBeGreaterThan(0);
            expect(useOnboardingStore.getState().securityOffered).toBe(false);
            expect(useAppLockStore.getState().enabled).toBe(false);
            expect(await AsyncStorage.getItem(KEY)).toBe("test");
            expect(selectEnvironment(state()).id).toBe("test");
            expect(leavePage).toHaveBeenCalledTimes(1);
            expect(state().restarts).toBe(1);
        });

        it("still works when the app never started", async () => {
            jest.spyOn(services, "isInitialized", "get").mockReturnValue(false);
            await switchEnvironment("test", leavePage);
            expect(signOut).not.toHaveBeenCalled();
            expect(state().restarts).toBe(1);
            jest.restoreAllMocks();
        });

        it("does nothing when choosing the current one", async () => {
            await switchEnvironment("dev", leavePage);
            expect(signOut).not.toHaveBeenCalled();
            expect(state().restarts).toBe(0);
        });
    });
});
