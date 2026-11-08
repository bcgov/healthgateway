import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { isThemePreference, useThemeStore } from "@/stores/theme.store";

describe("theme store", () => {
    beforeEach(async () => {
        await AsyncStorage.clear();
        useThemeStore.setState({ preference: "light", hydrated: false });
    });

    it("defaults to light", async () => {
        await useThemeStore.getState().hydrate();
        expect(useThemeStore.getState().preference).toBe("light");
        expect(useThemeStore.getState().hydrated).toBe(true);
    });

    it("persists an explicit choice and restores it", async () => {
        await useThemeStore.getState().setPreference("dark");
        expect(await AsyncStorage.getItem("hg.theme.preference")).toBe("dark");

        useThemeStore.setState({ preference: "light", hydrated: false });
        await useThemeStore.getState().hydrate();
        expect(useThemeStore.getState().preference).toBe("dark");
    });

    it("ignores an unknown stored value", async () => {
        await AsyncStorage.setItem("hg.theme.preference", "sepia");
        await useThemeStore.getState().hydrate();
        expect(useThemeStore.getState().preference).toBe("light");
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("hydrates only once", async () => {
        const read = jest.spyOn(AsyncStorage, "getItem");
        read.mockClear(); // the AsyncStorage mock is shared and keeps calls from earlier tests
        await useThemeStore.getState().hydrate();
        await useThemeStore.getState().hydrate();
        expect(read).toHaveBeenCalledTimes(1);
    });

    it("still finishes hydrating when storage cannot be read", async () => {
        jest.spyOn(AsyncStorage, "getItem").mockRejectedValueOnce(
            new Error("locked")
        );
        await useThemeStore.getState().hydrate();
        expect(useThemeStore.getState()).toMatchObject({
            hydrated: true,
            preference: "light",
        });
    });

    it("applies a choice even when it cannot be saved", async () => {
        jest.spyOn(AsyncStorage, "setItem").mockRejectedValueOnce(
            new Error("full")
        );
        await useThemeStore.getState().setPreference("system");
        expect(useThemeStore.getState().preference).toBe("system");
    });

    it("validates preference values", () => {
        expect(isThemePreference("light")).toBe(true);
        expect(isThemePreference("blue")).toBe(false);
        expect(isThemePreference(null)).toBe(false);
    });
});
