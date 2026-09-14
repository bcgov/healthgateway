import { EntryType } from "@/constants/entryType";
import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import ImmunizationResult from "@/models/immunizationResult";
import {
    IHttpDelegate,
    IImmunizationService,
    ILogger,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestImmunizationService implements IImmunizationService {
    private readonly API_VERSION: string = "2.0";
    private readonly IMMS_BASE_URI: string = "Immunization";
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
        this.baseUri = config.serviceEndpoints["Immunization"];
        this.isEnabled = ConfigUtil.isDatasetEnabled(EntryType.Immunization);
    }

    public getPatientImmunizations(hdid: string): Promise<ImmunizationResult> {
        this.logger.debug(`Get patient immunizations for hdid: ${hdid}`);
        if (!this.isEnabled) {
            return Promise.resolve({
                immunizations: [],
                recommendations: [],
            });
        }

        return this.http
            .getWithCors<ImmunizationResult>(
                `${this.baseUri}${this.IMMS_BASE_URI}?hdid=${hdid}&api-version=${this.API_VERSION}`
            )
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestImmunizationService.getPatientImmunizations()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Immunization
                );
            });
    }
}
