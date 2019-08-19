import { GetterTree } from "vuex";
import User from "@/models/user";
import { RootState, AuthState } from "@/models/storeState";

export const parseJwt = (token: string) => {
  try {
    var base64Url = token.split(".")[1];
    var base64 = base64Url.replace("-", "+").replace("_", "/");
    return JSON.parse(window.atob(base64));
  } catch (error) {
    return {};
  }
};

function tokenExp(token: string | undefined) {
  if (token) {
    const parsed = parseJwt(token);
    return parsed.exp ? parsed.exp * 1000 : null;
  }
  return null;
}

function tokenIsExpired(token: string | undefined) {
  const tokenExpiryTime = tokenExp(token);
  if (tokenExpiryTime) {
    return tokenExpiryTime < new Date().getTime();
  }
  return false;
}

export const getters: GetterTree<AuthState, RootState> = {
  authenticationStatus(state: AuthState): string {
    return state.statusMessage;
  },
  oidcIsAuthenticated(state: AuthState): boolean {
    return state.isAuthenticated;
  },
  oidcUser(state: AuthState): User | undefined {
    console.log("oidcUser");
    const { authentication } = state;
    return authentication.user;
  },
  oidcAccessToken: state => {
    return tokenIsExpired(state.authentication.accessToken)
      ? null
      : state.authentication.accessToken;
  },
  oidcAccessTokenExp: state => {
    return tokenExp(state.authentication.accessToken);
  },
  oidcScopes: state => {
    return state.authentication.scopes;
  },
  oidcIdToken: state => {
    return tokenIsExpired(state.authentication.idToken)
      ? null
      : state.authentication.idToken;
  },
  oidcIdTokenExp: state => {
    return tokenExp(state.authentication.idToken);
  },
  oidcAuthenticationIsChecked: state => {
    return state.authentication.isChecked;
  },
  oidcError: state => {
    return state.error;
  }
};
