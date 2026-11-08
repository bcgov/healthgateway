import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { MedicationStatement } from "@/models/medication";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IMedicationService {
    getPatientMedicationStatements(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatement[]>>;
}

/** Port of services/restMedicationService.ts */
export class MedicationService implements IMedicationService {
    private readonly BASE_URI = "MedicationStatement";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["Medication"];
    }

    async getPatientMedicationStatements(
        hdid: string,
        protectiveWord?: string
    ): Promise<RequestResult<MedicationStatement[]>> {
        const headers: Record<string, string> = {};
        if (protectiveWord) headers["protectiveWord"] = protectiveWord;
        if (!ConfigUtil.isDatasetEnabled(EntryType.Medication))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<MedicationStatement[]>>(
                `${this.baseUri}${this.BASE_URI}/${hdid}`,
                headers
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestMedicationService.getPatientMedicationStatements()"
            );
            throw toResultError(error, ServiceCode.Medication);
        }
    }
}
