import AsyncStorage from "@react-native-async-storage/async-storage";
import { Platform } from "react-native";
import { create } from "zustand";

import {
    DEVELOPMENT_DEFAULT,
    DEVELOPMENT_ENVIRONMENTS,
    type Environment,
    type EnvironmentId,
    environments,
    environmentsOf,
    isEnvironmentId,
    UNRELEASED_DEFAULT,
    UNRELEASED_ENVIRONMENTS,
} from "@/config/environments";
import { fetchReleasedBuild } from "@/services/environments.service";
import { describeError, logger } from "@/services/logger";
import { appBuild } from "@/utils/appVersion";

import { useConnectivityStore } from "./connectivity.store";

const STORAGE_KEY = "hg.environment";
const LAST_USED_KEY = "hg.environment.lastUsed";

const PRODUCTION_ONLY: readonly EnvironmentId[] = ["prod"];

interface EnvironmentState {
    /** the user's choice (Security → Environment, or the error page), kept only while this build offers it */
    selected?: EnvironmentId;
    /** what this build may use, in the selector's order: the selector shows only when there is more than one */
    offered: readonly EnvironmentId[];
    /** the environment the app runs on */
    effective: EnvironmentId;
    /** the environment the app ran on the last time; start-up resets the app when `effective` differs (`settleEnvironment`) */
    lastUsed: EnvironmentId;
    /** raises when the app must start again on a newly chosen environment */
    restarts: number;
    /**
     * Native, at start-up: works out `offered` and `effective` (environmentSelector.md, "App behaviour"). Rejects when
     * storage can't be read or the last used environment can't be recorded: start-up then stops (the error page) rather
     * than run on an environment without knowing where the app ran before, which could skip the reset.
     */
    hydrate: () => Promise<void>;
    /** a choice from the selector: saved, and the app runs on it from the restart */
    select: (id: EnvironmentId) => Promise<void>;
    /** saves `effective` as the last used environment, and the choice again (after a reset cleared storage) */
    remember: () => Promise<void>;
    requestRestart: () => void;
}

interface Resolved {
    offered: readonly EnvironmentId[];
    effective: EnvironmentId;
    selected?: EnvironmentId;
}

/**
 * A release build: what the released build allows. The released build or older offers Production only; a newer one
 * (TestFlight, Play testing, store review) Development, Test and Production, starting on Test. With no answer (offline,
 * slow, rate-limited), the app stays on the environment it was running on, with Production as the way back.
 */
async function resolveRelease(
    choice: EnvironmentId | undefined,
    previous: EnvironmentId
): Promise<Resolved> {
    let releasedBuild: number | null;
    try {
        if (!useConnectivityStore.getState().isOnline)
            throw new Error("offline");
        releasedBuild = await fetchReleasedBuild();
    } catch (error) {
        logger.info(
            `The released build is unavailable, staying on the last environment: ${describeError(error)}`
        );
        // never moves the app off the environment it was on: a flaky network can't wipe a tester's data
        const current = choice ?? previous;
        return current === "prod"
            ? { offered: PRODUCTION_ONLY, effective: "prod", selected: choice }
            : {
                  offered: environmentsOf([current, "prod"]).map((e) => e.id),
                  effective: current,
                  selected: choice,
              };
    }
    const build = appBuild();
    const unreleased =
        build !== undefined && releasedBuild !== null && build > releasedBuild;
    const offered = unreleased ? UNRELEASED_ENVIRONMENTS : PRODUCTION_ONLY;
    const selected = choice && offered.includes(choice) ? choice : undefined;
    return {
        offered,
        effective: selected ?? (unreleased ? UNRELEASED_DEFAULT : "prod"),
        selected,
    };
}

/** Before `hydrate`: a development build on Local, anything else on Production only. */
const initial = () =>
    __DEV__
        ? {
              offered: DEVELOPMENT_ENVIRONMENTS,
              effective: DEVELOPMENT_DEFAULT,
              lastUsed: DEVELOPMENT_DEFAULT,
          }
        : {
              offered: PRODUCTION_ONLY,
              effective: "prod" as EnvironmentId,
              lastUsed: "prod" as EnvironmentId,
          };

