import { LaboratoryOrder } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import {
    LaboratoryActions,
    LaboratoryGetters,
    LaboratoryModule,
    LaboratoryMutations,
    LaboratoryState,
} from "@/store/modules/laboratory/types";

const laboratoryState: LaboratoryState = {
    laboratoryOrders: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

const laboratoryGetters: LaboratoryGetters = {
    laboratoryOrders(): LaboratoryOrder[] {
        return [];
    },
    laboratoryCount(): number {
        return 0;
    },
    isLoading(): boolean {
        return false;
    },
};

const laboratoryActions: LaboratoryActions = {
    retrieve(): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

const laboratoryMutations: LaboratoryMutations = {
    setRequested(): void {},
    setLaboratoryOrders(): void {},
    laboratoryError(): void {},
};

const laboratoryStub: LaboratoryModule = {
    namespaced: true,
    state: laboratoryState,
    getters: laboratoryGetters,
    actions: laboratoryActions,
    mutations: laboratoryMutations,
};

export default laboratoryStub;
