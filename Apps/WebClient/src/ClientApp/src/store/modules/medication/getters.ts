import { GetterTree } from "vuex";

import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { MedicationState, RootState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const getters: GetterTree<MedicationState, RootState> = {
    getStoredMedicationStatements: (
        state: MedicationState
    ) => (): MedicationStatementHistory[] => {
        return state.medicationStatements;
    },
    getStoredMedicationInformation: (state: MedicationState) => (
        din: string
    ): MedicationResult | undefined => {
        din = din.padStart(8, "0");
        logger.debug(
            `getStoredMedicationInformation: din = ${JSON.stringify(din)}`
        );
        return state.medications.find((item) => item.din === din);
    },
};
