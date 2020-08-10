import { GetterTree } from "vuex";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { MedicationState, RootState, UserState } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const getters: GetterTree<MedicationState, RootState> = {
    getStoredMedication: (state: MedicationState) => (
        din: string
    ): MedicationResult | undefined => {
        din = din.padStart(8, "0");
        logger.info(`getStoredMedication: din = ${JSON.stringify(din)}`);
        return state.medications.find((item) => item.din === din);
    },
};
