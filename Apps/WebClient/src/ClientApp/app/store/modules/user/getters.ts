import { GetterTree } from "vuex";
import { RootState, UserState } from "@/models/storeState";
import User from "@/models/user";

export const getters: GetterTree<UserState, RootState> = {
  user(state: UserState): User {
    const { user } = state;
    return user;
  }
};
