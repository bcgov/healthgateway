import { injectable } from "inversify";
import { IHttpDelegate, IPatientService } from "@/services/interfaces";
import PatientData from "@/models/patientData";
import { ExternalConfiguration } from "@/models/configData";

@injectable()
export class RestPatientService implements IPatientService {
    private readonly PATIENT_BASE_URI: string = "v1/api/Patient";
    private baseUri: string = "";
    private http!: IHttpDelegate;
    constructor() {}

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Patient"];
        this.http = http;
    }

    public getPatientData(hdid: string): Promise<PatientData> {
        return this.http.get<PatientData>(
            `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}`
        );
    }
}
