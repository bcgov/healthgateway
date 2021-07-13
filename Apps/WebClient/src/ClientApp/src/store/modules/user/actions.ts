import { Commit } from "vuex";

import { ResultType } from "@/constants/resulttype";
import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/requestResult";
import { UserPreference } from "@/models/userPreference";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    ILogger,
    IPatientService,
    IUserProfileService,
} from "@/services/interfaces";

import { UserActions } from "./types";

function handleError(commit: Commit, error: Error) {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.error(`UserProfile ERROR: ${error}`);
    commit("userError");
}

export const actions: UserActions = {
    checkRegistration(context): Promise<boolean> {
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            return userProfileService
                .getProfile(context.state.user.hdid)
                .then((userProfile) => {
                    const logger: ILogger = container.get(
                        SERVICE_IDENTIFIER.Logger
                    );
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
    updateUserEmail(context, params: { emailAddress: string }): Promise<void> {
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            userProfileService
                .updateEmail(context.state.user.hdid, params.emailAddress)
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
        params: { userPreference: UserPreference }
    ): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            userProfileService
                .updateUserPreference(
                    context.state.user.hdid,
                    params.userPreference
                )
                .then((result) => {
                    logger.debug(
                        `updateUserPreference: ${JSON.stringify(result)}`
                    );
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
        params: { userPreference: UserPreference }
    ): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            userProfileService
                .createUserPreference(
                    context.state.user.hdid,
                    params.userPreference
                )
                .then((result) => {
                    logger.debug(
                        `createUserPreference: ${JSON.stringify(result)}`
                    );
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
    closeUserAccount(context): Promise<void> {
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            userProfileService
                .closeAccount(context.state.user.hdid)
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
    recoverUserAccount(context): Promise<void> {
        const userProfileService: IUserProfileService =
            container.get<IUserProfileService>(
                SERVICE_IDENTIFIER.UserProfileService
            );

        return new Promise((resolve, reject) => {
            userProfileService
                .recoverAccount(context.state.user.hdid)
                .then((userProfile) => {
                    const logger: ILogger = container.get(
                        SERVICE_IDENTIFIER.Logger
                    );
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
    getPatientData(context): Promise<void> {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        const patientService: IPatientService = container.get<IPatientService>(
            SERVICE_IDENTIFIER.PatientService
        );
        return new Promise((resolve, reject) => {
            if (context.getters.patientData.hdid !== undefined) {
                logger.debug(`Patient data found stored, not querying!`);
                resolve();
            } else {
                context.commit("setRequested");
                patientService
                    .getPatientData(context.state.user.hdid)
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
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.error(`ERROR: ${JSON.stringify(error)}`);
        context.commit("userError", error);

        context.dispatch(
            "errorBanner/addResultError",
            { message: "Fetch User Error", error },
            { root: true }
        );
    },
};
