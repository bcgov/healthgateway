import { ActionTree, Commit } from "vuex";

import { IPatientService, IUserProfileService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { RootState, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";
import UserEmailInvite from "@/models/userEmailInvite";
import UserSMSInvite from "@/models/userSMSInvite";

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
        .then((patientData) => {
          console.log("Patient Data: ", patientData);
          commit("setPatientData", patientData);
          resolve(patientData);
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },

  checkRegistration({ commit }, { hdid }): Promise<boolean> {
    return new Promise((resolve, reject) => {
      userProfileService
        .getProfile(hdid)
        .then((userProfile) => {
          console.log("User Profile: ", userProfile);
          let isRegistered: boolean;
          if (userProfile) {
            isRegistered = userProfile.acceptedTermsOfService;
          } else {
            isRegistered = false;
          }

          commit("setProfileUserData", userProfile);

          // If registered retrieve the invite as well
          if (isRegistered) {
            userProfileService
              .getLatestEmailInvite(hdid)
              .then((userEmailInvite) => {
                commit("setValidatedEmail", userEmailInvite);
                resolve(isRegistered);
              })
              .catch((error) => {
                handleError(commit, error);
                reject(error);
              });

            userProfileService
              .getLatestSMSInvite(hdid)
              .then((userSMSInvite) => {
                commit("setValidatedSMS", userSMSInvite);
                resolve(userSMSInvite);
              })
              .catch((error) => {
                handleError(commit, error);
                reject(error);
              });
          } else {
            commit("setValidatedEmail", undefined);
            commit("setValidatedSMS", undefined);
            resolve(isRegistered);
          }
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  getUserEmail({ commit }, { hdid }): Promise<UserEmailInvite> {
    return new Promise((resolve, reject) => {
      userProfileService
        .getLatestEmailInvite(hdid)
        .then((userEmailInvite) => {
          commit("setValidatedEmail", userEmailInvite);
          resolve(userEmailInvite);
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  getUserSMS({ commit }, { hdid }): Promise<UserSMSInvite> {
    return new Promise((resolve, reject) => {
      userProfileService
        .getLatestSMSInvite(hdid)
        .then((userSMSInvite) => {
          commit("setValidatedSMS", userSMSInvite);
          resolve(userSMSInvite);
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  updateUserEmail({ commit }, { hdid, emailAddress }): Promise<void> {
    return new Promise((resolve, reject) => {
      userProfileService
        .updateEmail(hdid, emailAddress)
        .then(() => {
          resolve();
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  closeUserAccount({ commit }, { hdid }): Promise<void> {
    return new Promise((resolve, reject) => {
      userProfileService
        .closeAccount(hdid)
        .then((userProfile) => {
          commit("setProfileUserData", userProfile);
          resolve();
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  recoverUserAccount({ commit }, { hdid }): Promise<void> {
    return new Promise((resolve, reject) => {
      userProfileService
        .recoverAccount(hdid)
        .then((userProfile) => {
          console.log("User Profile: ", userProfile);
          commit("setProfileUserData", userProfile);
          resolve();
        })
        .catch((error) => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
};
