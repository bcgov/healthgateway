import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import MedicationRequest from "@/models/MedicationRequest";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IMedicationService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestMedicationService implements IMedicationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly MEDICATION_STATEMENT_BASE_URI: string =
        "MedicationStatement";
    private readonly MEDICATION_REQUEST_BASE_URI: string = "MedicationRequest";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isMedicationEnabled = false;
    private isMedicationRequestEnabled = false;
    private readonly FETCH_ERROR = "Fetch error:";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Medication"];
        this.http = http;
        this.isMedicationEnabled = config.webClient.modules["Medication"];
        this.isMedicationRequestEnabled =
            config.webClient.modules["MedicationRequest"];
    }

    public getPatientMedicationStatementHistory(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        const headers: Dictionary<string> = {};
        if (protectiveWord) {
            headers["protectiveWord"] = protectiveWord;
        }
        return new Promise((resolve, reject) => {
            if (!this.isMedicationEnabled) {
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
                .get<RequestResult<MedicationStatementHistory[]>>(
                    `${this.baseUri}${this.MEDICATION_STATEMENT_BASE_URI}/${hdid}`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `getPatientMedicationStatementHistory ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Medication
                        )
                    );
                });
        });
    }

    public getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isMedicationRequestEnabled) {
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
                    `${this.baseUri}${this.MEDICATION_REQUEST_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `getPatientMedicationRequest ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Medication
                        )
                    );
                });
        });
    }
}
