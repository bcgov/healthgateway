import { ActionTree, Commit } from "vuex";

import {
    ILogger,
    IPatientService,
    IUserProfileService,
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { RootState, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";
import UserEmailInvite from "@/models/userEmailInvite";
import UserSMSInvite from "@/models/userSMSInvite";
import { DateWrapper } from "@/models/dateWrapper";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`UserProfile ERROR: ${error}`);
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
                    logger.debug(
                        `Patient Data: ${JSON.stringify(patientData)}`
                    );
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
        return new Promise((resolve, reject) => {
            return userProfileService
                .getProfile(params.hdid)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    let isRegistered: boolean;
                    if (userProfile) {
                        isRegistered = userProfile.acceptedTermsOfService;
                        // If there are no preferences, set the default popover state
                        if (
                            userProfile.preferences["tutorialPopover"] ===
                            undefined
                        ) {
                            userProfile.preferences["tutorialPopover"] = "true";
                        }
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

                        return Promise.all([
                            latestEmailPromise,
                            latestSMSPromise,
                        ])
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
    updateSMSResendDateTime(context, params: { dateTime: DateWrapper }): void {
        context.commit("setSMSResendDateTime", params.dateTime);
    },
    updateUserPreference(
        context,
        params: { hdid: string; name: string; value: string }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .updateUserPreference(params.hdid, params.name, params.value)
                .then((result) => {
                    if (result) {
                        context.commit("setUserPreference", {
                            name: params.name,
                            value: params.value,
                        });
                    }
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
                    logger.debug(
                        `recoverUserAccount User Profile: ${JSON.stringify(
                            userProfile
                        )}`
                    );
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
