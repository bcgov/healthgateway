import { GetterTree } from "vuex";
import { RootState, UserState } from "@/models/storeState";
import User from "@/models/user";

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
};
