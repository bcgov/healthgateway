import { User as OidcUser } from "oidc-client";

export default class AuthenticationData {
  public accessToken?: string;
  public idToken?: string;
  public isChecked: boolean = false;
  public oidcUser?: OidcUser;
  public scopes?: string[];
  public eventsAreBound: boolean = false;
  public error?: string;
  public acceptedTermsOfService: boolean = false;
}
