import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { LoadStatus } from "@/models/datasetState";
import type { Dependent } from "@/models/dependent";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { descending, fromIsoDate } from "@/utils/dateUtil";

import { useErrorStore } from "./error.store";
import { inProgressCalls } from "./utils/inProgressCalls";

const dependentSort = (a: Dependent, b: Dependent): number =>
    descending(
        fromIsoDate(a.dependentInformation.dateOfBirth),
        fromIsoDate(b.dependentInformation.dateOfBirth)
    );

interface DependentState {
    dependents: Dependent[];
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
    retrieveDependents: (hdid: string, bypassCache: boolean) => Promise<void>;
    removeDependent: (hdid: string, dependent: Dependent) => Promise<void>;
    reset: () => void;
}

/** Port of stores/dependent.ts */
export const useDependentStore = create<DependentState>((set, get) => {
    const inProgress = inProgressCalls();
    function handleError(error: unknown, errorType: ErrorType) {
        const resultError = error instanceof ResultError ? error : undefined;
        logger.error(`Error: ${describeError(error)}`);
        set({
            error: resultError,
            statusMessage: resultError?.message ?? String(error),
            status: LoadStatus.ERROR,
        });
        const errors = useErrorStore.getState();
        if (resultError?.statusCode === 429) {
            if (errorType === ErrorType.Retrieve)
                errors.setTooManyRequestsWarning("page");
            else errors.setTooManyRequestsError("page");
        } else {
            errors.addError(
                errorType,
                ErrorSourceType.Dependent,
                resultError?.traceId
            );
        }
    }

    return {
        dependents: [],
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
        retrieveDependents: async (hdid, bypassCache) => {
            // forced after a change, so an answer already on its way may be too old: wait it out, then ask again
            if (bypassCache)
                await inProgress.running(hdid)?.catch(() => undefined);
            if (get().status === LoadStatus.LOADED && !bypassCache) {
                logger.debug("Dependents found stored, not querying!");
                return;
            }
            return inProgress.share(hdid, async () => {
                logger.debug("Retrieving dependents");
                set({ status: LoadStatus.REQUESTED });
                try {
                    const result = await services.dependent.getAll(hdid);
                    set({
                        dependents: [...result].sort(dependentSort),
                        error: undefined,
                        status: LoadStatus.LOADED,
                    });
                } catch (error) {
                    rethrowIfSessionEnded(error);
                    handleError(error, ErrorType.Retrieve);
                    throw error;
                }
            });
        },
        removeDependent: async (hdid, dependent) => {
            logger.debug("Removing dependent");
            set({ status: LoadStatus.REQUESTED });
            try {
                await services.dependent.removeDependent(hdid, dependent);
                set((s) => ({
                    dependents: s.dependents.filter(
                        (d) =>
                            d.delegateId !== dependent.delegateId ||
                            d.ownerId !== dependent.ownerId
                    ),
                    error: undefined,
                    status: LoadStatus.LOADED,
                }));
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error, ErrorType.Delete);
                throw error;
            }
        },
        reset: () => {
            inProgress.clear();
            set({
                dependents: [],
                status: LoadStatus.NONE,
                statusMessage: "",
                error: undefined,
            });
        },
    };
});

export const selectDependentsAreLoading = (s: DependentState) =>
    s.status === LoadStatus.REQUESTED;
