import { IConfigService, IHttpDelegate } from "@/services/interfaces";
import { injectable } from "inversify";
import ExternalConfiguration from "@/models/externalConfiguration";
import store from "@/store/store";

@injectable()
export class RestConfigService implements IConfigService {
    private readonly CONFIG_BASE_URI: string = "v1/api/configuration";
    private http!: IHttpDelegate;
    private get csvExportBaseUri(): string {
        return store.getters.serviceEndpoints["CsvExportBaseUri"];
    }
    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }
    public getConfiguration(): Promise<ExternalConfiguration> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
                .then(result => {
                    return resolve(result);
                })
                .catch(err => {
                    console.log("Fetch error:" + err.toString());
                    reject(err);
                });
        });
    }
    public getUserProfilesExportUrl(): string {
        debugger;
        return `${this.csvExportBaseUri}/GetUserProfiles`;
    }
    public getUserNotesExportUrl(): string {
        return `${this.csvExportBaseUri}/GetNotes`;
    }
    public getUserCommentsExportUrl(): string {
        return `${this.csvExportBaseUri}/GetComments`;
    }
}
