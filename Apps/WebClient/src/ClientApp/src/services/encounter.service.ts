import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import type { Encounter } from "@/models/encounter";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IEncounterService {
    getPatientEncounters(hdid: string): Promise<RequestResult<Encounter[]>>;
}

/** Port of services/restEncounterService.ts */
export class EncounterService implements IEncounterService {
    private readonly ENCOUNTER_BASE_URI = "Encounter";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["Encounter"];
    }

    async getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.HealthVisit))
            return {
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            };
        try {
            return await this.http.get<RequestResult<Encounter[]>>(
                `${this.baseUri}${this.ENCOUNTER_BASE_URI}/${hdid}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestEncounterService.getPatientEncounters()"
            );
            throw toResultError(error, ServiceCode.Encounter);
        }
    }
}
