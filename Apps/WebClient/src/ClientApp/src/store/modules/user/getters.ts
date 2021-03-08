import { GetterTree } from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import PatientData from "@/models/patientData";
import { RootState, UserState } from "@/models/storeState";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";

export const getters: GetterTree<UserState, RootState> = {
    user(state: UserState): User {
        const { user } = state;
        return user;
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

    getPreference: (state: UserState) => (
        preferenceName: string
    ): UserPreference | undefined => {
        return state.user.preferences[preferenceName];
    },

    patientData(state: UserState): PatientData {
        return state.patientData;
    },
};
