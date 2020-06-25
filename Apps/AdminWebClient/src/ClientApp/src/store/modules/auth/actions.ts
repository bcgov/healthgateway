import { ActionTree, Commit } from "vuex";

import { IAuthenticationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { RootState, AuthState } from "@/models/storeState";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("authenticationError");
}

export const actions: ActionTree<AuthState, RootState> = {
  initialize(context): Promise<void> {
    console.log("Initializing the auth store...");
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    return new Promise((resolve, reject) => {
      authService
        .getAuthentication()
        .then(authData => {
          context.commit("authenticationLoaded", authData);
          resolve();
        })
        .catch(error => {
          handleError(context.commit, error);
          reject(error);
        })
        .finally(() => {
          console.log("Finished initialization");
        });
    });
  },
  login(context, params: { redirectPath: string }): Promise<void> {
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    context.commit("authenticationRequest");
    return new Promise((resolve, reject) => {
      authService
        .getAuthentication()
        .then(authData => {
          if (authData.isAuthenticated) {
            context.commit("authenticationLoaded", authData);
            console.log(authData.token);
          } else {
            authService.startLoginFlow(params.redirectPath);
          }
          resolve();
        })
        .catch(error => {
          handleError(context.commit, error);
          reject(error);
        });
    });
  },
  logout(context): Promise<void> {
    const authService: IAuthenticationService = container.get<
      IAuthenticationService
    >(SERVICE_IDENTIFIER.AuthenticationService);
    return new Promise((resolve, reject) => {
      authService
        .destroyToken()
        .then(() => {
          context.commit("logout");
          resolve();
        })
        .catch(error => {
          console.log("ERROR:" + error);
          context.commit("authenticationError");
          reject(error);
        });
    });
  }
};
