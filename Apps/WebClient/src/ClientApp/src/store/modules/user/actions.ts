import { ActionTree, Commit } from "vuex";

import { ResultType } from "@/constants/resulttype";
import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { RootState, UserState } from "@/models/storeState";
import { UserPreference } from "@/models/userPreference";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import {
    ILogger,
    IPatientService,
    IUserProfileService,
} from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

function handleError(commit: Commit, error: Error) {
    logger.error(`UserProfile ERROR: ${error}`);
    commit("userError");
}

const userProfileService: IUserProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);

const patientService: IPatientService = container.get<IPatientService>(
    SERVICE_IDENTIFIER.PatientService
);

export const actions: ActionTree<UserState, RootState> = {
    checkRegistration(context, params: { hdid: string }): Promise<boolean> {
        return new Promise((resolve, reject) => {
            return userProfileService
                .getProfile(params.hdid)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    if (userProfile) {
                        const notePreference =
                            UserPreferenceType.TutorialMenuNote;
                        // If there are no preferences, set the default popover state
                        if (
                            userProfile.preferences[notePreference] ===
                            undefined
                        ) {
                            userProfile.preferences[notePreference] = {
                                hdId: userProfile.hdid,
                                preference: notePreference,
                                value: "true",
                                version: 0,
                                createdDateTime: new DateWrapper().toISO(),
                            };
                        }
                        const exportPreference =
                            UserPreferenceType.TutorialMenuExport;
                        if (
                            userProfile.preferences[exportPreference] ===
                            undefined
                        ) {
                            userProfile.preferences[exportPreference] = {
                                hdId: userProfile.hdid,
                                preference: exportPreference,
                                value: "true",
                                version: 0,
                                createdDateTime: new DateWrapper().toISO(),
                            };
                        }
                    }

                    context.commit("setProfileUserData", userProfile);
                    resolve(userProfile.acceptedTermsOfService);
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
        params: { hdid: string; userPreference: UserPreference }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .updateUserPreference(params.hdid, params.userPreference)
                .then((result) => {
                    if (result) {
                        context.commit(
                            "setUserPreference",
                            params.userPreference
                        );
                    }
                    resolve();
                })
                .catch((error) => {
                    handleError(context.commit, error);
                    reject(error);
                });
        });
    },
    createUserPreference(
        context,
        params: { hdid: string; userPreference: UserPreference }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            userProfileService
                .createUserPreference(params.hdid, params.userPreference)
                .then((result) => {
                    if (result) {
                        context.commit(
                            "setUserPreference",
                            params.userPreference
                        );
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
    getPatientData(context, params: { hdid: string }): Promise<void> {
        return new Promise((resolve, reject) => {
            if (context.getters.patientData.hdid === undefined) {
                patientService
                    .getPatientData(params.hdid)
                    .then((result) => {
                        if (result.resultStatus === ResultType.Error) {
                            context.dispatch("handleError", result.resultError);
                            reject(result.resultError);
                        } else {
                            context.commit(
                                "setPatientData",
                                result.resourcePayload
                            );
                            resolve();
                        }
                    })
                    .catch((error) => {
                        context.dispatch("handleError", error);
                        reject(error);
                    });
            }
        });
    },
    handleError(context, error: ResultError) {
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("userError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch User Error", error },
            { root: true }
        );
    },
};
