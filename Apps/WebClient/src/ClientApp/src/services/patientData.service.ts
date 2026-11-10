import { EntryType, ServiceName } from "@/constants/entryType";
import { ServiceCode } from "@/constants/errorType";
import type { ExternalConfiguration } from "@/models/configData";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import {
    type PatientDataFile,
    type PatientDataResponse,
    PatientDataType,
} from "@/models/patientData";
import { ConfigUtil } from "@/utils/configUtil";

import { toResultError } from "./errorTranslator";
import type { IHttpClient } from "./http/httpClient";
import { logger } from "./logger";

const serviceTypeMap = new Map<PatientDataType, ServiceName>([
    [
        PatientDataType.OrganDonorRegistrationStatus,
        ServiceName.OrganDonorRegistration,
    ],
]);

export const patientDataTypeToEntryTypeMap = new Map<
    PatientDataType,
    EntryType
>([
    [PatientDataType.DiagnosticImaging, EntryType.DiagnosticImaging],
    [PatientDataType.BcCancerScreening, EntryType.BcCancerScreening],
]);

export const entryTypeToPatientDataTypeMap = new Map<
    EntryType,
    PatientDataType
>([
    [EntryType.DiagnosticImaging, PatientDataType.DiagnosticImaging],
    [EntryType.BcCancerScreening, PatientDataType.BcCancerScreening],
]);

export interface IPatientDataService {
    getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse>;
    getFile(hdid: string, fileId: string): Promise<PatientDataFile>;
}

/** Port of services/restPatientDataService.ts (imaging, BC Cancer screening, organ donor status). */
export class PatientDataService implements IPatientDataService {
    private readonly BASE_URI = "PatientData";
    private readonly baseUri: string;

    constructor(
        private readonly http: IHttpClient,
        config: ExternalConfiguration
    ) {
        this.baseUri = config.serviceEndpoints["PatientData"];
    }

    async getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse> {
        const delimiter = "patientDataTypes=";
        const query = delimiter + patientDataTypes.join(`&${delimiter}`);
        this.canProcessRequest(patientDataTypes);
        try {
            return await this.http.get<PatientDataResponse>(
                `${this.baseUri}${this.BASE_URI}/${hdid}?${query}&api-version=2.0`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestPatientDataService.getPatientData()");
            throw toResultError(error, ServiceCode.PatientData);
        }
    }

    async getFile(hdid: string, fileId: string): Promise<PatientDataFile> {
        try {
            return await this.http.get<PatientDataFile>(
                `${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}?api-version=2.0`
            );
        } catch (error) {
            rethrowIfSessionEnded(error);
            logger.error("Error in RestPatientDataService.getFile()");
            throw toResultError(error, ServiceCode.PatientData);
        }
    }

    private canProcessRequest(patientDataTypes: PatientDataType[]) {
        for (const patientDataType of patientDataTypes) {
            const serviceName = serviceTypeMap.get(patientDataType);
            const datasetName =
                patientDataTypeToEntryTypeMap.get(patientDataType);
            if (serviceName && !ConfigUtil.isServiceEnabled(serviceName))
                throw new ResultError(
                    ServiceCode.PatientData,
                    `Service ${serviceName} is not enabled`
                );
            if (datasetName && !ConfigUtil.isDatasetEnabled(datasetName))
                throw new ResultError(
                    ServiceCode.PatientData,
                    `Dataset ${datasetName} is not enabled`
                );
        }
    }
}
