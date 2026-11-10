import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";
import * as SecureStore from "expo-secure-store";
import { Alert } from "react-native";

import { onboardingScreens } from "@/constants/onboarding";
import { services } from "@/services/container";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectOnboardingPending,
    useOnboardingStore,
} from "@/stores/onboarding.store";
import { useThemeStore } from "@/stores/theme.store";
import { configFixture, fakeAuth } from "@/test/fixtures";
import { resetApp, runReset } from "@/utils/resetApp";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(async () => undefined),
    deleteItemAsync: jest.fn(async () => undefined),
}));

describe("resetApp", () => {
    const signOut = jest.fn(async () => undefined);
    const leavePage = jest.fn();

    beforeEach(async () => {
        services.initialize(configFixture, fakeAuth());
        signOut.mockClear();
        leavePage.mockClear();
        jest.mocked(SecureStore.deleteItemAsync).mockClear();
        useAuthStore.setState({ signOut });
        useAppLockStore.setState({ enabled: true, locked: false });
        useThemeStore.setState({ preference: "dark" });
        useOnboardingStore.setState({ screens: [], securityOffered: true });
        await AsyncStorage.setItem("hg.theme.preference", "dark");
        await AsyncStorage.setItem("hg.onboarding.securityOffered", "true");
        await AsyncStorage.setItem(
            "hg.onboarding.seen",
            JSON.stringify(
                onboardingScreens.map(({ type, version }) => ({
                    type,
                    version,
                }))
            )
        );
    });

    it("signs out, which removes the tokens", async () => {
        await resetApp(leavePage);
        expect(signOut).toHaveBeenCalledTimes(1);
        expect(leavePage).toHaveBeenCalledTimes(1);
    });

    it("deletes the stored login even when the app never started", async () => {
        jest.spyOn(services, "isInitialized", "get").mockReturnValueOnce(false);
        await resetApp(leavePage);
        expect(signOut).not.toHaveBeenCalled();
        expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith(
            "hg.oidc.session"
        );
    });

    it("brings the intro and the security step back", async () => {
        await resetApp(leavePage);
        const state = useOnboardingStore.getState();
        expect(state.screens).toHaveLength(onboardingScreens.length);
        expect(state.securityOffered).toBe(false);
        expect(selectOnboardingPending(state)).toBe(true);
    });

    it("forgets the lock and the saved settings", async () => {
        await resetApp(leavePage);
        expect(useAppLockStore.getState().enabled).toBe(false);
        expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith(
            "hg.appLock.enabled"
        );
        expect(useThemeStore.getState().preference).toBe("light");
        expect(await AsyncStorage.getItem("hg.theme.preference")).toBeNull();
        expect(await AsyncStorage.getItem("hg.onboarding.seen")).toBeNull();
    });

    it("leaves the signed-in page before the intro takes over", async () => {
        leavePage.mockImplementation(() => {
            expect(selectOnboardingPending(useOnboardingStore.getState())).toBe(
                false
            );
        });
        await resetApp(leavePage);
        expect(leavePage).toHaveBeenCalled();
    });

    it("stops before clearing anything when the stored login cannot be deleted", async () => {
        jest.mocked(SecureStore.deleteItemAsync).mockRejectedValueOnce(
            new Error("keychain")
        );
        await expect(resetApp(leavePage)).rejects.toThrow("keychain");
        expect(leavePage).not.toHaveBeenCalled();
        expect(useAppLockStore.getState().enabled).toBe(true);
        expect(useThemeStore.getState().preference).toBe("dark");
        expect(await AsyncStorage.getItem("hg.theme.preference")).toBe("dark");
    });

    it("tells the user when a reset from a button fails", async () => {
        const alert = jest
            .spyOn(Alert, "alert")
            .mockImplementation(() => undefined);
        runReset(() => Promise.reject(new Error("keychain")));
        await new Promise((resolve) => setImmediate(resolve));
        expect(alert).toHaveBeenCalledWith(
            "Reset failed",
            "The app could not be reset. Please try again."
        );
        alert.mockRestore();
    });

    it("still finishes when saved data cannot be cleared", async () => {
        jest.spyOn(AsyncStorage, "clear").mockRejectedValueOnce(
            new Error("locked")
        );
        await resetApp(leavePage);
        expect(useAppLockStore.getState().enabled).toBe(false);
        expect(useThemeStore.getState().preference).toBe("light");
    });
});
