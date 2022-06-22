import Vue from "vue";

import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import User, { OidcUserInfo } from "@/models/user";
import type { UserPreference } from "@/models/userPreference";
import UserProfile from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { UserMutation, UserState } from "./types";

export const mutations: UserMutation = {
    setRequested(state: UserState) {
        state.status = LoadStatus.REQUESTED;
    },
    setOidcUserInfo(state: UserState, userInfo: OidcUserInfo) {
        Vue.set(state.user, "hdid", userInfo.hdid);
        state.oidcUserInfo = userInfo;
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setProfileUserData(state: UserState, userProfile: UserProfile) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        if (userProfile) {
            const notePreference = UserPreferenceType.TutorialMenuNote;
            // If there are no preferences, set the default popover state
            if (userProfile.preferences[notePreference] === undefined) {
                userProfile.preferences[notePreference] = {
                    hdId: userProfile.hdid,
                    preference: notePreference,
                    value: "true",
                    version: 0,
                    createdDateTime: new DateWrapper().toISO(),
                };
            }
            const exportPreference = UserPreferenceType.TutorialMenuExport;
            if (userProfile.preferences[exportPreference] === undefined) {
                userProfile.preferences[exportPreference] = {
                    hdId: userProfile.hdid,
                    preference: exportPreference,
                    value: "true",
                    version: 0,
                    createdDateTime: new DateWrapper().toISO(),
                };
            }
        }

        Vue.set(
            state.user,
            "acceptedTermsOfService",
            userProfile ? userProfile.acceptedTermsOfService : false
        );
        Vue.set(
            state.user,
            "hasTermsOfServiceUpdated",
            userProfile ? userProfile.hasTermsOfServiceUpdated : false
        );
        Vue.set(
            state.user,
            "closedDateTime",
            userProfile ? userProfile.closedDateTime : undefined
        );
        Vue.set(
            state.user,
            "preferences",
            userProfile ? userProfile.preferences : {}
        );
        Vue.set(state.user, "hasEmail", !!userProfile.email);
        Vue.set(state.user, "verifiedEmail", userProfile.isEmailVerified);
        Vue.set(state.user, "hasSMS", !!userProfile.smsNumber);
        Vue.set(state.user, "verifiedSMS", userProfile.isSMSNumberVerified);

        logger.verbose(`state.user: ${JSON.stringify(state.user)}`);

        state.error = false;
        state.statusMessage = "success";
        if (state.patientData.hdid !== undefined) {
            state.status = LoadStatus.LOADED;
        } else {
            state.status = LoadStatus.PARTIALLY_LOADED;
        }
    },
    setSMSResendDateTime(state: UserState, dateTime: DateWrapper) {
        state.smsResendDateTime = dateTime;
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setUserPreference(state: UserState, userPreference: UserPreference) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.debug(
            `setUserPreference: preference.name: ${JSON.stringify(
                userPreference.preference
            )}, preference.value: ${JSON.stringify(userPreference.value)}`
        );
        state.user.preferences = Object.assign({}, state.user.preferences, {
            [userPreference.preference]: userPreference,
        });
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setPatientData(state: UserState, patientData: PatientData) {
        state.patientData = patientData;
        state.error = false;
        state.statusMessage = "success";
        if (state.user.hdid !== undefined) {
            state.status = LoadStatus.LOADED;
        } else {
            state.status = LoadStatus.PARTIALLY_LOADED;
        }
    },
    clearUserData(state: UserState) {
        state.user = new User();
        state.oidcUserInfo = undefined;
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    userError(state: UserState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
