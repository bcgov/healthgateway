import { User as OidcUser, UserManagerSettings } from "oidc-client";
import {
  ExternalConfiguration,
  OpenIdConnectConfiguration
} from "@/models/ConfigData";

export interface IAuthenticationService {
  initialize(config: OpenIdConnectConfiguration): void;
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

export interface IConfigService {
  getConfiguration(): Promise<ExternalConfiguration>;
}
