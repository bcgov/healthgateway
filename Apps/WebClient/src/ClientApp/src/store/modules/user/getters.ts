import { GetterTree } from "vuex";
import { RootState, UserState } from "@/models/storeState";
import User from "@/models/user";
import { DateWrapper } from "@/models/dateWrapper";

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
<<<<<<< HEAD
    smsResendDateTime(state: UserState): DateWrapper | undefined {
        return state.smsResendDateTime;
=======
    SMSResendDateTime(state: UserState): Date {
        const { user } = state;
        return user.SMSResendDateTime === undefined
            ? new Date()
            : user.SMSResendDateTime;
>>>>>>> master
    },
};
