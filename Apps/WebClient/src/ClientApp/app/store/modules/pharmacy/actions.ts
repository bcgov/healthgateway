import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, PharmacyState } from "@/models/storeState";
import Pharmacy from "@/models/pharmacy";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("medicationError");
}

const medicationService: IMedicationService = container.get<IMedicationService>(
  SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<PharmacyState, RootState> = {
  getPharmacy({ commit, getters }, { pharmacyId }): Promise<Pharmacy> {
    return new Promise((resolve, reject) => {
      var pharmacy = getters.getStoredPharmacy(pharmacyId);
      if (pharmacy) {
        console.log("Pharmacy found stored, not quering!");
        resolve(pharmacy);
      } else {
        medicationService
          .getPharmacyInfo(pharmacyId)
          .then(requestResult => {
            console.log("Pharmacy Data: ", requestResult);
            commit("addPharmacyData", requestResult.resourcePayload);
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
