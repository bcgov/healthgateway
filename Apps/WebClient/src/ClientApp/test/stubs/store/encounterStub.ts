import { voidMethod, voidPromise } from "@test/stubs/util";

import Encounter from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";
import {
    EncounterActions,
    EncounterGetters,
    EncounterModule,
    EncounterMutations,
    EncounterState,
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
    retrieve: voidPromise,
    handleError: voidMethod,
};

const encounterMutations: EncounterMutations = {
    setRequested: voidMethod,
    setPatientEncounters: voidMethod,
    encounterError: voidMethod,
};

const encounterStub: EncounterModule = {
    namespaced: true,
    state: encounterState,
    getters: encounterGetters,
    actions: encounterActions,
    mutations: encounterMutations,
};

export default encounterStub;
