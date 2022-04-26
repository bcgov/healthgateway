import { injectable } from "inversify";

import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { ServiceName } from "@/models/errorInterfaces";
import RequestResult from "@/models/requestResult";
import VaccinationStatus from "@/models/vaccinationStatus";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IVaccinationStatusService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestVaccinationStatusService implements IVaccinationStatusService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly PUBLIC_VACCINATION_STATUS_BASE_URI: string =
        "v1/api/PublicVaccineStatus";
    private readonly AUTHENTICATED_VACCINATION_STATUS_BASE_URI: string =
        "v1/api/AuthenticatedVaccineStatus";
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

    public getPublicVaccineStatus(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<VaccinationStatus>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject(
                    ErrorTranslator.moduleDisabledError(
                        ServiceName.Immunization
                    )
                );
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["dateOfVaccine"] = dateOfVaccine;

            return this.http
                .getWithCors<RequestResult<VaccinationStatus>>(
                    `${this.baseUri}${this.PUBLIC_VACCINATION_STATUS_BASE_URI}`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `Fetch public vaccine status error: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Immunization
                        )
                    );
                });
        });
    }

    public getPublicVaccineStatusPdf(
        phn: string,
        dateOfBirth: StringISODate,
        dateOfVaccine: StringISODate
    ): Promise<RequestResult<CovidVaccineRecord>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject(
                    ErrorTranslator.moduleDisabledError(
                        ServiceName.Immunization
                    )
                );
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["dateOfVaccine"] = dateOfVaccine;
            return this.http
                .getWithCors<RequestResult<CovidVaccineRecord>>(
                    `${this.baseUri}${this.PUBLIC_VACCINATION_STATUS_BASE_URI}/pdf`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `Fetch public vaccine proof error: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Immunization
                        )
                    );
                });
        });
    }

    public getAuthenticatedVaccineStatus(
        hdid: string
    ): Promise<RequestResult<VaccinationStatus>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject(
                    ErrorTranslator.moduleDisabledError(
                        ServiceName.Immunization
                    )
                );
                return;
            }

            return this.http
                .getWithCors<RequestResult<VaccinationStatus>>(
                    `${this.baseUri}${this.AUTHENTICATED_VACCINATION_STATUS_BASE_URI}?hdid=${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `Fetch authenticated vaccine status error: ${err}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceName.Immunization
                        )
                    );
                });
        });
    }

    public getAuthenticatedVaccineRecord(
        hdid: string
    ): Promise<RequestResult<CovidVaccineRecord>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject(
                    ErrorTranslator.moduleDisabledError(
                        ServiceName.Immunization
                    )
                );
                return;
            }

            return this.http
                .getWithCors<RequestResult<CovidVaccineRecord>>(
                    `${this.baseUri}${this.AUTHENTICATED_VACCINATION_STATUS_BASE_URI}/pdf?hdid=${hdid}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(
                        `Fetch authenticated vaccine proof error: ${err}`
                    );
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
