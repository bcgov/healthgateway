import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HospitalVisit } from "@/models/encounter";
import { HttpError } from "@/models/errors";
import {
    IHospitalVisitService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestHospitalVisitService implements IHospitalVisitService {
    private readonly API_VERSION: string = "2.0";
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

    public getHospitalVisits(hdid: string): Promise<HospitalVisit[]> {
        if (!this.isEnabled) {
            return Promise.resolve([]);
        }

        return this.http
            .getWithCors<
                HospitalVisit[]
            >(`${this.baseUri}${this.HOSPITAL_VISIT_BASE_URI}/HospitalVisit/${hdid}?api-version=${this.API_VERSION}`)
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
