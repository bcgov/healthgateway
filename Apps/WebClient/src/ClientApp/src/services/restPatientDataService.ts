import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import PatientDataResponse, {
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IPatientDataService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestPatientDataService implements IPatientDataService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly BASE_URI = "PatientData";
    private serviceBaseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    private isServicesEnabled(reject: (reason?: unknown) => void) {
        if (!this.isEnabled) {
            reject(new Error("Services feature is disabled."));
        }
    }

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.serviceBaseUri = config.serviceEndpoints["PatientData"];
        this.http = http;
        this.isEnabled = ConfigUtil.isServicesFeatureEnabled();
    }

    public getPatientData(
        hdid: string,
        patientDataTypes: PatientDataType[]
    ): Promise<PatientDataResponse> {
        const delimiter = "patientDataTypes=";
        const patientDataTypeQueryArray =
            delimiter + patientDataTypes.join(`&${delimiter}`);
        return new Promise((resolve, reject) => {
            this.isServicesEnabled(reject);
            this.http
                .getWithCors<PatientDataResponse>(
                    `${this.serviceBaseUri}${this.BASE_URI}/${hdid}?${patientDataTypeQueryArray}&api-version=2.0`
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

    getFile(hdid: string, fileId: string): Promise<PatientDataFile> {
        return new Promise((resolve, reject) => {
            this.isServicesEnabled(reject);
            this.http
                .getWithCors<PatientDataFile>(
                    `${this.serviceBaseUri}${this.BASE_URI}/${hdid}/file/${fileId}?api-version=2.0`
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
