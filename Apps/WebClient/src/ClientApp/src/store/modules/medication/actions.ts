import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { MedicationState, RootState } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("medicationError");
}

const medicationService: IMedicationService = container.get<IMedicationService>(
  SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<MedicationState, RootState> = {
  getMedication(context, params: { din: string }): Promise<MedicationResult> {
    return new Promise((resolve, reject) => {
      const medicationResult = context.getters.getStoredMedication(params.din);
      if (medicationResult) {
        console.log("Medication found stored, not quering!");
        //console.log("Medication Data: ", medicationResult);
        resolve(medicationResult);
      } else {
        console.log("Retrieving Medication info");
        medicationService
          .getMedicationInformation(params.din)
          .then((medicationData) => {
            //console.log("Medication Data: ", requestResult);
            if (medicationData) {
              context.commit("addMedicationData", medicationData);
              resolve(medicationData);
            } else {
              resolve(undefined);
            }
          })
          .catch((error) => {
            handleError(context.commit, error);
            reject(error);
          });
      }
    });
  },
};
