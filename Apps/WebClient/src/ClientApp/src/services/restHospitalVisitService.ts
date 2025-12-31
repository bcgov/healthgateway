import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import HospitalVisitResult from "@/models/hospitalVisitResult";
import RequestResult from "@/models/requestResult";
import {
    IHospitalVisitService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestHospitalVisitService implements IHospitalVisitService {
    private readonly HOSPITAL_VISIT_BASE_URI: string = "Encounter";
    private readonly logger;
    private readonly http;
    private readonly baseUri;
    private readonly isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["HospitalVisit"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.HospitalVisit);
    }

    public getHospitalVisits(
        hdid: string
    ): Promise<RequestResult<HospitalVisitResult>> {
        if (!this.isEnabled) {
            return Promise.resolve({
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
        }

        return this.http
            .getWithCors<
                RequestResult<HospitalVisitResult>
            >(`${this.baseUri}${this.HOSPITAL_VISIT_BASE_URI}/HospitalVisit/${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestHospitalVisitService.getHospitalVisits()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.HospitalVisit
                );
            });
    }
}
