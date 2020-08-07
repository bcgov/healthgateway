import { injectable } from "inversify";
import { IHttpDelegate, ICsvExportService } from "@/services/interfaces";
import { ResultType } from "@/constants/resulttype";

@injectable()
export class RestCsvExportService implements ICsvExportService {
    private readonly BASE_URI: string = "v1/api/CsvExport";
    private http!: IHttpDelegate;

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }
    public getUserProfiles(): Promise<void> {
        return this.getCsvExport("GetUserProfiles");
    }
    public getComments(): Promise<void> {
        return this.getCsvExport("GetComments");
    }
    public getNotes(): Promise<void> {
        return this.getCsvExport("GetNotes");
    }
    private getCsvExport(route: string): Promise<void> {
        return new Promise((resolve, reject) => {
            this.http
                .get(`${this.BASE_URI}/${route}`)
                .then(requestResult => {
                    this.handleResult(requestResult, resolve, reject);
                })
                .catch(err => {
                    console.log(err);
                    return reject(err);
                });
        });
    }
    private handleResult(requestResult: any, resolve: any, reject: any) {
        if (requestResult.resultStatus === ResultType.Success) {
            resolve(requestResult.resourcePayload);
        } else {
            reject(requestResult.resultMessage);
        }
    }
}
