import { stubbedPromise, voidMethod } from "@test/stubs/util";

import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";
import {
    ImmunizationActions,
    ImmunizationGetters,
    ImmunizationModule,
    ImmunizationMutations,
    ImmunizationState,
} from "@/store/modules/immunization/types";

const immunizationState: ImmunizationState = {
    immunizations: [],
    recommendations: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

const immunizationGetters: ImmunizationGetters = {
    immunizations(): ImmunizationEvent[] {
        return [];
    },
    recomendations(): Recommendation[] {
        return [];
    },
    immunizationCount(): number {
        return 0;
    },
    isDeferredLoad(): boolean {
        return false;
    },
    isLoading(): boolean {
        return false;
    },
};

const immunizationActions: ImmunizationActions = {
    retrieve: stubbedPromise,
    handleError: voidMethod,
};

const immunizationMutations: ImmunizationMutations = {
    setRequested: voidMethod,
    setImmunizationResult: voidMethod,
    immunizationError: voidMethod,
};

const immunizationStub: ImmunizationModule = {
    namespaced: true,
    state: immunizationState,
    getters: immunizationGetters,
    actions: immunizationActions,
    mutations: immunizationMutations,
};

export default immunizationStub;
