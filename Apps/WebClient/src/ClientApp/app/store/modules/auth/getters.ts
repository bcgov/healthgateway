import { GetterTree } from "vuex";
import { AuthState } from "@/models/authState";
import User from "@/models/user";
import { RootState } from "@/models/rootState";

export const getters: GetterTree<AuthState, RootState> = {
  isAuthenticated(state: AuthState): boolean {
    const { authentication } = state;
    return authentication != undefined ? authentication.isAuthenticated : false;
  },
  authenticationStatus(state: AuthState): string {
    return state.statusMessage;
  },
  authenticatedUser(state: AuthState): User | undefined {
    const { authentication } = state;
    return authentication != undefined ? authentication.user : undefined;
  }
};
