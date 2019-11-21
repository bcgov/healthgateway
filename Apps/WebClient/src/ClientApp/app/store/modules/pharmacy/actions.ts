import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, PharmacyState } from "@/models/storeState";
import Pharmacy from "@/models/pharmacy";
import { ResultType } from "@/constants/resulttype";

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
        console.log("Retrieving Pharmacy info");
        medicationService
          .getPharmacyInfo(pharmacyId)
          .then(requestResult => {
            console.log("Pharmacy Data: ", requestResult);
            if (requestResult.resultStatus === ResultType.Success) {
              commit("addPharmacyData", requestResult.resourcePayload);
              resolve(requestResult.resourcePayload);
            } else {
              reject(requestResult.resultMessage);
            }
          })
          .catch(error => {
            handleError(commit, error);
            reject(error);
          });
      }
    });
  }
};
