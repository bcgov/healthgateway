import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/serviceCodes";
import { ServiceName } from "@/constants/serviceName";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError, ResultError } from "@/models/errors";
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

const datasetTypeMap: Map<PatientDataType, EntryType> = new Map<
    PatientDataType,
    EntryType
>([[PatientDataType.DiagnosticImaging, EntryType.DiagnosticImaging]]);

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

    public getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse> {
        const delimiter = "patientDataTypes=";
        const patientDataTypeQueryArray =
            delimiter + patientDataTypes.join(`&${delimiter}`);

        this.canProcessRequest(patientDataTypes);

        return this.http
            .getWithCors<PatientDataResponse>(
                `${this.baseUri}${this.BASE_URI}/${hdid}?${patientDataTypeQueryArray}&api-version=2.0`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestPatientDataService.getPatientData()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.PatientData
                );
            });
    }

    public getFile(hdid: string, fileId: string): Promise<PatientDataFile> {
        return this.http
            .getWithCors<PatientDataFile>(
                `${this.baseUri}${this.BASE_URI}/${hdid}/file/${fileId}?api-version=2.0`
            )
            .catch((err: HttpError) => {
                this.logger.error(`Error in RestPatientDataService.getFile()`);
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.PatientData
                );
            });
    }

    private canProcessRequest(patientDataTypes: PatientDataType[]) {
        for (const patientDataType of patientDataTypes) {
            const serviceName = serviceTypeMap.get(patientDataType);
            const datasetName = datasetTypeMap.get(patientDataType);

            if (serviceName && !ConfigUtil.isServiceEnabled(serviceName)) {
                throw {
                    resultMessage: `Service ${serviceName} is not enabled`,
                } as ResultError;
            }

            if (datasetName && !ConfigUtil.isDatasetEnabled(datasetName)) {
                throw {
                    resultMessage: `Dataset ${datasetName} is not enabled`,
                } as ResultError;
            }
        }
    }
}
