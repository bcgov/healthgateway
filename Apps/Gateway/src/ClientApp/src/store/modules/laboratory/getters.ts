import { Covid19LaboratoryOrder, LaboratoryOrder } from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryGetters, LaboratoryState } from "./types";
import { getCovid19TestResultState, getLabResultState } from "./util";

export const getters: LaboratoryGetters = {
    covid19LaboratoryOrders(
        state: LaboratoryState
    ): (hdid: string) => Covid19LaboratoryOrder[] {
        return (hdid: string) => getCovid19TestResultState(state, hdid).data;
    },
    covid19LaboratoryOrdersCount(
        state: LaboratoryState
    ): (hdid: string) => number {
        return (hdid: string) =>
            getCovid19TestResultState(state, hdid).data.length;
    },
    covid19LaboratoryOrdersAreLoading(
        state: LaboratoryState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getCovid19TestResultState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
    laboratoryOrders(
        state: LaboratoryState
    ): (hdid: string) => LaboratoryOrder[] {
        return (hdid: string) => getLabResultState(state, hdid).data;
    },
    laboratoryOrdersCount(state: LaboratoryState): (hdid: string) => number {
        return (hdid: string) => getLabResultState(state, hdid).data.length;
    },
    laboratoryOrdersAreLoading(
        state: LaboratoryState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getLabResultState(state, hdid).status === LoadStatus.REQUESTED;
    },
    laboratoryOrdersAreQueued(
        state: LaboratoryState
    ): (hdid: string) => boolean {
        return (hdid: string) => getLabResultState(state, hdid).queued;
    },
};
