import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Patient from "@/models/patient";
import { IHttpDelegate, ILogger, IPatientService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestPatientService implements IPatientService {
    private readonly PATIENT_BASE_URI: string = "Patient";
    private logger;
    private http;
    private baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["Patient"];
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
