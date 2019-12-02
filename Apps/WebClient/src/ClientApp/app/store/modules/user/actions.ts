import { ActionTree, Commit } from "vuex";

import { IPatientService, IUserProfileService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("userError");
}

const patientService: IPatientService = container.get<IPatientService>(
  SERVICE_IDENTIFIER.PatientService
);

const userProfileService: IUserProfileService = container.get<
  IUserProfileService
>(SERVICE_IDENTIFIER.UserProfileService);

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
  },

  checkRegistration({ commit }, { hdid }): Promise<boolean> {
    return new Promise((resolve, reject) => {
      userProfileService
        .getProfile(hdid)
        .then(userProfile => {
          console.log("User Profile: ", userProfile);
          var isRegistered: boolean;
          if (userProfile) {
            isRegistered = userProfile.acceptedTermsOfService;
          } else {
            isRegistered = false;
          }

          commit("setProfileUserData", userProfile);
          resolve(isRegistered);
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  }
};
