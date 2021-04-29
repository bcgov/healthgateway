import Encounter from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";
import {
    EncounterActions,
    EncounterGetters,
    EncounterModule,
    EncounterMutations,
    EncounterState,
} from "@/store/modules/encounter/types";

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    retrieve: stubbedPromise,
    handleError: stubbedVoid,
};

const encounterMutations: EncounterMutations = {
    setRequested: (state: EncounterState) => stubbedPromise(),
    setPatientEncounters: stubbedVoid,
    encounterError: stubbedVoid,
};

const encounterStub: EncounterModule = {
    namespaced: true,
    state: encounterState,
    getters: encounterGetters,
    actions: encounterActions,
    mutations: encounterMutations,
};

export default encounterStub;
