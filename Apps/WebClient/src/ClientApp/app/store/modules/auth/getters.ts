import { GetterTree } from "vuex";
import { User as OidcUser } from "oidc-client";
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

function tokenExp(token: string | undefined): number | undefined {
  if (token) {
    const parsed = parseJwt(token);
    return parsed.exp ? parsed.exp * 1000 : undefined;
  }
  return undefined;
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
  oidcUser(state: AuthState): OidcUser | undefined {
    const { authentication } = state;
    return authentication.oidcUser;
  },
  oidcAccessToken(state: AuthState): string | undefined {
    return tokenIsExpired(state.authentication.accessToken)
      ? undefined
      : state.authentication.accessToken;
  },
  oidcAccessTokenExp(state: AuthState): number | undefined {
    return tokenExp(state.authentication.accessToken);
  },
  oidcScopes(state: AuthState): string[] | undefined {
    return state.authentication.scopes;
  },
  oidcIdToken(state: AuthState): string | undefined {
    return tokenIsExpired(state.authentication.idToken)
      ? undefined
      : state.authentication.idToken;
  },
  oidcIdTokenExp(state: AuthState): number | undefined {
    return tokenExp(state.authentication.idToken);
  },
  oidcAuthenticationIsChecked(state: AuthState): boolean {
    return state.authentication.isChecked;
  },
  oidcError(state: AuthState): any {
    return state.error;
  },
  userIsRegistered(state: AuthState): boolean {
    let authentication = state.authentication;
    return authentication === undefined ? false : authentication.acceptedTermsOfService;
  }
};