let revision = 0;

export const useEnvironmentStore = create<EnvironmentState>((set, get) => ({
    selected: undefined,
    ...initial(),
    restarts: 0,
    hydrate: async () => {
        if (Platform.OS === "web") return;
        revision += 1;
        const started = revision;
        try {
            const [stored, last] = await Promise.all([
                AsyncStorage.getItem(STORAGE_KEY),
                AsyncStorage.getItem(LAST_USED_KEY),
            ]);
            // a newer read or a choice came in while this waited: this one's answer is out of date
            if (started !== revision) return;
            if (__DEV__) {
                // every environment, no endpoint: the Maestro runs and local work are unaffected
                const selected = isEnvironmentId(stored) ? stored : undefined;
                const effective = selected ?? DEVELOPMENT_DEFAULT;
                set({
                    selected,
                    offered: DEVELOPMENT_ENVIRONMENTS,
                    effective,
                    lastUsed: isEnvironmentId(last) ? last : effective,
                });
                return;
            }
            // a release build never honours Local (no plain http)
            const choice =
                isEnvironmentId(stored) && stored !== "local"
                    ? stored
                    : undefined;
            // Where the app last ran, Local included: a release build installed over a development build (both are
            // hg.local) moves off Local, so start-up resets it. An install from before `lastUsed` was saved ran on its
            // choice (Local too), or on Production.
            const previous = isEnvironmentId(last)
                ? last
                : isEnvironmentId(stored)
                  ? stored
                  : "prod";
            // Recorded before the choice can be forgotten below, so a reset that fails or is cut short still has it,
            // and runs again on the next start
            if (!isEnvironmentId(last))
                await AsyncStorage.setItem(LAST_USED_KEY, previous);
            if (started !== revision) return;
            // what the release build may run on never includes Local
            const resolved = await resolveRelease(
                choice,
                previous === "local" ? "prod" : previous
            );
            if (started !== revision) return;
            set({ ...resolved, lastUsed: previous });
            // a choice this build no longer offers (it was just released, say) is forgotten; if that fails, the next
            // start forgets it again
            if (choice && !resolved.selected)
                await AsyncStorage.removeItem(STORAGE_KEY).catch(
                    (error: unknown) =>
                        logger.warn(
                            `Could not forget the environment choice: ${describeError(error)}`
                        )
                );
        } catch (error) {
            logger.warn(
                `Could not read the environment: ${describeError(error)}`
            );
            throw error;
        }
    },
    select: async (id) => {
        revision += 1;
        set({ selected: id, effective: id, lastUsed: id });
        try {
            await AsyncStorage.multiSet([
                [STORAGE_KEY, id],
                [LAST_USED_KEY, id],
            ]);
        } catch (error) {
            logger.warn(
                `Could not save the environment: ${describeError(error)}`
            );
        }
    },
    remember: async () => {
        const { effective, selected } = get();
        set({ lastUsed: effective });
        try {
            await AsyncStorage.multiSet([
                [LAST_USED_KEY, effective],
                ...(selected
                    ? ([[STORAGE_KEY, selected]] as [string, string][])
                    : []),
            ]);
        } catch (error) {
            logger.warn(
                `Could not save the environment: ${describeError(error)}`
            );
        }
    },
    requestRestart: () => set((s) => ({ restarts: s.restarts + 1 })),
}));

export const selectEnvironment = (s: { effective: EnvironmentId }) =>
    environments.find((e) => e.id === s.effective) as Environment;

/** Native: the selector (Security → Environment, Change Environment on the error page) shows only for several. */
export const selectHasEnvironmentChoice = (s: {
    offered: readonly EnvironmentId[];
}) => Platform.OS !== "web" && s.offered.length > 1;
