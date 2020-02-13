import { GetterTree } from "vuex";
import User from "@/models/userProfile";
import { RootState, AuthState } from "@/models/storeState";

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
    return authentication != undefined ? authentication.userProfile : undefined;
  }
};
