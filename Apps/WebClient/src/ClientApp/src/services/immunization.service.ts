import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import { ResultType } from "@/constants/resultType";
import type { ExternalConfiguration } from "@/models/configData";
import { HttpError, ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { ImmunizationResult } from "@/models/immunization";
import type { RequestResult } from "@/models/requestResult";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

interface ImmunizationResultV1 extends ImmunizationResult {
    loadState?: { refreshInProgress: boolean };
}

/** The web client's V1 store re-queried every 10 s. Bounded here so the records screen cannot wait forever. */
const V1_RETRY_MS = 10000;
const V1_MAX_ATTEMPTS = 12;

export interface IImmunizationService {
    getPatientImmunizations(hdid: string): Promise<ImmunizationResult>;
}

/** Port of services/restImmunizationService.ts */
export class ImmunizationService implements IImmunizationService {
    private readonly API_VERSION = "2.0";
    private readonly IMMS_BASE_URI = "Immunization";
    private readonly baseUri: string;
    /** this environment was found to serve V1 only; one instance serves one environment */
    private useV1 = false;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["Immunization"];
    }

    async getPatientImmunizations(hdid: string): Promise<ImmunizationResult> {
        logger.debug("Get patient immunizations");
        // read when called: the web client fixes it at construction, after the configuration has loaded
        if (!ConfigUtil.isDatasetEnabled(EntryType.Immunization))
            return { immunizations: [], recommendations: [] };
        try {
            if (this.useV1) return await this.getV1(hdid);
            try {
                return await this.http.get<ImmunizationResult>(
                    `${this.baseUri}${this.IMMS_BASE_URI}?hdid=${hdid}&api-version=${this.API_VERSION}`
                );
            } catch (error) {
                rethrowIfSessionEnded(error);
                if (!(error instanceof HttpError) || error.statusCode !== 400)
                    throw error;
            }
            // An environment not yet on V2 rejects the version with a 400; the wrong request would fail on V1 too
            const result = await this.getV1(hdid);
            this.useV1 = true;
            return result;
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error(
                "Error in RestImmunizationService.getPatientImmunizations()"
            );
            if (error instanceof ResultError) throw error;
            throw toResultError(error, ServiceCode.Immunization);
        }
    }

    /**
     * Test and Production still deploy V1 (checked 2026-10-31; the web client there calls it too). Remove this once they
     * serve V2. V1 wraps the result and may answer that a refresh is running, to be asked again shortly.
     */
    private async getV1(hdid: string): Promise<ImmunizationResult> {
        for (let attempt = 1; ; attempt++) {
            const result = await this.http.get<
                RequestResult<ImmunizationResultV1>
            >(`${this.baseUri}${this.IMMS_BASE_URI}?hdid=${hdid}`);
            if (result.resultStatus !== ResultType.Success) {
                throw result.resultError
                    ? ResultError.fromModel(result.resultError)
                    : new ResultError(
                          "ImmunizationService",
                          "Unknown API error on retrieve immunizations"
                      );
            }
            const { immunizations, recommendations, loadState } =
                result.resourcePayload;
            if (!loadState?.refreshInProgress || attempt >= V1_MAX_ATTEMPTS)
                return { immunizations, recommendations };
            logger.info("Immunizations load deferred");
            await new Promise((resolve) => setTimeout(resolve, V1_RETRY_MS));
        }
    }
}
