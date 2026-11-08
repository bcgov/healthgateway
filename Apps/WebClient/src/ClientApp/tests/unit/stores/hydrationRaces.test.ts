import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";
import * as SecureStore from "expo-secure-store";

import { useAppLockStore } from "@/stores/appLock.store";
import { useEnvironmentStore } from "@/stores/environment.store";
import { useOnboardingStore } from "@/stores/onboarding.store";
import { useThemeStore } from "@/stores/theme.store";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(async () => undefined),
    deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock("@/services/vault", () => ({
    vault: {
        unlock: jest.fn(),
        lock: jest.fn(),
        destroy: jest.fn(async () => undefined),
    },
}));

/** Holds the next read from storage until told what it found. */
const holdNextRead = () => {
    let found: (value: string | null) => void = () => undefined;
    jest.spyOn(AsyncStorage, "getItem").mockImplementationOnce(
        () =>
            new Promise((resolve) => {
                found = resolve;
            })
    );
    return (value: string | null) => found(value);
};

describe("a slow read from storage does not undo what came after it", () => {
    beforeEach(() => {
        jest.restoreAllMocks();
    });

    it("an environment chosen during hydration stands", async () => {
        const found = holdNextRead();
        const hydrating = useEnvironmentStore.getState().hydrate();
        await useEnvironmentStore.getState().select("test");
        found("development");
        await hydrating;
        expect(useEnvironmentStore.getState().selected).toBe("test");
    });

    it("of two hydrations of the environment, the newer stands", async () => {
        const older = holdNextRead();
        const first = useEnvironmentStore.getState().hydrate();
        const newer = holdNextRead();
        const second = useEnvironmentStore.getState().hydrate();
        newer("test");
        await second;
        older("development");
        await first;
        expect(useEnvironmentStore.getState().selected).toBe("test");
    });

    it("a theme chosen during hydration stands", async () => {
        const found = holdNextRead();
        const hydrating = useThemeStore.getState().hydrate();
        await useThemeStore.getState().setPreference("dark");
        found("light");
        await hydrating;
        expect(useThemeStore.getState().preference).toBe("dark");
        expect(useThemeStore.getState().hydrated).toBe(true);
    });

    it("the lock reset during hydration stays off", async () => {
        let found: (value: string | null) => void = () => undefined;
        jest.mocked(SecureStore.getItemAsync).mockReturnValueOnce(
            new Promise((resolve) => {
                found = resolve;
            })
        );
        const hydrating = useAppLockStore.getState().hydrate();
        await useAppLockStore.getState().reset();
        found("true");
        await hydrating;
        expect(useAppLockStore.getState().enabled).toBe(false);
    });

    it("the security offer finished during hydration stays finished", async () => {
        const seen = holdNextRead();
        const hydrating = useOnboardingStore.getState().hydrate(false);
        await useOnboardingStore.getState().completeSecurityOffer();
        seen(null);
        await hydrating;
        expect(useOnboardingStore.getState().securityOffered).toBe(true);
    });
});
