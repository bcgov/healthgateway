import { ServiceCode } from "@/constants/serviceCodes";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { HttpError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import VaccinationStatus from "@/models/vaccinationStatus";
import {
    IHttpDelegate,
    ILogger,
    IVaccinationStatusService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestVaccinationStatusService implements IVaccinationStatusService {
    private readonly PUBLIC_VACCINATION_STATUS_BASE_URI: string =
        "PublicVaccineStatus";
    private readonly AUTHENTICATED_VACCINATION_STATUS_BASE_URI: string =
        "AuthenticatedVaccineStatus";
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
        this.isEnabled = true;
    }

    public getPublicVaccineStatus(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<VaccinationStatus>> {
        if (!this.isEnabled) {
            return Promise.reject(
                ErrorTranslator.featureDisabledError(ServiceCode.Immunization)
            );
        }

        const headers: Dictionary<string> = {};
        headers["phn"] = phn;
        headers["dateOfBirth"] = dateOfBirth;
        headers["dateOfVaccine"] = dateOfVaccine;

        return this.http
            .getWithCors<
                RequestResult<VaccinationStatus>
            >(`${this.baseUri}${this.PUBLIC_VACCINATION_STATUS_BASE_URI}`, headers)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestVaccinationStatusService.getPublicVaccineStatus()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Immunization
                );
            });
    }

    public getPublicVaccineStatusPdf(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<CovidVaccineRecord>> {
        if (!this.isEnabled) {
            return Promise.reject(
                ErrorTranslator.featureDisabledError(ServiceCode.Immunization)
            );
        }

        const headers: Dictionary<string> = {};
        headers["phn"] = phn;
        headers["dateOfBirth"] = dateOfBirth;
        headers["dateOfVaccine"] = dateOfVaccine;

        return this.http
            .getWithCors<
                RequestResult<CovidVaccineRecord>
            >(`${this.baseUri}${this.PUBLIC_VACCINATION_STATUS_BASE_URI}/pdf`, headers)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestVaccinationStatusService.getPublicVaccineStatusPdf()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Immunization
                );
            });
    }

    public getAuthenticatedVaccineStatus(
        hdid: string
    ): Promise<RequestResult<VaccinationStatus>> {
        if (!this.isEnabled) {
            return Promise.reject(
                ErrorTranslator.featureDisabledError(ServiceCode.Immunization)
            );
        }

        return this.http
            .getWithCors<
                RequestResult<VaccinationStatus>
            >(`${this.baseUri}${this.AUTHENTICATED_VACCINATION_STATUS_BASE_URI}?hdid=${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestVaccinationStatusService.getAuthenticatedVaccineStatus()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Immunization
                );
            });
    }

    public getAuthenticatedVaccineRecord(
        hdid: string
    ): Promise<RequestResult<CovidVaccineRecord>> {
        if (!this.isEnabled) {
            return Promise.reject(
                ErrorTranslator.featureDisabledError(ServiceCode.Immunization)
            );
        }

        return this.http
            .getWithCors<
                RequestResult<CovidVaccineRecord>
            >(`${this.baseUri}${this.AUTHENTICATED_VACCINATION_STATUS_BASE_URI}/pdf?hdid=${hdid}`)
            .catch((err: HttpError) => {
                this.logger.error(
                    `Error in RestVaccinationStatusService.getAuthenticatedVaccineRecord()`
                );
                throw ErrorTranslator.internalNetworkError(
                    err,
                    ServiceCode.Immunization
                );
            });
    }
}
