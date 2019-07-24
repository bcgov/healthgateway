import { ActionTree } from 'vuex';
import axios from 'axios';
import { AuthState, Authentication, RootState } from '@/models/authState';

const STORAGE_KEY = 'token';  // Key for localStoage for the token
const AUTH_URI = '/auth/Login'; // This app's backend service to perform authentication (keeper of the client secret)
const HTTP_HEADER_AUTH = 'Authorization'; // Auth key for ensuring we send the base64 token 

export const actions: ActionTree<AuthState, RootState> = {
        login({ commit }, { idpHint, redirectUri }): any { 
        return new Promise((resolve, reject) => {
            commit('authenticationRequest', redirectUri)
            // Handle OIDC login by setting a hint that the AuthServer needs to know which IdP to route to
            // The server-side backend keeps the client secret needed to route to KeyCloak AS
            // We get back a JWT signed if the authentication was successful  
            axios.get(AUTH_URI, { params: { hint: idpHint, redirectUri: redirectUri }})
                .then(resp => {
                    const token = resp.data.token
                    localStorage.setItem(STORAGE_KEY, token)
                    // Place the token (which is already base64) into http request header
                    axios.defaults.headers.common[HTTP_HEADER_AUTH] = token
                    commit('authenticationLoaded', token)
                    resolve(resp)
                })
                .catch(err => {
                    commit('authenticationError', err.toString())
                    localStorage.removeItem(STORAGE_KEY)
                    reject(err)
                })
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