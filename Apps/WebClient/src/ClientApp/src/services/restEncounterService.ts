import { EntryType } from "@/constants/entryType";
import { ResultType } from "@/constants/resulttype";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { Encounter } from "@/models/encounter";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import {
    IEncounterService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestEncounterService implements IEncounterService {
    private readonly ENCOUNTER_BASE_URI: string = "Encounter";
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
        this.baseUri = config.serviceEndpoints["Encounter"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.HealthVisit);
    }

    public getPatientEncounters(
        hdid: string
    ): Promise<RequestResult<Encounter[]>> {
        if (!this.isEnabled) {
            return Promise.resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        }

        return this.http
            .getWithCors<
                RequestResult<Encounter[]>
            >(`${this.baseUri}${this.ENCOUNTER_BASE_URI}/${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestEncounterService.getPatientEncounters()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Encounter
                );
            });
    }
}
