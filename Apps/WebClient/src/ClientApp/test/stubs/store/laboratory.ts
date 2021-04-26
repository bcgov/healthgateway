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

var laboratoryState: LaboratoryState = {
    laboratoryOrders: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

var laboratoryGetters: LaboratoryGetters = {
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

var laboratoryActions: LaboratoryActions = {
    retrieve(): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

var laboratoryMutations: LaboratoryMutations = {
    setRequested(): void {},
    setLaboratoryOrders(): void {},
    laboratoryError(): void {},
};

var laboratoryStub: LaboratoryModule = {
    namespaced: true,
    state: laboratoryState,
    getters: laboratoryGetters,
    actions: laboratoryActions,
    mutations: laboratoryMutations,
};

export default laboratoryStub;
