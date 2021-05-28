import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import { ServiceName } from "@/models/errorInterfaces";
import ImmunizationResult from "@/models/immunizationResult";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IHttpDelegate,
    IImmunizationService,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestImmunizationService implements IImmunizationService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly IMMS_BASE_URI: string = "v1/api/Immunization";
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
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Immunization
                        )
                    );
                });
        });
    }
}
