import Encounter from "@/models/encounter";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import {
    EncounterState,
    EncounterGetters,
    EncounterActions,
    EncounterMutations,
    EncounterModule,
} from "@/store/modules/encounter/types";

const encounterState: EncounterState = {
    patientEncounters: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

const encounterGetters: EncounterGetters = {
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

const encounterActions: EncounterActions = {
    retrieve(): Promise<RequestResult<Encounter[]>> {
        return new Promise(() => {});
    },
    handleError(): void {},
};

const encounterMutations: EncounterMutations = {
    setRequested(state: EncounterState): void {},
    setPatientEncounters(): void {},
    encounterError(): void {},
};

const encounterStub: EncounterModule = {
    namespaced: true,
    state: encounterState,
    getters: encounterGetters,
    actions: encounterActions,
    mutations: encounterMutations,
};

export default encounterStub;
