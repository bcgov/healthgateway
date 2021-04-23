import MedicationRequest from "@/models/MedicationRequest";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
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
import {
    MedicationModule,
    MedicationState,
} from "@/store/modules/medication/types";

var medicationState: MedicationState = {
    status: LoadStatus.NONE,
};

var medicationStatementState: MedicationStatementState = {
    medicationStatements: [],
    protectiveWordAttempts: 0,
    statusMessage: "",
    status: LoadStatus.NONE,
};

var medicationStatementGetters: MedicationStatementGetters = {
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

var medicationStatementActions: MedicationStatementActions = {
    retrieveMedicationStatements(): Promise<
        RequestResult<MedicationStatementHistory[]>
    > {
        return new Promise(() => {});
    },
    handleStatementError(): void {},
};

var medicationStatementMutations: MedicationStatementMutations = {
    setMedicationStatementRequested(): void {},
    setMedicationStatementResult(): void {},
    medicationStatementError(): void {},
};

var medicationStatementStub: MedicationStatementModule = {
    getters: medicationStatementGetters,
    actions: medicationStatementActions,
    mutations: medicationStatementMutations,
};

var medicationRequestState: MedicationRequestState = {
    medicationRequests: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

var medicationRequestGetters: MedicationRequestGetters = {
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

var medicationRequestActions: MedicationRequestActions = {
    retrieveMedicationRequests(): Promise<RequestResult<MedicationRequest[]>> {
        return new Promise(() => {});
    },
    handleRequestError(): void {},
};

var medicationRequestMutations: MedicationRequestMutations = {
    setMedicationRequestRequested(): void {},
    setMedicationRequestResult(): void {},
    medicationRequestError(): void {},
};

var medicationRequestStub: MedicationRequestModule = {
    getters: medicationRequestGetters,
    actions: medicationRequestActions,
    mutations: medicationRequestMutations,
};

var medicationStub: MedicationModule = {
    namespaced: true,
    modules: {
        statement: medicationStatementStub,
        request: medicationRequestStub,
    },
};

export default medicationStub;
