import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import ImmunizationResult from "@/models/immunizationResult";
import RequestResult from "@/models/requestResult";
import {
    IHttpDelegate,
    IImmunizationService,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";
import { WinstonLogger } from "@/services/winstonLogger";

export class RestImmunizationService implements IImmunizationService {
    private logger: ILogger = new WinstonLogger(true); // TODO: inject logger
    private readonly IMMS_BASE_URI: string = "Immunization";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Immunization"];
        this.http = http;
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Immunization);
    }

    public getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationResult>> {
        this.logger.debug(`Get patient inmmunization for hdid::  ${hdid}`);
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                return resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loadState: { refreshInProgress: false },
                        immunizations: [],
                        recommendations: [],
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
            }

            return this.http
                .getWithCors<RequestResult<ImmunizationResult>>(
                    `${this.baseUri}${this.IMMS_BASE_URI}?hdid=${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestImmunizationService.getPatientImmunizations()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Immunization
                        )
                    );
                });
        });
    }
}
