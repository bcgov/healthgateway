import { ActionTree, Commit } from "vuex";

import {
  IPatientService,
  IUserProfileService,
  IUserEmailService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";
import EmailInvite from "@/models/emailInvite";

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

const userEmailService: IUserEmailService = container.get<IUserEmailService>(
  SERVICE_IDENTIFIER.UserEmailService
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

          // If registered retrieve the invite as well
          if (isRegistered) {
            userEmailService
              .getLatestInvite(hdid)
              .then(emailInvite => {
                commit("setValidatedEmail", emailInvite);
                resolve(isRegistered);
              })
              .catch(error => {
                handleError(commit, error);
                reject(error);
              });
          } else {
            commit("setValidatedEmail", undefined);
            resolve(isRegistered);
          }
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  getUserEmail({ commit }, { hdid }): Promise<EmailInvite> {
    return new Promise((resolve, reject) => {
      userEmailService
        .getLatestInvite(hdid)
        .then(emailInvite => {
          resolve(emailInvite);
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  updateUserEmail({ commit }, { hdid, emailAddress }): Promise<void> {
    return new Promise((resolve, reject) => {
      userEmailService
        .updateEmail(hdid, emailAddress)
        .then(() => {
          resolve();
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  }
};
