import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import Patient from "@/models/patient";
import { IHttpDelegate, ILogger, IPatientService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestPatientService implements IPatientService {
    private readonly PATIENT_BASE_URI: string = "Patient";
    private readonly logger;
    private readonly http;
    private readonly baseUri;

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
        return this.http
            .get<Patient>(
                `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}?api-version=2.0`
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestPatientService.getPatient()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Patient
                );
            });
    }
}
