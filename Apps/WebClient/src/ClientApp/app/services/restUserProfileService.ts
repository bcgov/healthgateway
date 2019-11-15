import { injectable } from "inversify";
import { IHttpDelegate, IUserProfileService } from "@/services/interfaces";
import UserProfile from "@/models/userProfile";

@injectable()
export class RestUserProfileService implements IUserProfileService {
  private readonly USER_PROFILE_BASE_URI: string = "v1/api/UserProfile";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getProfile(hdid: string): Promise<UserProfile> {
    return new Promise((resolve, reject) => {
      this.http
        .getWithCors<UserProfile>(`${this.USER_PROFILE_BASE_URI}/${hdid}`)
        .then(result => {
          return resolve(result);
        })
        .catch(err => {
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }

  public createProfile(profile: UserProfile): Promise<boolean> {
    return new Promise((resolve, reject) => {
      this.http
        .post<boolean>(`${this.USER_PROFILE_BASE_URI}/${profile.hdid}`, profile)
        .then(result => {
          console.log(result);
          return resolve(result);
        })
        .catch(err => {
          console.log("Fetch error:" + err.toString());
          reject(err);
        });
    });
  }
}
