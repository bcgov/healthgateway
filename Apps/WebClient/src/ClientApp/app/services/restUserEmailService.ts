import { injectable } from "inversify";
import { IHttpDelegate, IUserEmailService } from "@/services/interfaces";
import UserEmailInvite from "@/models/userEmailInvite";
import { Dictionary } from "vue-router/types/router";

@injectable()
export class RestUserEmailService implements IUserEmailService {
  private readonly USER_EMAIL_BASE_URI: string = "v1/api/UserEmail";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public validateEmail(inviteKey: string): Promise<boolean> {
    return new Promise(resolve => {
      this.http
        .get(`${this.USER_EMAIL_BASE_URI}/Validate/${inviteKey}`)
        .then(() => {
          return resolve(true);
        })
        .catch(err => {
          console.log(err);
          return resolve(false);
        });
    });
  }

  public getLatestInvite(hdid: string): Promise<UserEmailInvite> {
    return new Promise(resolve => {
      this.http
        .get<UserEmailInvite>(`${this.USER_EMAIL_BASE_URI}/${hdid}`)
        .then(userEmailInvite => {
          return resolve(userEmailInvite);
        })
        .catch(err => {
          console.log(err);
          return resolve(err);
        });
    });
  }

  public updateEmail(hdid: string, email: string): Promise<boolean> {
    return new Promise(resolve => {
      let headers: Dictionary<string> = {};
      headers["Content-Type"] = "application/json; charset=utf-8";

      this.http
        .put<void>(
          `${this.USER_EMAIL_BASE_URI}/${hdid}`,
          JSON.stringify(email),
          headers
        )
        .then(() => {
          return resolve();
        })
        .catch(err => {
          console.log(err);
          return resolve(err);
        });
    });
  }
}
