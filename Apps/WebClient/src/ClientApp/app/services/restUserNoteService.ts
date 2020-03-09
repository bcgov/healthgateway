import { injectable } from "inversify";
import { IHttpDelegate, IUserNoteService } from "@/services/interfaces";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";

@injectable()
export class RestUserNoteService implements IUserNoteService {
  private readonly USER_NOTE_BASE_URI: string = "v1/api/Note";
  private http!: IHttpDelegate;

  public initialize(http: IHttpDelegate): void {
    this.http = http;
  }

  public getNotes(): Promise<RequestResult<UserNote[]>> {
    return new Promise((resolve, reject) => {
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
    throw new Error(this.NOT_IMPLENTED);
  }

  public updateNote(noteId: string): Promise<UserNote> {
    throw new Error(this.NOT_IMPLENTED);
  }

  public deleteNote(noteId: string): Promise<void> {
    throw new Error(this.NOT_IMPLENTED);
  }
}
