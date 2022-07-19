import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationGetters, ImmunizationState } from "./types";

export const getters: ImmunizationGetters = {
    immunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations;
    },
    covidImmunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations
            .filter(
                (x) =>
                    x.targetedDisease?.toLowerCase().includes("covid") &&
                    x.valid
            )
            .sort((a, b) => {
                const firstDate = new DateWrapper(a.dateOfImmunization);
                const secondDate = new DateWrapper(b.dateOfImmunization);

                if (firstDate.isAfter(secondDate)) {
                    return 1;
                }
                if (firstDate.isBefore(secondDate)) {
                    return -1;
                }
                return 0;
            });
    },
    recomendations(state: ImmunizationState): Recommendation[] {
        return state.recommendations;
    },
    immunizationCount(state: ImmunizationState): number {
        return state.immunizations.length;
    },
    error(state: ImmunizationState): ResultError | undefined {
        return state.error;
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        return (
            state.status === LoadStatus.DEFERRED ||
            state.status === LoadStatus.ASYNC_REQUESTED
        );
    },
    isLoading(state: ImmunizationState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
