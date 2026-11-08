import AsyncStorage from "@react-native-async-storage/async-storage";
import { create } from "zustand";

import { describeError, logger } from "@/services/logger";

export type ThemePreference = "system" | "light" | "dark";

const STORAGE_KEY = "hg.theme.preference";
const preferences: ThemePreference[] = ["system", "light", "dark"];

interface ThemeState {
    /** Light is the default; "dark" and "system" (follow the OS appearance) are explicit user choices. */
    preference: ThemePreference;
    hydrated: boolean;
    /** Loads the persisted preference once; safe to call repeatedly. */
    hydrate: () => Promise<void>;
    setPreference: (preference: ThemePreference) => Promise<void>;
}

export const isThemePreference = (value: unknown): value is ThemePreference =>
    preferences.includes(value as ThemePreference);

let revision = 0;

export const useThemeStore = create<ThemeState>((set, get) => ({
    preference: "light",
    hydrated: false,
    hydrate: async () => {
        if (get().hydrated) return;
        try {
            revision += 1;
            const started = revision;
            const stored = await AsyncStorage.getItem(STORAGE_KEY);
            // chosen while this waited: the choice stands
            if (started !== revision) {
                set({ hydrated: true });
                return;
            }
            set({
                preference: isThemePreference(stored) ? stored : "light",
                hydrated: true,
            });
        } catch (error) {
            logger.warn(
                `Could not read the theme preference: ${describeError(error)}`
            );
            set({ hydrated: true });
        }
    },
    setPreference: async (preference) => {
        revision += 1;
        set({ preference });
        try {
            await AsyncStorage.setItem(STORAGE_KEY, preference);
        } catch (error) {
            logger.warn(
                `Could not save the theme preference: ${describeError(error)}`
            );
        }
    },
}));
