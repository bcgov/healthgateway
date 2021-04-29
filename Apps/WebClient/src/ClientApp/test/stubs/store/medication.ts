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

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    handleStatementError: stubbedVoid,
};

const medicationStatementMutations: MedicationStatementMutations = {
    setMedicationStatementRequested: stubbedVoid,
    setMedicationStatementResult: stubbedVoid,
    medicationStatementError: stubbedVoid,
};

const medicationStatementStub: MedicationStatementModule = {
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
    retrieveMedicationRequests: stubbedPromise,
    handleRequestError: stubbedVoid,
};

const medicationRequestMutations: MedicationRequestMutations = {
    setMedicationRequestRequested: stubbedVoid,
    setMedicationRequestResult: stubbedVoid,
    medicationRequestError: stubbedVoid,
};

const medicationRequestStub: MedicationRequestModule = {
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
