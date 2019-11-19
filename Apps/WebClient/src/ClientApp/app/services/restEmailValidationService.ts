import { injectable } from "inversify";
import { IHttpDelegate, IEmailValidationService } from "@/services/interfaces";

@injectable()
export class RestEmailValidationService implements IEmailValidationService {
  private readonly VALIDATE_EMAIL_BASE_URI: string = "v1/api/ValidateEmail";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public validateEmail(inviteKey: string): Promise<boolean> {
    return new Promise((resolve) => {
      this.http
          .get(`${this.VALIDATE_EMAIL_BASE_URI}/${inviteKey}`)
        .then(() => {
          return resolve(true);
        })
          .catch(err => {
              console.log(err);
            return resolve(false);
        });
    });
  }
}
