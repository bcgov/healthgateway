import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import { Dictionary } from "@/models/baseTypes";
import { MedicationState } from "@/models/datasetState";
import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { RootState } from "@/store/types";

export interface MedicationStatementState {
    medications: Dictionary<MedicationState>;
}

export interface MedicationStatementGetters
    extends GetterTree<MedicationStatementState, RootState> {
    medications(
        state: MedicationStatementState
    ): (hdid: string) => MedicationStatementHistory[];
    medicationsCount(state: MedicationStatementState): (hdid: string) => number;
    medicationsAreLoading(
        state: MedicationStatementState
    ): (hdid: string) => boolean;
    medicationsAreProtected(
        state: MedicationStatementState
    ): (hdid: string) => boolean;
    protectiveWordAttempts(
        state: MedicationStatementState
    ): (hdid: string) => number;
}

type StoreContext = ActionContext<MedicationStatementState, RootState>;
export interface MedicationStatementActions
    extends ActionTree<MedicationStatementState, RootState> {
    retrieveMedications(
        context: StoreContext,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>>;
    handleMedicationsError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
}

export interface MedicationStatementMutations
    extends MutationTree<MedicationStatementState> {
    setMedicationsRequested(
        state: MedicationStatementState,
        hdid: string
    ): void;
    setMedications(
        state: MedicationStatementState,
        payload: {
            hdid: string;
            medicationResult: RequestResult<MedicationStatementHistory[]>;
        }
    ): void;
    setMedicationsError(
        state: MedicationStatementState,
        payload: { hdid: string; error: Error }
    ): void;
}

export interface MedicationStatementModule
    extends Module<MedicationStatementState, RootState> {
    getters: MedicationStatementGetters;
    actions: MedicationStatementActions;
    mutations: MedicationStatementMutations;
}
