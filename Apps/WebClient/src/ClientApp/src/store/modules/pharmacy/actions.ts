import { ActionTree, Commit } from "vuex";

import { IMedicationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { PharmacyState, RootState } from "@/models/storeState";
import Pharmacy from "@/models/pharmacy";

function handleError(commit: Commit, error: Error) {
    console.log("ERROR:" + error);
    commit("medicationError");
}

const medicationService: IMedicationService = container.get<IMedicationService>(
    SERVICE_IDENTIFIER.MedicationService
);

export const actions: ActionTree<PharmacyState, RootState> = {
    getPharmacy(context, params: { pharmacyId: string }): Promise<Pharmacy> {
        return new Promise((resolve, reject) => {
            const pharmacy = context.getters.getStoredPharmacy(
                params.pharmacyId
            );
            if (pharmacy) {
                console.log("Pharmacy found stored, not quering!");
                //console.log("Pharmacy Data: ", requestResult);
                resolve(pharmacy);
            } else {
                console.log("Retrieving Pharmacy info");
                medicationService
                    .getPharmacyInfo(params.pharmacyId)
                    .then((pharmacy) => {
                        //console.log("Pharmacy Data: ", requestResult);
                        context.commit("addPharmacyData", pharmacy);
                        resolve(pharmacy);
                    })
                    .catch((error) => {
                        handleError(context.commit, error);
                        reject(error);
                    });
            }
        });
    },
};
