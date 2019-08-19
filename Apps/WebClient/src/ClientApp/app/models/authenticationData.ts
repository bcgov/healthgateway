import User from "@/models/user";

export default class AuthenticationData {
  public accessToken?: string;
  public idToken?: string;
  public isChecked: boolean = false;
  public user?: User;
  public scopes?: string[];
  public eventsAreBound: boolean = false;
  public error?: string;
}
