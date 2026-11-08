import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { rethrowIfSessionEnded } from "@/models/errors";
import type { Patient } from "@/models/patient";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

export interface IPatientService {
    getPatient(hdid: string): Promise<Patient>;
}

export class PatientService implements IPatientService {
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["Patient"];
    }

    async getPatient(hdid: string): Promise<Patient> {
        try {
            return await this.http.get<Patient>(
                `${this.baseUri}Patient/${hdid}?api-version=2.0`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in PatientService.getPatient()", error);
            throw toResultError(error, ServiceCode.Patient);
        }
    }
}
