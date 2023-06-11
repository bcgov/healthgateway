import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Patient from "@/models/patient";
import { IHttpDelegate, ILogger, IPatientService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import { WinstonLogger } from "@/services/winstonLogger";

export class RestPatientService implements IPatientService {
    private logger: ILogger = new WinstonLogger(true); // TODO: inject logger
    private readonly PATIENT_BASE_URI: string = "Patient";
    private baseUri = "";
    private http!: IHttpDelegate;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Patient"];
        this.http = http;
    }

    public getPatient(hdid: string): Promise<Patient> {
        return new Promise((resolve, reject) =>
            this.http
                .get<Patient>(
                    `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}?api-version=2.0`
                )
                .then((apiResult) => resolve(apiResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestPatientService.getPatient()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Patient
                        )
                    );
                })
        );
    }
}
