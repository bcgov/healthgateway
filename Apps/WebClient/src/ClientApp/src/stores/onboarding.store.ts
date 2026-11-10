import AsyncStorage from "@react-native-async-storage/async-storage";
import { Platform } from "react-native";
import { create } from "zustand";

import {
    type OnboardingScreen,
    onboardingScreens,
} from "@/constants/onboarding";
import { describeError, logger } from "@/services/logger";

const SEEN_KEY = "hg.onboarding.seen";
const SECURITY_OFFERED_KEY = "hg.onboarding.securityOffered";

interface SeenScreen {
    type: string;
    version: number;
}

interface OnboardingState {
    /** Tutorial screens this user has not seen at their latest version. */
    screens: OnboardingScreen[];
    /** The "Protect your personal information" step has been answered. */
    securityOffered: boolean;
    /** Loads what was seen. Native only: the web client has no tutorial, so nothing is ever pending there. */
    hydrate: (lockEnabled: boolean) => Promise<void>;
    completeTutorial: () => Promise<void>;
    completeSecurityOffer: () => Promise<void>;
}

const parseSeen = (raw: string | null): SeenScreen[] => {
    if (!raw) return [];
    try {
        const parsed: unknown = JSON.parse(raw);
        return Array.isArray(parsed) ? (parsed as SeenScreen[]) : [];
    } catch {
        return [];
    }
};

let revision = 0;

export const useOnboardingStore = create<OnboardingState>((set, get) => ({
    screens: [],
    securityOffered: true,

    hydrate: async (lockEnabled) => {
        if (Platform.OS === "web") return;
        try {
            revision += 1;
            const started = revision;
            const [seen, offered] = await Promise.all([
                AsyncStorage.getItem(SEEN_KEY).then(parseSeen),
                AsyncStorage.getItem(SECURITY_OFFERED_KEY),
            ]);
            // read again, or finished, while this waited: this answer is out of date
            if (started !== revision) return;
            set({
                screens: onboardingScreens.filter(
                    (screen) =>
                        !seen.some(
                            (s) =>
                                s.type === screen.type &&
                                s.version === screen.version
                        )
                ),
                securityOffered: offered === "true" || lockEnabled,
            });
        } catch (error) {
            // never hold the app back for a tutorial
            logger.warn(
                `Could not read onboarding state: ${describeError(error)}`
            );
        }
    },

    completeTutorial: async () => {
        revision += 1;
        const shown = get().screens;
        set({ screens: [] });
        try {
            const seen = parseSeen(await AsyncStorage.getItem(SEEN_KEY));
            const visits = shown.map(({ type, version }) => ({
                type,
                version,
            }));
            await AsyncStorage.setItem(
                SEEN_KEY,
                JSON.stringify([...seen, ...visits])
            );
        } catch (error) {
            logger.warn(
                `Could not save onboarding state: ${describeError(error)}`
            );
        }
    },

    completeSecurityOffer: async () => {
        revision += 1;
        set({ securityOffered: true });
        try {
            await AsyncStorage.setItem(SECURITY_OFFERED_KEY, "true");
        } catch (error) {
            logger.warn(
                `Could not save onboarding state: ${describeError(error)}`
            );
        }
    },
}));

export const selectOnboardingPending = (state: OnboardingState) =>
    state.screens.length > 0 || !state.securityOffered;
