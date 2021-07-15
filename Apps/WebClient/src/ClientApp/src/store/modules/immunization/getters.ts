import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationGetters, ImmunizationState } from "./types";

export const getters: ImmunizationGetters = {
    immunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations;
    },
    covidImmunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations
            .filter((x) => x.targetedDisease?.toLowerCase().includes("covid"))
            .sort((a, b) => {
                const firstDate = new DateWrapper(a.dateOfImmunization);
                const secondDate = new DateWrapper(b.dateOfImmunization);

                return firstDate.isAfter(secondDate)
                    ? 1
                    : firstDate.isBefore(secondDate)
                    ? -1
                    : 0;
            });
    },
    recomendations(state: ImmunizationState): Recommendation[] {
        return state.recommendations;
    },
    immunizationCount(state: ImmunizationState): number {
        return state.immunizations.length;
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
