import { injectable } from "inversify";
import { IHttpDelegate, IUserCommentService } from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import UserComment from "@/models/userComment";
import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";

@injectable()
export class RestUserCommentService implements IUserCommentService {
  NOT_IMPLENTED: string = "Method not implemented.";
  private readonly USER_COMMENT_BASE_URI: string = "v1/api/Comment";
  private http!: IHttpDelegate;
  private isEnabled: boolean = false;

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.http = http;
    this.isEnabled = config.webClient.modules["Comment"];
  }

  public getCommentsForEntry(parentEntryId: string): Promise<RequestResult<UserComment[]>> {
    return new Promise((resolve, reject) => {
      this.http
        .getWithCors<RequestResult<UserComment[]>>(
          `${this.USER_COMMENT_BASE_URI}/`
        )
        .then(userComments => {
          // let test = [
          //   {
          //     id: 1,
          //     hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
          //     parentEntry: "id-0.9130414104642899",
          //     text: "Sample comment 1 for Methadone",
          //     commentDateTime: new Date(2020, 4, 18),
          //     version: "11503"
          //   },
          //   {
          //     id: 1,
          //     hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
          //     parentEntry: "id-0.9130414104642899",
          //     text: "Sample comment 2 for Methadone",
          //     commentDateTime: new Date(2020, 4, 15),
          //     version: "11503"
          //   },
          //   {
          //     id: 1,
          //     hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
          //     parentEntry: "id-0.41768123314459693",
          //     text: "Sample comment for Mint-Zopiclone",
          //     commentDateTime: new Date(2020, 4, 10),
          //     version: "11503"
          //   },
          //   {
          //     id: 1,
          //     hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
          //     parentEntry: "id-0.7073512160505682",
          //     text: "Sample comment 1 for Pms-Cyclobenzaprine - Tab 10mg",
          //     commentDateTime: new Date(2020, 4, 18),
          //     version: "11503"
          //   },
          //   {
          //     id: 1,
          //     hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
          //     parentEntry: "id-0.7073512160505682",
          //     text: "Sample comment 2 for Pms-Cyclobenzaprine - Tab 10mg",
          //     commentDateTime: new Date(2020, 4, 17),
          //     version: "11503"
          //   },
          // ]
          // console.log("TEST DATA: ", test);
          resolve(userComments);
          console.log("Comments retreived: ", userComments);
          return(userComments);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public createComment(comment: UserComment): Promise<UserComment> {
    return new Promise((resolve, reject) => {
      this.http
        .post<RequestResult<UserComment>>(
          `${this.USER_COMMENT_BASE_URI}/`,
          comment
        )
        .then(result => {
          return this.handleResult(result, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public updateComment(comment: UserComment): Promise<UserComment> {
    return new Promise((resolve, reject) => {
      this.http
        .put<RequestResult<UserComment>>(
          `${this.USER_COMMENT_BASE_URI}/`,
          comment
        )
        .then(result => {
          return this.handleResult(result, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public deleteComment(comment: UserComment): Promise<void> {
    return new Promise((resolve, reject) => {
      this.http
        .delete<RequestResult<UserComment>>(
          `${this.USER_COMMENT_BASE_URI}/`,
          comment
        )
        .then(result => {
          return this.handleResult(result, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  private handleResult(
    requestResult: RequestResult<any>,
    resolve: any,
    reject: any
  ) {
    if (requestResult.resultStatus === ResultType.Success) {
      resolve(requestResult.resourcePayload);
    } else {
      reject(requestResult.resultMessage);
    }
  }
}
