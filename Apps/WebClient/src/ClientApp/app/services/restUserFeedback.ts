import { injectable } from "inversify";
import {
  IHttpDelegate,
  IUserFeedbackService
} from "@/services/interfaces";
import UserFeedback from "@/models/userFeedback";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
  private readonly USER_FEEDBACK_BASE_URI: string = "v1/api/UserFeedback";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public submitFeedback(feedback: UserFeedback): Promise<boolean> {
    return new Promise((resolve, reject) => {
      this.http
        .post<boolean>(this.USER_FEEDBACK_BASE_URI, feedback)
        .then(result => {
          console.log(result);
          return resolve(result === true);
        })
        .catch(err => {
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }
}
