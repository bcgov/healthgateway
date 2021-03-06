import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ExternalConfiguration } from "@/models/configData";
import Encounter from "@/models/encounter";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IEncounterService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestEncounterService implements IEncounterService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly ENCOUNTER_BASE_URI: string = "v1/api/Encounter";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Encounter"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Encounter"];
    }

    public getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: [],
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<Encounter[]>>(
                    `${this.baseUri}${this.ENCOUNTER_BASE_URI}/${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch error: ${err}`);
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Encounter
                        )
                    );
                });
        });
    }
}
