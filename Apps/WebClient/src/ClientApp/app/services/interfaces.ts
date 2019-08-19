import AuthenticationData from "@/models/authenticationData";
import ImmsData from "@/models/immsData";

export interface IAuthenticationService {
  startLoginFlow(idpHint: string, redirectUri: string): void;
  getAuthentication(): Promise<AuthenticationData>;
  refreshToken(): Promise<AuthenticationData>;
  destroyToken(): Promise<void>;
}

export interface IImmsService {
  getItems(): Promise<ImmsData[]>;
}
