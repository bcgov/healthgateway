import Encounter from "@/models/encounter";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import {
    EncounterActions,
    EncounterGetters,
    EncounterModule,
    EncounterMutations,
    EncounterState,
} from "@/store/modules/encounter/types";

var encounterState: EncounterState = {
    patientEncounters: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

var encounterGetters: EncounterGetters = {
    patientEncounters(): Encounter[] {
        return [];
    },
    encounterCount(): number {
        return 0;
    },
    isLoading(): boolean {
        return false;
    },
};

var encounterActions: EncounterActions = {
    retrieve(): Promise<RequestResult<Encounter[]>> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

var encounterMutations: EncounterMutations = {
    setRequested(state: EncounterState): void {},
    setPatientEncounters(): void {},
    encounterError(): void {},
};

var encounterStub: EncounterModule = {
    namespaced: true,
    state: encounterState,
    getters: encounterGetters,
    actions: encounterActions,
    mutations: encounterMutations,
};

export default encounterStub;
