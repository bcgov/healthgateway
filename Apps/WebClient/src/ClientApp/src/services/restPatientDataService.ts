import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import PatientData from "@/models/patientData";
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

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.serviceBaseUri = config.serviceEndpoints["PatientData"];
        this.http = http;
        this.isEnabled = ConfigUtil.isServicesFeatureEnabled();
    }
    public getPatientData(hdid: string): Promise<PatientData | undefined> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve(undefined);
            }
            this.http
                .getWithCors<PatientData>(
                    `${this.serviceBaseUri}${this.BASE_URI}/${hdid}`
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

    getFile(hdid: string, fileId: string): Promise<File> {
        throw new Error("Not implemented");
    }
}
