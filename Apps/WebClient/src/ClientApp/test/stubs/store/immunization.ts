import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";
import {
    ImmunizationActions,
    ImmunizationGetters,
    ImmunizationModule,
    ImmunizationMutations,
    ImmunizationState,
} from "@/store/modules/immunization/types";

var immunizationState: ImmunizationState = {
    immunizations: [],
    recommendations: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

var immunizationGetters: ImmunizationGetters = {
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

var immunizationActions: ImmunizationActions = {
    retrieve(): Promise<void> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

var immunizationMutations: ImmunizationMutations = {
    setRequested(state: ImmunizationState): void {},
    setImmunizationResult(): void {},
    immunizationError(): void {},
};

var immunizationStub: ImmunizationModule = {
    namespaced: true,
    state: immunizationState,
    getters: immunizationGetters,
    actions: immunizationActions,
    mutations: immunizationMutations,
};

export default immunizationStub;
