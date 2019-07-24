import axios from 'axios';
import Vue from 'vue';

export const STORAGE_KEY = 'token';  // Key for localStoage for the token
export const AUTH_URI = '/auth/Login'; // This app's backend service to perform authentication (keeper of the client secret)
export const HINT_KEY = 'hint'; // Tell the Auth Server (KeyCloak) which Identity Provider to call (OIDC)
export const HTTP_HEADER_AUTH = 'Authorization'; // Auth key for ensuring we send the base64 token 

export default {
  namespaced: true,
  state: {
    status: '',
    token: localStorage.getItem(STORAGE_KEY) || ''
  },

  getters: {
    isLoggedIn: (state) => { return !!state.token },
    authStatus: (state) => state.status,
  },

  actions: {
    // OIDC login via backend for this front end
    login(commit, { idpHint, redirectUri }) {
      return new Promise((resolve, reject) => {
        commit('auth_request')
        // Handle OIDC login by setting a hint that the AuthServer needs to know which IdP to route to
        // The server-side backend keeps the client secret needed to route to KeyCloak AS
        // We get back a JWT signed if the authentication was successful 
        const params = { hint: idpHint, redirectUri: redirectUri }
      
        return Vue.axios.get(AUTH_URI, { params })
          .then(resp => {
            const token = resp.data.token
            localStorage.setItem(STORAGE_KEY, token)
            // Place the token (which is already base64) into http request header
            axios.defaults.headers.common[HTTP_HEADER_AUTH] = token
            commit('auth_success', token)
            resolve(resp)
          })
          .catch(error => {
            commit('auth_error')
            localStorage.removeItem(STORAGE_KEY)
            reject(error)
          })
      })
    },
    logout(commit) {
      return new Promise((resolve, reject) => {
        commit('logout')
        localStorage.removeItem(STORAGE_KEY)
        delete axios.defaults.headers.common[HTTP_HEADER_AUTH]
        resolve()
      })
    }
  },
  mutations: {
    auth_request(state) {
      state.status = 'loading'
    },
    auth_success(state, token) {
      state.status = 'success'
      state.token = token
    },
    auth_error(state) {
      state.status = 'error'
      state.token = ''
    },
    logout(state) {
      state.status = ''
      state.token = ''
    },
  },
};