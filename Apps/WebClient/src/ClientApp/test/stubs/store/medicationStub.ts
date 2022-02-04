import { voidMethod, voidPromise } from "@test/stubs/util";

import MedicationRequest from "@/models/MedicationRequest";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { LoadStatus } from "@/models/storeOperations";
import {
    MedicationRequestActions,
    MedicationRequestGetters,
    MedicationRequestModule,
    MedicationRequestMutations,
    MedicationRequestState,
} from "@/store/modules/medication/modules/request/types";
import {
    MedicationStatementActions,
    MedicationStatementGetters,
    MedicationStatementModule,
    MedicationStatementMutations,
    MedicationStatementState,
} from "@/store/modules/medication/modules/statement/types";
import { MedicationModule } from "@/store/modules/medication/types";

const medicationStatementState: MedicationStatementState = {
    medicationStatements: [],
    protectiveWordAttempts: 0,
    statusMessage: "",
    status: LoadStatus.NONE,
};

const medicationStatementGetters: MedicationStatementGetters = {
    medicationStatements(): MedicationStatementHistory[] {
        return [];
    },
    medicationStatementCount(): number {
        return 0;
    },
    protectedWordAttempts(): number {
        return 0;
    },
    isProtected(): boolean {
        return false;
    },
    isMedicationStatementLoading(): boolean {
        return false;
    },
};

const medicationStatementActions: MedicationStatementActions = {
    retrieveMedicationStatements: voidPromise,
    handleMedicationStatementError: voidMethod,
};

const medicationStatementMutations: MedicationStatementMutations = {
    setMedicationStatementRequested: voidMethod,
    setMedicationStatementResult: voidMethod,
    medicationStatementError: voidMethod,
};

const medicationStatementStub: MedicationStatementModule = {
    state: medicationStatementState,
    getters: medicationStatementGetters,
    actions: medicationStatementActions,
    mutations: medicationStatementMutations,
};

const medicationRequestState: MedicationRequestState = {
    medicationRequests: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

const medicationRequestGetters: MedicationRequestGetters = {
    medicationRequests(): MedicationRequest[] {
        return [];
    },
    medicationRequestCount(): number {
        return 0;
    },
    isMedicationRequestLoading(): boolean {
        return false;
    },
};

const medicationRequestActions: MedicationRequestActions = {
    retrieveMedicationRequests: voidPromise,
    handleMedicationRequestError: voidMethod,
};

const medicationRequestMutations: MedicationRequestMutations = {
    setMedicationRequestRequested: voidMethod,
    setMedicationRequestResult: voidMethod,
    medicationRequestError: voidMethod,
};

const medicationRequestStub: MedicationRequestModule = {
    state: medicationRequestState,
    getters: medicationRequestGetters,
    actions: medicationRequestActions,
    mutations: medicationRequestMutations,
};

const medicationStub: MedicationModule = {
    namespaced: true,
    modules: {
        statement: medicationStatementStub,
        request: medicationRequestStub,
    },
};

export default medicationStub;
