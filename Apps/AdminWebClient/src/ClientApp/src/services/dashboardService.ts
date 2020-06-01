import { injectable } from "inversify";
import { IHttpDelegate, IDashboardService } from "@/services/interfaces";

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
        .get<number>(`${this.BASE_URI}/RegisteredCount`)
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public getWaitlistedUsersCount(): Promise<number> {
    return new Promise((resolve, reject) => {
      this.http
        .get<number>(`${this.BASE_URI}/WaitlistCount`)
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public getLoggedInUsersCount(): Promise<number> {
    return new Promise((resolve, reject) => {
      this.http
        .get<number>(
          `${
            this.BASE_URI
          }/LoggedInCount?offset=${new Date().getTimezoneOffset()}`
        )
        .then(requestResult => {
          resolve(requestResult);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public getUnregisteredInvitedUsersCount(): Promise<number> {
    return new Promise((resolve, reject) => {
      this.http
        .get<number>(`${this.BASE_URI}/UnregisteredInvitedCount`)
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
