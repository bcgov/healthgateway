import { User as OidcUser, UserManagerSettings } from "oidc-client";

export interface IAuthenticationService {
  getUser(): Promise<OidcUser | null>;
  logout(): Promise<void>;
  signinSilent(): Promise<OidcUser | null>;
  signinRedirect(idphint: string, redirectPath: string): Promise<void>;
  signinRedirectCallback(): Promise<OidcUser>;
  getOidcConfig(): UserManagerSettings;
}

export interface IImmsService {
  getItems(): Promise<any>;
}
