import { IConfigService, IHttpDelegate } from "@/services/interfaces";
import { injectable } from "inversify";
import ExternalConfiguration from "@/models/externalConfiguration";

@injectable()
export class RestConfigService implements IConfigService {
    private readonly CONFIG_BASE_URI: string = "v1/api/configuration";
    private http!: IHttpDelegate;
    private config: ExternalConfiguration = new ExternalConfiguration();

    public initialize(http: IHttpDelegate): void {
        this.http = http;
    }
    public getConfiguration(): Promise<ExternalConfiguration> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<ExternalConfiguration>(`${this.CONFIG_BASE_URI}/`)
                .then(result => {
                    this.config = result;
                    return resolve(result);
                })
                .catch(err => {
                    console.log("Fetch error:" + err.toString());
                    reject(err);
                });
        });
    }
    public getUserProfilesExportUrl(): string {
        return `${this.config.serviceEndpoints["CsvExportBaseUri"]}/GetUserProfiles`;
    }
    public getUserNotesExportUrl(): string {
        return `${this.config.serviceEndpoints["CsvExportBaseUri"]}/GetNotes`;
    }
    public getUserCommentsExportUrl(): string {
        return `${this.config.serviceEndpoints["CsvExportBaseUri"]}/GetComments`;
    }
}
