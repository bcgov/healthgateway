import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, MedicationState } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("medicationError");
}

const medicationService: IMedicationService = container.get<IMedicationService>(
  SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<MedicationState, RootState> = {
  getMedication({ commit, getters }, { din }): Promise<MedicationResult> {
    return new Promise((resolve, reject) => {
      var medicationResult = getters.getStoredMedication(din);
      if (medicationResult) {
        console.log("Medication found stored, not quering!");
        resolve(medicationResult);
      } else {
        medicationService
          .getMedicationInformation(din)
          .then(requestResult => {
            console.log("Medication Data: ", requestResult);
            commit("addMedicationData", requestResult.resourcePayload);
            resolve(requestResult.resourcePayload);
          })
          .catch(error => {
            handleError(commit, error);
            reject(error);
          });
      }
    });
  }
};
