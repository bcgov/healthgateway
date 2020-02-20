import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
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
        //console.log("Pharmacy Data: ", requestResult);
        resolve(pharmacy);
      } else {
        console.log("Retrieving Pharmacy info");
        medicationService
          .getPharmacyInfo(pharmacyId)
          .then(pharmacy => {
            //console.log("Pharmacy Data: ", requestResult);
            commit("addPharmacyData", pharmacy);
            resolve(pharmacy);
          })
          .catch(error => {
            handleError(commit, error);
            reject(error);
          });
      }
    });
  }
};
