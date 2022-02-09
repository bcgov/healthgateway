import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult, { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface MedicationStatementState {
    medicationStatements: MedicationStatementHistory[];
    protectiveWordAttempts: number;
    statusMessage: string;
    error?: ResultError;
    status: LoadStatus;
}

export interface MedicationStatementGetters
    extends GetterTree<MedicationStatementState, RootState> {
    medicationStatements(
        state: MedicationStatementState
    ): MedicationStatementHistory[];
    medicationStatementCount(state: MedicationStatementState): number;
    protectedWordAttempts(state: MedicationStatementState): number;
    isProtected(state: MedicationStatementState): boolean;
    isMedicationStatementLoading(state: MedicationStatementState): boolean;
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
        params: { error: ResultError; errorType: ErrorType }
    ): void;
}

export interface MedicationStatementMutations
    extends MutationTree<MedicationStatementState> {
    setMedicationStatementRequested(state: MedicationStatementState): void;
    setMedicationStatementResult(
        state: MedicationStatementState,
        medicationResult: RequestResult<MedicationStatementHistory[]>
    ): void;
    medicationStatementError(
        state: MedicationStatementState,
        error: Error
    ): void;
}

export interface MedicationStatementModule
    extends Module<MedicationStatementState, RootState> {
    getters: MedicationStatementGetters;
    actions: MedicationStatementActions;
    mutations: MedicationStatementMutations;
}
