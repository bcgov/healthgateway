import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import User, { OidcUserInfo } from "@/models/user";
import { QuickLinkUtil } from "@/utility/quickLinkUtil";

import { UserGetters, UserState } from "./types";

export const getters: UserGetters = {
    user(state: UserState): User {
        const { user } = state;
        return user;
    },
    lastLoginDateTime(state: UserState): StringISODateTime {
        const { user } = state;
        const loginDateTimes = user.lastLoginDateTimes;

        // If there is only 1 login, then it means this is the user's first time logging in.
        if (loginDateTimes.length == 1) {
            return loginDateTimes[0];
        }
        return loginDateTimes[1];
    },
    oidcUserInfo(state: UserState): OidcUserInfo | undefined {
        const { oidcUserInfo } = state;
        return oidcUserInfo;
    },
    isValidIdentityProvider: (state: UserState): boolean => {
        const { oidcUserInfo } = state;

        return oidcUserInfo === undefined
            ? false
            : oidcUserInfo.idp === "BCSC" || oidcUserInfo.idp === undefined;
    },
    userIsRegistered(state: UserState): boolean {
        const { user } = state;
        return user === undefined ? false : user.acceptedTermsOfService;
    },
    userIsActive(state: UserState): boolean {
        const { user } = state;
        return user === undefined ? false : !user.closedDateTime;
    },
    smsResendDateTime(state: UserState): DateWrapper | undefined {
        return state.smsResendDateTime;
    },
    seenTutorialComment: function (state: UserState): boolean {
        return state.seenTutorialComment;
    },
    hasTermsOfServiceUpdated(state: UserState): boolean {
        const { user } = state;
        return user === undefined ? false : user.hasTermsOfServiceUpdated;
    },
    quickLinks(state: UserState): QuickLink[] | undefined {
        const { user } = state;
        const preference = user.preferences[UserPreferenceType.QuickLinks];
        if (preference === undefined) {
            return undefined;
        }
        return QuickLinkUtil.toQuickLinks(preference.value);
    },
    patientData(state: UserState): PatientData {
        return state.patientData;
    },
    patientRetrievalFailed(state: UserState): boolean {
        return state.patientRetrievalFailed;
    },
    isLoading(state: UserState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
