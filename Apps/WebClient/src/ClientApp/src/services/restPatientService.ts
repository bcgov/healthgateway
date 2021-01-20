import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import PatientData from "@/models/patientData";
import RequestResult from "@/models/requestResult";
import { IHttpDelegate, IPatientService } from "@/services/interfaces";

@injectable()
export class RestPatientService implements IPatientService {
    private readonly PATIENT_BASE_URI: string = "v1/api/Patient";
    private baseUri = "";
    private http!: IHttpDelegate;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Patient"];
        this.http = http;
    }

    public getPatientData(hdid: string): Promise<RequestResult<PatientData>> {
        return this.http.get<RequestResult<PatientData>>(
            `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}`
        );
    }
}
