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
    environments,
    environmentsOf,
    isEnvironmentId,
} from "@/config/environments";
import { clearStoredSession } from "@/services/auth/storedSession";
import { services } from "@/services/container";
import { fetchReleasedBuild } from "@/services/environments.service";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { savedConfigKey } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import {
    selectEnvironment,
    selectHasEnvironmentChoice,
    useEnvironmentStore,
} from "@/stores/environment.store";
import { useOnboardingStore } from "@/stores/onboarding.store";
import {
    settleEnvironment,
    switchEnvironment,
} from "@/stores/switchEnvironment";
import { useThemeStore } from "@/stores/theme.store";
import { configFixture, fakeAuth } from "@/test/fixtures";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(async () => undefined),
}));
// the real one, so a test can make deleting the stored login fail
jest.mock("@/services/auth/storedSession", () => {
    const actual = jest.requireActual<
        typeof import("@/services/auth/storedSession")
    >("@/services/auth/storedSession");
    return {
        ...actual,
        clearStoredSession: jest.fn(actual.clearStoredSession),
    };
});
jest.mock("@/services/environments.service", () => ({
    fetchReleasedBuild: jest.fn(),
}));
// this app's build number, as app.config.ts bakes it in
const mockExtra: { build?: unknown } = { build: 452 };
jest.mock("expo-constants", () => ({
    __esModule: true,
    default: {
        get expoConfig() {
            return { version: "3.0.0", extra: mockExtra };
        },
    },
}));

const released = jest.mocked(fetchReleasedBuild);
const dev = globalThis as unknown as { __DEV__: boolean };
const KEY = "hg.environment";
const LAST_USED_KEY = "hg.environment.lastUsed";
const state = () => useEnvironmentStore.getState();
const reset = () =>
    useEnvironmentStore.setState({
        selected: undefined,
        offered: ["local", "dev", "test", "prod"],
        effective: "local",
        lastUsed: "local",
        restarts: 0,
    });

