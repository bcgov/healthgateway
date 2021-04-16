import { Module } from "vuex";

import { LoadStatus } from "@/models/storeOperations";

import { MedicationRequestModule } from "./modules/request/types";
import { MedicationStatementModule } from "./modules/statement/types";
import { RootState } from "@/store/types";

export interface MedicationState {
    status: LoadStatus;
}

export interface MedicationModule extends Module<MedicationState, RootState> {
    namespaced: boolean;
    state: MedicationState;
    modules: {
        statement: MedicationStatementModule;
        request: MedicationRequestModule;
    };
}
