import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { Encounter } from "@/models/encounter";
import { HttpError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IEncounterService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestEncounterService implements IEncounterService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly ENCOUNTER_BASE_URI: string = "Encounter";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEncounterEnabled = false;
    private isHospitalVisitEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Encounter"];
        this.http = http;
        this.isEncounterEnabled = config.webClient.modules["Encounter"];
        this.isHospitalVisitEnabled = config.webClient.modules["HospitalVisit"];
    }

    public getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        return new Promise((resolve, reject) => {
            if (!this.isEncounterEnabled) {
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
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestEncounterService.getPatientEncounters()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Encounter
                        )
                    );
                });
        });
    }

    public getHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isHospitalVisitEnabled) {
                resolve({
                    pageIndex: 0,
                    pageSize: 0,
                    resourcePayload: {
                        loaded: true,
                        queued: false,
                        retryin: 0,
                        hospitalVisits: [],
                    },
                    resultStatus: ResultType.Success,
                    totalResultCount: 0,
                });
                return;
            }
            this.http
                .getWithCors<RequestResult<HospitalVisitResult>>(
                    `${this.baseUri}${this.ENCOUNTER_BASE_URI}/HospitalVisit/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestEncounterService.getHospitalVisits()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Encounter
                        )
                    );
                });
        });
    }
}
