export default class User {
  public hdid?: string;
  public acceptedTermsOfService: boolean = false;
  public hasEmail: boolean = false;
  public verifiedEmail: boolean = false;
  public hasTermsOfServiceUpdated: boolean = false;
  public closedDateTime: Date | undefined;
}
