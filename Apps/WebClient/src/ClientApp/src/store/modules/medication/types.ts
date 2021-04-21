import { Module } from "vuex";

import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

import { MedicationRequestModule } from "./modules/request/types";
import { MedicationStatementModule } from "./modules/statement/types";

export interface MedicationState {
    status: LoadStatus;
}

export interface MedicationModule extends Module<MedicationState, RootState> {
    namespaced: boolean;
    modules: {
        statement: MedicationStatementModule;
        request: MedicationRequestModule;
    };
}
