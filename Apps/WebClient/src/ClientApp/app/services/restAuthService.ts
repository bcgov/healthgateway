import { IAuthenticationService } from "@/services/interfaces";
import {
  UserManager,
  WebStorageStateStore,
  User as OidcUser,
  UserManagerSettings
} from "oidc-client";

import { injectable } from "inversify";
import "reflect-metadata";

@injectable()
export class RestAuthenticationService implements IAuthenticationService {
  private readonly oidcUserManager: UserManager;

  constructor() {
    this.oidcUserManager = new UserManager(this.getOidcConfig());
  }

  public getOidcConfig(): UserManagerSettings {
    // TODO: This should retrieve the configuration from the configuration store or from the service itself

    return {
      userStore: new WebStorageStateStore({ store: window.localStorage }),
      authority: "http://localhost:8080/auth/realms/Health",
      client_id: "gateway",
      redirect_uri: "http://localhost:5000/loginCallback",
      response_type: "code",
      scope: "openid profile",
      post_logout_redirect_uri: "http://localhost:5000/logout",
      filterProtocolClaims: true,
      loadUserInfo: false
    };
  }

  public getUser(): Promise<OidcUser | null> {
    return new Promise<OidcUser | null>(resolve => {
      this.oidcUserManager
        .getUser()
        .then(user => {
          resolve(user);
        })
        .catch(() => {
          resolve(null);
        });
    });
  }

  public logout(): Promise<void> {
    return this.oidcUserManager.signoutRedirect();
  }

  public signinSilent(): Promise<OidcUser> {
    return this.oidcUserManager.signinSilent();
  }

  public signinRedirect(idpHint: string, redirectPath: string): Promise<void> {
    var fullRedirectUrl = new URL(redirectPath, window.location.href);
    console.log(fullRedirectUrl);
    console.log(redirectPath);
    sessionStorage.setItem("vuex_oidc_active_route", redirectPath);
    return this.oidcUserManager.signinRedirect({
      extraQueryParams: {
        kc_idp_hint: idpHint
      }
    });
  }
  public signinRedirectCallback(): Promise<OidcUser> {
    return this.oidcUserManager.signinRedirectCallback();
  }

  //TODO: Do we still need this?
  public expireSiteMinderCookie() {
    // This expires the siteminder cookie preventing the app from login in using the cache.
    var d = new Date();
    document.cookie = `SMSESSION=;domain=.gov.bc.ca;path=/;expires=${d.toUTCString()}`;
  }
}
