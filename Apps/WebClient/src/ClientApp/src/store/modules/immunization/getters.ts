import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationGetters, ImmunizationState } from "./types";
import { getImmunizationDatasetState } from "./util";

export const getters: ImmunizationGetters = {
    immunizations(
        state: ImmunizationState
    ): (hdid: string) => ImmunizationEvent[] {
        return (hdid: string) => getImmunizationDatasetState(state, hdid).data;
    },
    covidImmunizations(
        state: ImmunizationState
    ): (hdid: string) => ImmunizationEvent[] {
        return (hdid: string) =>
            getImmunizationDatasetState(state, hdid)
                .data.filter(
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
    recomendations(
        state: ImmunizationState
    ): (hdid: string) => Recommendation[] {
        return (hdid: string) =>
            getImmunizationDatasetState(state, hdid).recommendations;
    },
    immunizationsCount(state: ImmunizationState): (hdid: string) => number {
        return (hdid: string) =>
            getImmunizationDatasetState(state, hdid).data.length;
    },
    immunizationsAreLoading(
        state: ImmunizationState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getImmunizationDatasetState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
    immunizationsAreDeferred(
        state: ImmunizationState
    ): (hdid: string) => boolean {
        return (hdid: string) => {
            const datasetState = getImmunizationDatasetState(state, hdid);
            return (
                datasetState.status === LoadStatus.DEFERRED ||
                datasetState.status === LoadStatus.ASYNC_REQUESTED
            );
        };
    },
    immunizationsError(
        state: ImmunizationState
    ): (hdid: string) => ResultError | undefined {
        return (hdid: string) => getImmunizationDatasetState(state, hdid).error;
    },
};