describe("environments", () => {
    beforeEach(async () => {
        await AsyncStorage.clear();
        released.mockReset();
        mockExtra.build = 452;
        useConnectivityStore.setState({ isOnline: true });
        reset();
    });

    afterEach(() => {
        dev.__DEV__ = true;
    });

    it("has a banner name for all but Production", () => {
        expect(environments.map((e) => e.banner)).toEqual([
            "Local Development",
            "Development",
            "Test",
            undefined,
        ]);
    });

    it("lists environments in the selector's order, whatever order they're asked for in", () => {
        expect(environmentsOf(["prod", "test"]).map((e) => e.id)).toEqual([
            "test",
            "prod",
        ]);
        expect(isEnvironmentId("test")).toBe(true);
        expect(isEnvironmentId("staging")).toBe(false);
    });

    describe("a development build", () => {
        it("offers every environment and starts on Local, without asking for the released build", async () => {
            await state().hydrate();
            expect(state().offered).toEqual(["local", "dev", "test", "prod"]);
            expect(selectEnvironment(state()).id).toBe("local");
            expect(released).not.toHaveBeenCalled();
        });

        it("restores a saved choice", async () => {
            await AsyncStorage.setItem(KEY, "test");
            await state().hydrate();
            expect(selectEnvironment(state()).id).toBe("test");
        });

        it("ignores an unknown saved choice", async () => {
            await AsyncStorage.setItem(KEY, "staging");
            await state().hydrate();
            expect(selectEnvironment(state()).id).toBe("local");
        });
    });

    describe("a release build (this one is build 452)", () => {
        beforeEach(() => {
            dev.__DEV__ = false;
        });

        describe("the released build answers", () => {
            it("above it (TestFlight, Play testing, review): Development, Test and Production, starting on Test", async () => {
                released.mockResolvedValue(451);
                await state().hydrate();
                expect(state().offered).toEqual(["dev", "test", "prod"]);
                expect(selectEnvironment(state()).id).toBe("test");
            });

            it("above it, with a choice: the choice", async () => {
                released.mockResolvedValue(451);
                await AsyncStorage.setItem(KEY, "dev");
                await state().hydrate();
                expect(selectEnvironment(state()).id).toBe("dev");
            });

            it.each([
                ["the released build", 452],
                ["older", 460],
            ])(
                "%s: Production only, and a choice no longer offered is forgotten",
                async (_name, releasedBuild) => {
                    released.mockResolvedValue(releasedBuild);
                    await AsyncStorage.setItem(KEY, "test");
                    await AsyncStorage.setItem(LAST_USED_KEY, "test");
                    await state().hydrate();
                    expect(state().offered).toEqual(["prod"]);
                    expect(selectEnvironment(state()).id).toBe("prod");
                    expect(state().selected).toBeUndefined();
                    expect(await AsyncStorage.getItem(KEY)).toBeNull();
                    // where it ran before, so start-up resets the app
                    expect(state().lastUsed).toBe("test");
                }
            );

            it("none released (null): Production only", async () => {
                released.mockResolvedValue(null);
                await state().hydrate();
                expect(state().offered).toEqual(["prod"]);
            });

            it("no build number in this app: Production only", async () => {
                mockExtra.build = undefined;
                released.mockResolvedValue(0);
                await state().hydrate();
                expect(state().offered).toEqual(["prod"]);
            });

            it("never honours a saved Local", async () => {
                released.mockResolvedValue(451);
                await AsyncStorage.setItem(KEY, "local");
                await state().hydrate();
                expect(selectEnvironment(state()).id).toBe("test");
            });
        });

        describe("no answer: it stays where it was, so a flaky network can't wipe a tester's data", () => {
            beforeEach(() => {
                released.mockRejectedValue(new Error("429"));
            });

            it("on its choice, with Production as the way back", async () => {
                await AsyncStorage.setItem(KEY, "dev");
                await state().hydrate();
                expect(state().offered).toEqual(["dev", "prod"]);
                expect(selectEnvironment(state()).id).toBe("dev");
            });

            it("on the default it last ran on (no choice)", async () => {
                await AsyncStorage.setItem(LAST_USED_KEY, "test");
                await state().hydrate();
                expect(state().offered).toEqual(["test", "prod"]);
                expect(selectEnvironment(state()).id).toBe("test");
                expect(state().lastUsed).toBe("test");
            });

            it("otherwise Production only, with no selector", async () => {
                await state().hydrate();
                expect(state().offered).toEqual(["prod"]);
                expect(selectEnvironment(state()).id).toBe("prod");
            });

            it("offline: doesn't ask", async () => {
                useConnectivityStore.setState({ isOnline: false });
                await AsyncStorage.setItem(LAST_USED_KEY, "test");
                await state().hydrate();
                expect(released).not.toHaveBeenCalled();
                expect(selectEnvironment(state()).id).toBe("test");
            });
        });

        it("an install from before lastUsed was saved ran on its choice", async () => {
            released.mockResolvedValue(451);
            await AsyncStorage.setItem(KEY, "dev");
            await state().hydrate();
            expect(state().lastUsed).toBe("dev");
        });

        describe("installed over a development build (both are hg.local, so its storage stays)", () => {
            beforeEach(async () => {
                await AsyncStorage.setItem(KEY, "local");
                await AsyncStorage.setItem(LAST_USED_KEY, "local");
            });

            it("knows it last ran on Local, so start-up resets it", async () => {
                released.mockResolvedValue(452);
                await state().hydrate();
                expect(selectEnvironment(state()).id).toBe("prod");
                expect(state().lastUsed).toBe("local");
            });

            it("with no answer: Production, never Local", async () => {
                released.mockRejectedValue(new Error("offline"));
                await state().hydrate();
                expect(state().offered).toEqual(["prod"]);
                expect(selectEnvironment(state()).id).toBe("prod");
                expect(state().lastUsed).toBe("local");
            });
        });

        it("an install from before lastUsed was saved: records where it ran before forgetting the choice, so a reset that fails runs again on the next start", async () => {
            jest.spyOn(services, "isInitialized", "get").mockReturnValue(false);
            // build 452 is released; the install only has its choice, Test
            released.mockResolvedValue(452);
            await AsyncStorage.setItem(KEY, "test");
            await state().hydrate();
            expect(await AsyncStorage.getItem(KEY)).toBeNull();
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("test");

            // the stored login can't be deleted: the reset stops before clearing anything
            jest.mocked(clearStoredSession).mockRejectedValueOnce(
                new Error("keychain")
            );
            await expect(settleEnvironment()).rejects.toThrow("keychain");
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("test");

            // the next start still sees Test, and resets
            reset();
            await state().hydrate();
            expect(state().lastUsed).toBe("test");
            await settleEnvironment();
            expect(clearStoredSession).toHaveBeenCalledTimes(2);
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("prod");
            jest.restoreAllMocks();
        });

        it("a fresh install last ran on Production", async () => {
            released.mockResolvedValue(451);
            await state().hydrate();
            expect(state().lastUsed).toBe("prod");
        });
    });

    it("the selector shows only for several environments", () => {
        expect(selectHasEnvironmentChoice({ offered: ["prod"] })).toBe(false);
        expect(selectHasEnvironmentChoice({ offered: ["test", "prod"] })).toBe(
            true
        );
    });

    it("a choice is saved as the choice and the last used", async () => {
        await state().select("test");
        expect(await AsyncStorage.getItem(KEY)).toBe("test");
        expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("test");
        expect(selectEnvironment(state()).id).toBe("test");
    });

    describe("settleEnvironment (start-up)", () => {
        beforeEach(() => {
            jest.spyOn(services, "isInitialized", "get").mockReturnValue(false);
        });

        afterEach(() => {
            jest.restoreAllMocks();
        });

        it("on the same environment as last time: keeps everything and saves it as the last used", async () => {
            useEnvironmentStore.setState({
                effective: "test",
                lastUsed: "test",
            });
            // the saved configuration's key names the environment
            await AsyncStorage.setItem(savedConfigKey(), "{}");
            await settleEnvironment();
            expect(await AsyncStorage.getItem(savedConfigKey())).toBe("{}");
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("test");
        });

        it("on another (a build was just released): resets the app first, so nothing carries across", async () => {
            // Test's saved configuration, from the last run
            useEnvironmentStore.setState({ effective: "test" });
            const testKey = savedConfigKey();
            await AsyncStorage.setItem(testKey, "{}");
            useThemeStore.setState({ preference: "dark" });
            useEnvironmentStore.setState({
                effective: "prod",
                lastUsed: "test",
            });
            await settleEnvironment();
            expect(await AsyncStorage.getItem(testKey)).toBeNull();
            expect(useThemeStore.getState().preference).toBe("light");
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("prod");
            expect(state().lastUsed).toBe("prod");
        });

        it("keeps a choice the reset cleared from storage", async () => {
            useEnvironmentStore.setState({
                selected: "dev",
                effective: "dev",
                lastUsed: "test",
            });
            await settleEnvironment();
            expect(await AsyncStorage.getItem(KEY)).toBe("dev");
        });
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
            expect(await AsyncStorage.getItem(LAST_USED_KEY)).toBe("test");
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
            // a development build starts on Local
            await switchEnvironment("local", leavePage);
            expect(signOut).not.toHaveBeenCalled();
            expect(state().restarts).toBe(0);
        });
    });
});
