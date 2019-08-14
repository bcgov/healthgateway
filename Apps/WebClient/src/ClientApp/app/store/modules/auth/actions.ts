import { ActionTree, Commit } from "vuex";
import { AuthState } from "@/models/authState";

import { IAuthenticationService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { RootState } from "@/models/rootState";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("authenticationError");
}

export const actions: ActionTree<AuthState, RootState> = {
  initialize({ commit }): any {
    console.log("Initializing the auth store...");
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    return new Promise((resolve, reject) => {
      authService
        .getAuthentication()
        .then(authData => {
          commit("authenticationLoaded", authData);
          resolve();
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        })
        .finally(() => {
          console.log("Finished initialization");
        });
    });
  },
  login({ commit }, { idpHint, redirectPath }): Promise<void> {
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    commit("authenticationRequest");
    return new Promise((resolve, reject) => {
      authService
        .getAuthentication()
        .then(authData => {
          if (authData.isAuthenticated) {
            commit("authenticationLoaded", authData);
            console.log(authData.token);
          } else {
            authService.startLoginFlow(idpHint, redirectPath);
          }
          resolve();
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        });
    });
  },
  logout({ commit }): any {
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    return new Promise((resolve, reject) => {
      authService
        .destroyToken()
        .then(() => {
          commit("logout");
          resolve();
        })
        .catch(error => {
          console.log("ERROR:" + error);
          commit("authenticationError");
          reject(error);
        });
    });
  }
};
