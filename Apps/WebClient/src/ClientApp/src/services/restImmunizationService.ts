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

export class RestImmunizationService implements IImmunizationService {
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

    public getPatientImmunizations(
        hdid: string
    ): Promise<RequestResult<ImmunizationResult>> {
        this.logger.debug(`Get patient inmmunization for hdid::  ${hdid}`);
        if (!this.isEnabled) {
            return Promise.resolve({
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
            .getWithCors<
                RequestResult<ImmunizationResult>
            >(`${this.baseUri}${this.IMMS_BASE_URI}?hdid=${hdid}`)
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
