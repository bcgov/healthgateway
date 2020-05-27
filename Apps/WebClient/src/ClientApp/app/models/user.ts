export default class User {
  public hdid: string = "";
  public acceptedTermsOfService: boolean = false;
  public hasEmail: boolean = false;
  public verifiedEmail: boolean = false;
  public hasPhone: boolean = false;
  public verifiedPhone: boolean = false;
  public hasTermsOfServiceUpdated: boolean = false;
  public closedDateTime: Date | undefined;
}
