import { injectable } from "inversify";
import { IHttpDelegate, IUserNoteService } from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import { ResultType } from '@/constants/resulttype';
import { ExternalConfiguration } from '@/models/configData';

@injectable()
export class RestUserNoteService implements IUserNoteService {
  private readonly USER_NOTE_BASE_URI: string = "v1/api/Note";
  private http!: IHttpDelegate;
  private isEnabled: boolean = false;

  public initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    this.http = http;
    this.isEnabled = config.webClient.modules["Note"];
  }

  public getNotes(): Promise<RequestResult<UserNote[]>> {
    return new Promise((resolve, reject) => {
      if (!this.isEnabled) {
        resolve({
          pageIndex: 0,
          pageSize: 0,
          resourcePayload: [],
          resultMessage: "",
          resultStatus: ResultType.Success,
          totalResultCount: 0
        });
        return;
      }

      this.http
        .getWithCors<RequestResult<UserNote[]>>(`${this.USER_NOTE_BASE_URI}/`)
        .then(userNotes => {
          return resolve(userNotes);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  NOT_IMPLENTED: string = "Method not implemented.";

  public createNote(note: UserNote): Promise<UserNote> {
    return new Promise((resolve, reject) => {
      if (!this.isEnabled) {
        resolve();
        return;
      }

      this.http
        .post<RequestResult<UserNote>>(`${this.USER_NOTE_BASE_URI}/`, note)
        .then(result => {
          return this.handleResult(result, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public updateNote(note: UserNote): Promise<UserNote> {
    return new Promise((resolve, reject) => {
      this.http
        .put<RequestResult<UserNote>>(`${this.USER_NOTE_BASE_URI}/`, note)
        .then(result => {
          return this.handleResult(result, resolve, reject);
        })
        .catch(err => {
          console.log(err);
          return reject(err);
        });
    });
  }

  public deleteNote(noteId: string): Promise<void> {
    throw new Error(this.NOT_IMPLENTED);
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
