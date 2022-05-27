import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { QuickLink } from "@/models/quickLink";
import { LoadStatus } from "@/models/storeOperations";
import User, { OidcUserInfo } from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import { QuickLinkUtil } from "@/utility/quickLinkUtil";

import { UserGetters, UserState } from "./types";

export const getters: UserGetters = {
    user(state: UserState): User {
        const { user } = state;
        return user;
    },
    oidcUserInfo(state: UserState): OidcUserInfo | undefined {
        const { oidcUserInfo } = state;
        return oidcUserInfo;
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
    hasTermsOfServiceUpdated(state: UserState): boolean {
        const { user } = state;
        return user === undefined ? false : user.hasTermsOfServiceUpdated;
    },
    getPreference:
        (state: UserState) =>
        (preferenceName: string): UserPreference | undefined => {
            return state.user.preferences[preferenceName];
        },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    quickLinks(_state: UserState, userGetters: any): QuickLink[] | undefined {
        const preference = userGetters.getPreference(
            UserPreferenceType.QuickLinks
        );
        if (preference === undefined) {
            return undefined;
        }
        return QuickLinkUtil.toQuickLinks(preference.value);
    },
    patientData(state: UserState): PatientData {
        return state.patientData;
    },
    isLoading(state: UserState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
