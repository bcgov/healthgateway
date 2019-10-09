export default class User {
  public firstName?: string;
  public lastName?: string;
  public email?: string;
  public hdid?: string;
  public phn?: string;

  public getFullname(): string {
    return this.firstName + " " + this.lastName;
  }
}
