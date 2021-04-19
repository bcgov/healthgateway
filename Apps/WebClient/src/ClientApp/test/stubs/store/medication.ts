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

const medicationState: MedicationState = {
    status: LoadStatus.NONE,
};

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
    retrieveMedicationStatements(): Promise<
        RequestResult<MedicationStatementHistory[]>
    > {
        return new Promise(() => {});
    },
    handleStatementError(): void {},
};

const medicationStatementMutations: MedicationStatementMutations = {
    setMedicationStatementRequested(): void {},
    setMedicationStatementResult(): void {},
    medicationStatementError(): void {},
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
    retrieveMedicationRequests(): Promise<RequestResult<MedicationRequest[]>> {
        return new Promise(() => {});
    },
    handleRequestError(): void {},
};

const medicationRequestMutations: MedicationRequestMutations = {
    setMedicationRequestRequested(): void {},
    setMedicationRequestResult(): void {},
    medicationRequestError(): void {},
};

const medicationRequestStub: MedicationRequestModule = {
    state: medicationRequestState,
    getters: medicationRequestGetters,
    actions: medicationRequestActions,
    mutations: medicationRequestMutations,
};

const medicationStub: MedicationModule = {
    namespaced: true,
    state: medicationState,
    modules: {
        statement: medicationStatementStub,
        request: medicationRequestStub,
    },
};

export default medicationStub;
