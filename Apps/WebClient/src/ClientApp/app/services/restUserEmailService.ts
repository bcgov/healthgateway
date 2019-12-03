import { injectable } from "inversify";
import { IHttpDelegate, IUserEmailService } from "@/services/interfaces";
import EmailInvite from "@/models/emailInvite";

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

  public getLatestInvite(hdid: string): Promise<EmailInvite> {
    return new Promise(resolve => {
      this.http
        .get<EmailInvite>(`${this.USER_EMAIL_BASE_URI}/${hdid}`)
        .then(emailInvite => {
          return resolve(emailInvite);
        })
        .catch(err => {
          console.log(err);
          return resolve(err);
        });
    });
  }
}
