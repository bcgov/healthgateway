import axios, { AxiosResponse } from 'axios';
import { IAuthenticationService } from '@/services/interfaces';

import { injectable, inject } from "inversify";
import "reflect-metadata";

import AuthenticationData from "@/models/authenticationData";
import router from "@/router"

@injectable()
export class RestAuthenticationService implements IAuthenticationService {

    private readonly BASE_PATH: string = 'someHardcodedPath';
    private readonly STORAGE_KEY: string = 'token';  // Key for localStoage for the token
    private readonly AUTH_URI: string = 'api/getAuthenticationData'; // This app's backend service to perform authentication (keeper of the client secret)
    private readonly HTTP_HEADER_AUTH: string = 'Authorization'; // Auth key for ensuring we send the base64 token 

    public startLoginFlow(idpHint: string, relativeToPath: string): void {
        // Handle OIDC login by setting a hint that the AuthServer needs to know which IdP to route to
        // The server-side backend keeps the client secret needed to route to KeyCloak AS
        // We get back a JWT signed if the authentication was successful  
        console.log("Starting login flow....");
        
        var fullRedirectUrl = new URL(relativeToPath, window.location.href); 

        let queryParams = `?idpHint=${idpHint}&redirectUri=${fullRedirectUrl.href}`;
        
        var authPathUrl = new URL('/Auth/Login', window.location.href); 
        let fullPath = authPathUrl + queryParams;
        console.log(fullPath);       
        window.location.href = fullPath;
    }

    public getAuthentication(): Promise<AuthenticationData> {
        return new Promise((resolve, reject) => {
            axios.get<AuthenticationData>(this.AUTH_URI)
                .then((response: AxiosResponse) => {
                    // Verify that the object is correct.
                    if (response.data instanceof Object) {
                        let credentials: AuthenticationData = response.data;
                        return resolve(credentials);
                    }
                    else {
                        return reject('invalid authentication data');
                    }
                })
                .catch(err => {
                    console.log("Fetch error:" + err.toString());
                    reject(err);
                })
        })
    }

    public refreshToken(): Promise<AuthenticationData> {
        return new Promise((resolve, reject) => {
            // NOT IMPLEMENTED
            resolve();
        })
    }

    public destroyToken(): Promise<void> {
        return new Promise((resolve, reject) => {
            // NOT IMPLEMENTED
            resolve();
        })
    }
}