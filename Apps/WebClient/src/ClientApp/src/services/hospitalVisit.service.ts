import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import type { HospitalVisit } from "@/models/encounter";
import { rethrowIfSessionEnded } from "@/models/errors";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IHospitalVisitService {
    getHospitalVisits(hdid: string): Promise<HospitalVisit[]>;
}

/** Port of services/restHospitalVisitService.ts (the API returns the visits, not a RequestResult). */
export class HospitalVisitService implements IHospitalVisitService {
    private readonly API_VERSION = "2.0";
    private readonly HOSPITAL_VISIT_BASE_URI = "Encounter";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["HospitalVisit"];
    }

    async getHospitalVisits(hdid: string): Promise<HospitalVisit[]> {
        if (!ConfigUtil.isDatasetEnabled(EntryType.HospitalVisit)) return [];
        try {
            return await this.http.get<HospitalVisit[]>(
                `${this.baseUri}${this.HOSPITAL_VISIT_BASE_URI}/HospitalVisit/${hdid}?api-version=${this.API_VERSION}`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestHospitalVisitService.getHospitalVisits()"
            );
            throw toResultError(error, ServiceCode.HospitalVisit);
        }
    }
}
