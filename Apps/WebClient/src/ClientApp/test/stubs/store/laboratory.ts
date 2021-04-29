import { LaboratoryOrder } from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";
import {
    LaboratoryActions,
    LaboratoryGetters,
    LaboratoryModule,
    LaboratoryMutations,
    LaboratoryState,
} from "@/store/modules/laboratory/types";

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    retrieve: stubbedPromise,
    handleError: stubbedVoid,
};

const laboratoryMutations: LaboratoryMutations = {
    setRequested: stubbedVoid,
    setLaboratoryOrders: stubbedVoid,
    laboratoryError: stubbedVoid,
};

const laboratoryStub: LaboratoryModule = {
    namespaced: true,
    state: laboratoryState,
    getters: laboratoryGetters,
    actions: laboratoryActions,
    mutations: laboratoryMutations,
};

export default laboratoryStub;
