import { injectable } from "inversify";

import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IMedicationService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestMedicationService implements IMedicationService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly MEDICATION_STATEMENT_BASE_URI: string =
        "MedicationStatement";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Medication"];
        this.http = http;
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Medication);
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
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestMedicationService.getPatientMedicationStatementHistory()`
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
