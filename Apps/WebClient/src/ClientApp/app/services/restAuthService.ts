import axios, { AxiosResponse } from 'axios';
import { IAuthenticationService } from '@/services/interfaces';

import { injectable, inject } from "inversify";
import "reflect-metadata";

import BearerToken from "@/models/bearerToken";
import router from "@/router"

@injectable()
export class RestAuthenticationService implements IAuthenticationService {

    private readonly BASE_PATH: string = 'someHardcodedPath';
    private readonly STORAGE_KEY: string = 'token';  // Key for localStoage for the token
    private readonly AUTH_URI: string = 'api/Something'; // This app's backend service to perform authentication (keeper of the client secret)
    private readonly HTTP_HEADER_AUTH: string = 'Authorization'; // Auth key for ensuring we send the base64 token 

    public startLoginFlow(idpHint: string, toPath: string): void {
        // Handle OIDC login by setting a hint that the AuthServer needs to know which IdP to route to
        // The server-side backend keeps the client secret needed to route to KeyCloak AS
        // We get back a JWT signed if the authentication was successful  
        let fullPath = window.location.href  + toPath;
        router.push({ path: '/Auth/Login', query: { idpHint: idpHint, redirectUri: fullPath } });
    }

    public getBearerToken(): Promise<BearerToken> {
        return new Promise((resolve, reject) => {
            axios.get<BearerToken>(this.AUTH_URI)
                .then((response: AxiosResponse) => {
                    // Verify that the object is correct.
                    if (response.data instanceof Object) {
                        let credentials: BearerToken = response.data;
                        return resolve(credentials);
                    }
                    else {
                        return reject('invalid data');
                    }
                })
                .catch(err => {
                    //commit('authenticationError', err.toString());
                    localStorage.removeItem(this.STORAGE_KEY);

                    console.log("Fetch error:" + err.toString());
                    reject(err);
                })
        })
    }

    public refreshToken(): Promise<BearerToken> {
        return new Promise((resolve, reject) => {
            //commit('logout');
            //localStorage.removeItem(STORAGE_KEY);
            //delete axios.defaults.headers.common[HTTP_HEADER_AUTH];
            resolve();
        })
    }

    public destroyToken(): Promise<void> {
        return new Promise((resolve, reject) => {
            //commit('logout');
            //localStorage.removeItem(STORAGE_KEY);
            //delete axios.defaults.headers.common[HTTP_HEADER_AUTH];
            resolve();
        })
    }
}