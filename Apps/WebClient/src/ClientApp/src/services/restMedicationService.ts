import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import {
    IHttpDelegate,
    ILogger,
    IMedicationService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import RequestResultUtil from "@/utility/requestResultUtil";

@injectable()
export class RestMedicationService implements IMedicationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly MEDICATION_STATEMENT_BASE_URI: string =
        "v1/api/MedicationStatement";
    private readonly MEDICATION_BASE_URI: string = "v1/api/Medication";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private readonly FETCH_ERROR = "Fetch error:";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Medication"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Medication"];
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
                            ServiceName.Medication
                        )
                    );
                });
        });
    }

    public getMedicationInformation(
        drugIdentifier: string
    ): Promise<MedicationResult> {
        return new Promise((resolve, reject) => {
            return this.http
                .get<RequestResult<MedicationResult>>(
                    `${this.baseUri}${this.MEDICATION_BASE_URI}/${drugIdentifier}`
                )
                .then((requestResult) => {
                    return RequestResultUtil.handleResult(
                        requestResult,
                        resolve,
                        reject
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `getMedicationInformation ${this.FETCH_ERROR}: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Medication
                        )
                    );
                });
        });
    }
}
