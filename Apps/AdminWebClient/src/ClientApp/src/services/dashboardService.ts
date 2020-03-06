import { injectable } from "inversify";
import { IHttpDelegate, IDashboardService } from "@/services/interfaces";
import UserFeedback from "@/models/userFeedback";
import RequestResult from "@/models/requestResult";

@injectable()
export class DashboardService implements IDashboardService {
  private readonly BASE_URI: string = "v1/api/Dashboard";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getRegisteredUsersCount(): Promise<number> {
    return new Promise((resolve, reject) => {
      this.http
        .get<RequestResult<UserFeedback[]>>(`${this.BASE_URI}`)
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }
}
