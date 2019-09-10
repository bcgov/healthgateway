import { ActionTree, Commit } from "vuex";

import { IPatientService, IHttpDelegate } from "@/services/interfaces";
import SERVICE_IDENTIFIER, {
  DELEGATE_IDENTIFIER
} from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, ConfigState, UserState } from "@/models/storeState";
import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";
import PatientData from "@/models/patientData";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("userError");
}

const patientService: IPatientService = container.get<IPatientService>(
  SERVICE_IDENTIFIER.PatientService
);

export const actions: ActionTree<UserState, RootState> = {
  getPatientData({ commit }, { hdid }): Promise<PatientData> {
    return new Promise((resolve, reject) => {
      patientService
        .getPatientData(hdid)
        .then(patientData => {
          console.log("Patient Data: ", patientData);
          commit("setPatientData", patientData);
          resolve(patientData);
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  }
};
