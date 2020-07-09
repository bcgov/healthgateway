import { ActionTree, Commit } from "vuex";

import { IPatientService, IUserProfileService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { RootState, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";
import UserEmailInvite from "@/models/userEmailInvite";
import UserSMSInvite from "@/models/userSMSInvite";
import UserPreference from "../../../models/userPreference";

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
    getPatientData(context, params: { hdid: string }): Promise<PatientData> {
        return new Promise((resolve, reject) => {
            patientService
                .getPatientData(params.hdid)
                .then((patientData) => {
                    console.log("Patient Data: ", patientData);
                    context.commit("setPatientData", patientData);
                    resolve(patientData);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    checkRegistration(context, params: { hdid: string }): Promise<boolean> {
        console.log(params);
        return new Promise((resolve, reject) => {
            userProfileService
                .getProfile(params.hdid)
                .then((userProfile) => {
                    console.log("User Profile: ", userProfile);
                    let isRegistered: boolean;
                    if (userProfile) {
                        isRegistered = userProfile.acceptedTermsOfService;
                    } else {
                        isRegistered = false;
                    }

                    context.commit("setProfileUserData", userProfile);

                    // If registered retrieve the invite as well
                    if (isRegistered) {
                        var latestEmailPromise = userProfileService.getLatestEmailInvite(
                            params.hdid
                        );
                        var latestSMSPromise = userProfileService.getLatestSMSInvite(
                            params.hdid
                        );

                        Promise.all([latestEmailPromise, latestSMSPromise])
                            .then((results) => {
                                // Latest Email invite
                                if (results[0]) {
                                    context.commit(
                                        "setValidatedEmail",
                                        results[0]
                                    );
                                }
                                // Latest SMS invite
                                if (results[1]) {
                                    context.commit(
                                        "setValidatedSMS",
                                        results[1]
                                    );
                                }
                                resolve(isRegistered);
                            })
                            .catch((error) => {
                                handleError(context.commit, error);
                                reject(error);
                            });
                    } else {
                        context.commit("setValidatedEmail", undefined);
                        context.commit("setValidatedSMS", undefined);
                        resolve(isRegistered);
                    }
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    getUserEmail(context, params: { hdid: string }): Promise<UserEmailInvite> {
        return new Promise((resolve, reject) => {
            userProfileService
                .getLatestEmailInvite(params.hdid)
                .then((userEmailInvite) => {
                    context.commit("setValidatedEmail", userEmailInvite);
                    resolve(userEmailInvite);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    getUserSMS(context, params: { hdid: string }): Promise<UserSMSInvite> {
        return new Promise((resolve, reject) => {
            userProfileService
                .getLatestSMSInvite(params.hdid)
                .then((userSMSInvite) => {
                    context.commit("setValidatedSMS", userSMSInvite);
                    resolve(userSMSInvite);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    updateUserEmail(
        context,
        params: { hdid: string; emailAddress: string }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .updateEmail(params.hdid, params.emailAddress)
                .then(() => {
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    getUserPreference(
        context,
        params: { hdid: string }
    ): Promise<UserPreference> {
        return new Promise((resolve, reject) => {
            userProfileService
                .getUserPreference(params.hdid)
                .then((userPreference) => {
                    console.log("User Preference Data: ", userPreference);
                    context.commit("setUserPreference", userPreference);
                    resolve(userPreference);
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    updateUserPreference(
        context,
        params: { userPreference: UserPreference }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .createUserPreference(params.userPreference)
                .then(() => {
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    closeUserAccount(context, params: { hdid: string }): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .closeAccount(params.hdid)
                .then((userProfile) => {
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    recoverUserAccount(context, params: { hdid: string }): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .recoverAccount(params.hdid)
                .then((userProfile) => {
                    console.log("User Profile: ", userProfile);
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
};
