import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/serviceCodes";
import { ServiceName } from "@/constants/serviceName";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import PatientDataResponse, {
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import {
    IHttpDelegate,
    ILogger,
    IPatientDataService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

const serviceTypeMap: Map<PatientDataType, ServiceName> = new Map<
    PatientDataType,
    ServiceName
>([
    [
        PatientDataType.OrganDonorRegistrationStatus,
        ServiceName.OrganDonorRegistration,
    ],
]);

export const patientDataTypeToEntryTypeMap: Map<PatientDataType, EntryType> =
    new Map<PatientDataType, EntryType>([
        [PatientDataType.DiagnosticImaging, EntryType.DiagnosticImaging],
        [PatientDataType.CancerScreening, EntryType.CancerScreening],
    ]);

export const entryTypeToPatientdataTypeMap: Map<EntryType, PatientDataType> =
    new Map<EntryType, PatientDataType>([
        [EntryType.DiagnosticImaging, PatientDataType.DiagnosticImaging],
        [EntryType.CancerScreening, PatientDataType.CancerScreening],
    ]);

export class RestPatientDataService implements IPatientDataService {
    private readonly BASE_URI = "PatientData";
    private logger;
    private http;
    private baseUri;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["PatientData"];
    }

    private canProcessRequest(
        patientDataTypes: PatientDataType[],
        reject: (reason?: unknown) => void
    ) {
        patientDataTypes.forEach((patientDataType) => {
            const serviceName = serviceTypeMap.get(patientDataType);
            const datasetName =
                patientDataTypeToEntryTypeMap.get(patientDataType);

            if (serviceName && !ConfigUtil.isServiceEnabled(serviceName)) {
                reject(`Service ${serviceName} is not enabled`);
            }

            if (datasetName && !ConfigUtil.isDatasetEnabled(datasetName)) {
                reject(`Dataset ${datasetName} is not enabled`);
            }
        });
    }

    public getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse> {
        const delimiter = "patientDataTypes=";
        const patientDataTypeQueryArray =
            delimiter + patientDataTypes.join(`&${delimiter}`);
        return new Promise((resolve, reject) => {
            this.canProcessRequest(patientDataTypes, reject);
            this.http
                .getWithCors<PatientDataResponse>(
                    `${this.baseUri}${this.BASE_URI}/${hdid}?${patientDataTypeQueryArray}&api-version=2.0`
                )
                .then(resolve)
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestPatientDataService.getPatientData()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.PatientData
                        )
                    );
                });
        });
    }

    public getFile(hdid: string, fileId: string): Promise<PatientDataFile> {
        return new Promise((resolve, reject) => {
            this.http
                .getWithCors<PatientDataFile>(
                    `${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}?api-version=2.0`
                )
                .then(resolve)
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestPatientDataService.getFile()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.PatientData
                        )
                    );
                });
        });
    }
}
