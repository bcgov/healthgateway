import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import MedicationRequest from "@/models/MedicationRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    ISpecialAuthorityService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestSpecialAuthorityService implements ISpecialAuthorityService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
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
        this.isEnabled = config.webClient.modules["MedicationRequest"];
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
