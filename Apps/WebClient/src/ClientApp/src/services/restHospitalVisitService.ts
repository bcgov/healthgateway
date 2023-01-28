import { injectable } from "inversify";

import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHospitalVisitService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestHospitalVisitService implements IHospitalVisitService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly HOSPITAL_VISIT_BASE_URI: string = "Encounter";
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["HospitalVisit"];
        this.http = http;
        this.isEnabled = config.webClient.modules["HospitalVisit"];
    }

    public getHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
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
                    `${this.baseUri}${this.HOSPITAL_VISIT_BASE_URI}/HospitalVisit/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestHospitalVisitService.getHospitalVisits()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HospitalVisit
                        )
                    );
                });
        });
    }
}
