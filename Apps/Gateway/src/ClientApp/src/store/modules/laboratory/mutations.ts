import { Covid19TestResultState, LabResultState } from "@/models/datasetState";
import {
    Covid19LaboratoryOrder,
    LaboratoryOrderResult,
} from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryMutations, LaboratoryState } from "./types";
import {
    getCovid19TestResultState,
    getLabResultState,
    setCovid19TestResultState,
    setLabResultState,
} from "./util";

export const mutations: LaboratoryMutations = {
    setCovid19LaboratoryOrdersRequested(state: LaboratoryState, hdid: string) {
        const currentState = getCovid19TestResultState(state, hdid);
        const nextState: Covid19TestResultState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setCovid19TestResultState(state, hdid, nextState);
    },
    setCovid19LaboratoryOrders(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrders: Covid19LaboratoryOrder[];
        }
    ) {
        const { hdid, laboratoryOrders } = payload;
        const currentState = getCovid19TestResultState(state, hdid);
        const nextState: Covid19TestResultState = {
            ...currentState,
            data: laboratoryOrders,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
        };
        setCovid19TestResultState(state, hdid, nextState);
    },
    setCovid19LaboratoryError(
        state: LaboratoryState,
        payload: {
            hdid: string;
            error: Error;
        }
    ) {
        const { hdid, error } = payload;
        const currentState = getCovid19TestResultState(state, hdid);
        const nextState: Covid19TestResultState = {
            ...currentState,
            statusMessage: error.message,
            status: LoadStatus.ERROR,
        };
        setCovid19TestResultState(state, hdid, nextState);
    },
    setLaboratoryOrdersRequested(state: LaboratoryState, hdid: string) {
        const currentState = getLabResultState(state, hdid);
        const nextState: LabResultState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setLabResultState(state, hdid, nextState);
    },
    setLaboratoryOrders(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrderResult: LaboratoryOrderResult;
        }
    ) {
        const { hdid, laboratoryOrderResult } = payload;
        const currentState = getLabResultState(state, hdid);
        const nextState: LabResultState = {
            ...currentState,
            data: laboratoryOrderResult.orders,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
            queued: laboratoryOrderResult.queued,
        };
        setLabResultState(state, hdid, nextState);
    },
    setLaboratoryOrdersRefreshInProgress(
        state: LaboratoryState,
        payload: {
            hdid: string;
            laboratoryOrderResult: LaboratoryOrderResult;
        }
    ) {
        const { hdid, laboratoryOrderResult } = payload;
        const currentState = getLabResultState(state, hdid);
        const nextState: LabResultState = {
            ...currentState,
            data: laboratoryOrderResult.orders,
            error: undefined,
            statusMessage: "",
            status: LoadStatus.REQUESTED,
            queued: laboratoryOrderResult.queued,
        };
        setLabResultState(state, hdid, nextState);
    },
    setLaboratoryError(
        state: LaboratoryState,
        payload: {
            hdid: string;
            error: Error;
        }
    ) {
        const { hdid, error } = payload;
        const currentState = getLabResultState(state, hdid);
        const nextState: LabResultState = {
            ...currentState,
            statusMessage: error.message,
            status: LoadStatus.ERROR,
        };
        setLabResultState(state, hdid, nextState);
    },
};
