import { User as OidcUser } from "oidc-client";
import Vue from "vue";

import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import type { UserPreference } from "@/models/userPreference";
import UserProfile from "@/models/userProfile";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

import { UserMutation, UserState } from "./types";

export const mutations: UserMutation = {
    setRequested(state: UserState) {
        state.status = LoadStatus.REQUESTED;
    },
    setOidcUserData(state: UserState, oidcUser: OidcUser) {
        Vue.set(state.user, "hdid", oidcUser.profile.hdid);
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    setProfileUserData(state: UserState, userProfile: UserProfile) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

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
        Vue.set(state, "smsResendDateTime", dateTime);
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
        Vue.set(
            state.user.preferences,
            userPreference.preference,
            userPreference
        );
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
