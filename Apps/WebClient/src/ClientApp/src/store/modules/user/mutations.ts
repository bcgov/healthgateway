import { User as OidcUser } from "oidc-client";
import Vue from "vue";
import { MutationTree } from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import { StateType, UserState } from "@/models/storeState";
import User from "@/models/user";
import UserEmailInvite from "@/models/userEmailInvite";
import type { UserPreference } from "@/models/userPreference";
import UserProfile from "@/models/userProfile";
import UserSMSInvite from "@/models/userSMSInvite";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
export const mutations: MutationTree<UserState> = {
    setOidcUserData(state: UserState, oidcUser: OidcUser) {
        Vue.set(state.user, "hdid", oidcUser.profile.hdid);
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    setProfileUserData(state: UserState, userProfile: UserProfile) {
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
        logger.verbose(`state.user: ${JSON.stringify(state.user)}`);
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    setValidatedEmail(state: UserState, userEmailInvite: UserEmailInvite) {
        if (userEmailInvite) {
            Vue.set(state.user, "hasEmail", true);
            Vue.set(state.user, "verifiedEmail", userEmailInvite.validated);
        } else {
            Vue.set(state.user, "hasEmail", false);
            Vue.set(state.user, "verifiedEmail", false);
        }

        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    setValidatedSMS(state: UserState, userSMSInvite: UserSMSInvite) {
        if (userSMSInvite) {
            Vue.set(state.user, "hasSMS", true);
            Vue.set(state.user, "verifiedSMS", userSMSInvite.validated);
        } else {
            Vue.set(state.user, "hasSMS", false);
            Vue.set(state.user, "verifiedSMS", false);
        }
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    setSMSResendDateTime(state: UserState, dateTime: DateWrapper) {
        Vue.set(state, "smsResendDateTime", dateTime);
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    setUserPreference(state: UserState, userPreference: UserPreference) {
        logger.debug(
            `setUserPreference: preference.name: ${JSON.stringify(
                userPreference.preference
            )}, preference.value: ${JSON.stringify(userPreference.value)}`
        );
        state.user.preferences[userPreference.preference] = userPreference;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    clearUserData(state: UserState) {
        state.user = new User();
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    userError(state: UserState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
