import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import ImmunizationResult from "@/models/immunizationResult";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    IImmunizationService,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestImmunizationService implements IImmunizationService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
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
        this.isEnabled = config.webClient.modules["Immunization"];
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
