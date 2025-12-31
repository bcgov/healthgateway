import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import MedicationStatement from "@/models/medicationStatement";
import RequestResult from "@/models/requestResult";
import {
    IHttpDelegate,
    ILogger,
    IMedicationService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestMedicationService implements IMedicationService {
    private readonly BASE_URI: string = "MedicationStatement";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["Medication"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Medication);
    }

    public getPatientMedicationStatements(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatement[]>> {
        const headers: Dictionary<string> = {};
        if (protectiveWord) {
            headers["protectiveWord"] = protectiveWord;
        }

        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .get<
                RequestResult<MedicationStatement[]>
            >(`${this.baseUri}${this.BASE_URI}/${hdid}`, headers)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestMedicationService.getPatientMedicationStatements()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Medication
                );
            });
    }
}
