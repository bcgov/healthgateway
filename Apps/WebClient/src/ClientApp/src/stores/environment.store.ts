import AsyncStorage from "@react-native-async-storage/async-storage";
import { Platform } from "react-native";
import { create } from "zustand";

import {
    defaultEnvironmentId,
    type Environment,
    type EnvironmentId,
    environments,
    selectableEnvironments,
} from "@/config/environments";
import { describeError, logger } from "@/services/logger";

const STORAGE_KEY = "hg.environment";

interface EnvironmentState {
    /** the user's choice; undefined means the build's default */
    selected?: EnvironmentId;
    /** raises when the app must start again on a newly chosen environment */
    restarts: number;
    hydrate: () => Promise<void>;
    select: (id: EnvironmentId) => Promise<void>;
    requestRestart: () => void;
}

let revision = 0;

export const useEnvironmentStore = create<EnvironmentState>((set) => ({
    selected: undefined,
    restarts: 0,
    hydrate: async () => {
        if (Platform.OS === "web") return;
        try {
            revision += 1;
            const started = revision;
            const stored = await AsyncStorage.getItem(STORAGE_KEY);
            // a newer read or a choice came in while this waited: this one's answer is out of date
            if (started !== revision) return;
            // only what this build offers: a release build never honours a stored Local
            const valid = selectableEnvironments().find((e) => e.id === stored);
            set({ selected: valid?.id });
        } catch (error) {
            logger.warn(
                `Could not read the environment: ${describeError(error)}`
            );
        }
    },
    select: async (id) => {
        revision += 1;
        set({ selected: id });
        try {
            await AsyncStorage.setItem(STORAGE_KEY, id);
        } catch (error) {
            logger.warn(
                `Could not save the environment: ${describeError(error)}`
            );
        }
    },
    requestRestart: () => set((s) => ({ restarts: s.restarts + 1 })),
}));

export const selectEnvironment = (s: { selected?: EnvironmentId }) =>
    environments.find(
        (e) => e.id === (s.selected ?? defaultEnvironmentId())
    ) as Environment;
