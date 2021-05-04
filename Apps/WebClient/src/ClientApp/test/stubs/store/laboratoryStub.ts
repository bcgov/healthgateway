import { voidMethod, voidPromise } from "@test/stubs/util";

import { LaboratoryOrder } from "@/models/laboratory";
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
    retrieve: voidPromise,
    handleError: voidMethod,
};

const laboratoryMutations: LaboratoryMutations = {
    setRequested: voidMethod,
    setLaboratoryOrders: voidMethod,
    laboratoryError: voidMethod,
};

const laboratoryStub: LaboratoryModule = {
    namespaced: true,
    state: laboratoryState,
    getters: laboratoryGetters,
    actions: laboratoryActions,
    mutations: laboratoryMutations,
};

export default laboratoryStub;
