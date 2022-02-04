import { injectable } from "inversify";

import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import PatientData from "@/models/patientData";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IHttpDelegate, ILogger, IPatientService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestPatientService implements IPatientService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
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
        return new Promise((resolve, reject) => {
            return this.http
                .get<RequestResult<PatientData>>(
                    `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Immunization
                        )
                    );
                });
        });
    }
}
