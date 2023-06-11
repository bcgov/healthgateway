import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import MedicationRequest from "@/models/medicationRequest";
import RequestResult from "@/models/requestResult";
import {
    IHttpDelegate,
    ILogger,
    ISpecialAuthorityService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import { WinstonLogger } from "@/services/winstonLogger";

export class RestSpecialAuthorityService implements ISpecialAuthorityService {
    private logger: ILogger = new WinstonLogger(true); // TODO: inject logger
    private readonly SPECIAL_AUTHORITY_BASE_URI: string = "MedicationRequest";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["SpecialAuthority"];
        this.http = http;
        this.isEnabled = ConfigUtil.isDatasetEnabled(
            EntryType.SpecialAuthorityRequest
        );
    }

    public getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .get<RequestResult<MedicationRequest[]>>(
                    `${this.baseUri}${this.SPECIAL_AUTHORITY_BASE_URI}/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestSpecialAuthorityService.getPatientMedicationRequest()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.SpecialAuthority
                        )
                    );
                });
        });
    }
}
