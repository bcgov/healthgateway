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
    medicationStatements(
        state: MedicationStatementState
    ): (hdid: string) => MedicationStatementHistory[];
    medicationStatementCount(
        state: MedicationStatementState
    ): (hdid: string) => number;
    protectedWordAttempts(
        state: MedicationStatementState
    ): (hdid: string) => number;
    isProtected(state: MedicationStatementState): (hdid: string) => boolean;
    isMedicationStatementLoading(
        state: MedicationStatementState
    ): (hdid: string) => boolean;
}

type StoreContext = ActionContext<MedicationStatementState, RootState>;
export interface MedicationStatementActions
    extends ActionTree<MedicationStatementState, RootState> {
    retrieveMedicationStatements(
        context: StoreContext,
        params: { hdid: string; protectiveWord?: string }
    ): Promise<RequestResult<MedicationStatementHistory[]>>;
    handleMedicationStatementError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
}

export interface MedicationStatementMutations
    extends MutationTree<MedicationStatementState> {
    setMedicationStatementRequested(
        state: MedicationStatementState,
        hdid: string
    ): void;
    setMedicationStatementResult(
        state: MedicationStatementState,
        payload: {
            hdid: string;
            medicationResult: RequestResult<MedicationStatementHistory[]>;
        }
    ): void;
    medicationStatementError(
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
