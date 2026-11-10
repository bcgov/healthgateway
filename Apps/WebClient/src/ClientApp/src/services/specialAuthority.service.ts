import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { MedicationRequest } from "@/models/medicationRequest";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface ISpecialAuthorityService {
    getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>>;
}

/** Port of services/restSpecialAuthorityService.ts */
export class SpecialAuthorityService implements ISpecialAuthorityService {
    private readonly SPECIAL_AUTHORITY_BASE_URI = "MedicationRequest";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["SpecialAuthority"];
    }

    async getPatientMedicationRequest(
        hdid: string
    ): Promise<RequestResult<MedicationRequest[]>> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.SpecialAuthorityRequest))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<MedicationRequest[]>>(
                `${this.baseUri}${this.SPECIAL_AUTHORITY_BASE_URI}/${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestSpecialAuthorityService.getPatientMedicationRequest()"
            );
            throw toResultError(error, ServiceCode.SpecialAuthority);
        }
    }
}
