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
import PreferenceUtil from "@/utility/preferenceUtil";

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
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        if (userProfile) {
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialNote,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialMenuExport,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialAddDependent,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialAddQuickLink,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialTimelineFilter,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.TutorialComment,
                "true"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideVaccineCardQuickLink,
                "false"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideOrganDonorQuickLink,
                "false"
            );
            PreferenceUtil.setDefaultValue(
                userProfile.preferences,
                UserPreferenceType.HideImmunizationRecordQuickLink,
                "false"
            );
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
            "lastLoginDateTime",
            userProfile ? userProfile.lastLoginDateTime : undefined
        );
        Vue.set(
            state.user,
            "lastLoginDateTimes",
            userProfile ? userProfile.lastLoginDateTimes : []
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
    setEmailVerified(state: UserState) {
        Vue.set(state.user, "verifiedEmail", true);
    },
    setSMSResendDateTime(state: UserState, dateTime: DateWrapper) {
        state.smsResendDateTime = dateTime;
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setUserPreference(state: UserState, userPreference: UserPreference) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

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
    setPatientRetrievalFailed(state: UserState) {
        state.patientRetrievalFailed = true;
    },
    clearUserData(state: UserState) {
        state.user = new User();
        state.oidcUserInfo = undefined;
        state.patientData = new PatientData();
        state.patientRetrievalFailed = false;
        state.smsResendDateTime = undefined;
        state.seenTutorialComment = false;
        state.error = false;
        state.statusMessage = "";
        state.status = LoadStatus.NONE;
    },
    userError(state: UserState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
    setSeenTutorialComment: function (state: UserState, value: boolean): void {
        state.seenTutorialComment = value;
    },
};
