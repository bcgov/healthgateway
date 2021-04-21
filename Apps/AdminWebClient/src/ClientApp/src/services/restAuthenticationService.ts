import "reflect-metadata";

import { injectable } from "inversify";

import AuthenticationData from "@/models/authenticationData";
import ExternalConfiguration from "@/models/externalConfiguration";
import Router from "@/router";
import { IAuthenticationService, IHttpDelegate } from "@/services/interfaces";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
    private readonly AUTH_BASE_URI: string = "v1/api/Authentication";
    private http!: IHttpDelegate;
    private config!: ExternalConfiguration;

    public initialize(
        http: IHttpDelegate,
        config: ExternalConfiguration
    ): void {
        this.http = http;
        this.config = config;
    }

    public startLoginFlow(relativeToPath: string): void {
        // Handle OIDC login by setting a hint that the AuthServer needs to know which IdP to route to
        // The server-side backend keeps the client secret needed to route to KeyCloak AS
        // We get back a JWT signed if the authentication was successful
        console.log("Starting login flow....");

        const fullRedirectUrl = new URL(relativeToPath, window.location.href);

        Router.push({
            path: "/Login",
            query: { redirectUri: fullRedirectUrl.href }
        });
        // Triggers a page refresh so the server side route can redirect to the oidc flow
        Router.go(0);
    }

    public getAuthentication(): Promise<AuthenticationData> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<AuthenticationData>(`${this.AUTH_BASE_URI}/`)
                .then(result => {
                    return resolve(result);
                })
                .catch(err => {
                    console.log("Fetch error:" + err.toString());
                    reject(err);
                });
        });
    }

    public refreshToken(): Promise<AuthenticationData> {
        return Promise.reject("refresh token not implemented.");
    }

    public destroyToken(): Promise<void> {
        return new Promise(() => {
            console.log("Starting Logout flow....");

            const authPathUrl = new URL(
                `${this.config.forwardProxies.basePath}/Logout`,
                window.location.href
            );

            window.location.href = authPathUrl.href;
        });
    }
}
