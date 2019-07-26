import { ActionTree } from 'vuex';
import axios from 'axios';
import { AuthState, RootState } from '@/models/authState';

import { IAuthenticationService } from '@/services/interfaces'
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";

export const actions: ActionTree<AuthState, RootState> = {
    initialize({ commit }): any {
        console.log('Initializing the auth store...');
        const authService: IAuthenticationService = container.get<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService);
        return new Promise((resolve, reject) => {
            authService.getAuthentication().then(authData => {
                commit('authenticationInitialized', authData);
                resolve();
            }).catch(error => {
                console.log('ERROR:' + error);
                commit('authenticationError');
                reject(error);
            }).finally(() => {
                console.log('Finished initialization');
            });
        })
    },
    login({ commit }, { idpHint, redirectPath }): Promise<void> {

        const authService: IAuthenticationService = container.get<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService);
        commit('authenticationRequest');
        return new Promise((resolve, reject) => {
            authService.getAuthentication().then(authData => {
                if (authData.isAuthenticated) {
                    commit('authenticationLoaded', authData);
                    console.log(authData.token);
                }
                else {
                    authService.startLoginFlow(idpHint, redirectPath);
                }
                resolve();
            }).catch(error => {
                console.log('ERROR:' + error);
                commit('authenticationError');
                reject(error);
            });
        })
    },
    logout({ commit }): any {
        return new Promise((resolve, reject) => {
            commit('logout')
            localStorage.removeItem(STORAGE_KEY)
            delete axios.defaults.headers.common[HTTP_HEADER_AUTH]
            resolve()
        })
    }
};