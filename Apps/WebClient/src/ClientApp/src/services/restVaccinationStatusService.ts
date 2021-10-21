import { injectable } from "inversify";

import { VaccineProofTemplate } from "@/constants/vaccineProofTemplate";
import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { ServiceName } from "@/models/errorInterfaces";
import Report from "@/models/report";
import RequestResult from "@/models/requestResult";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import {
    IHttpDelegate,
    ILogger,
    IVaccinationStatusService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestVaccinationStatusService implements IVaccinationStatusService {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private readonly VACCINATION_STATUS_BASE_URI: string =
        "v1/api/VaccineStatus";
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
                reject();
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["dateOfVaccine"] = dateOfVaccine;

            return this.http
                .getWithCors<RequestResult<VaccinationStatus>>(
                    `${this.baseUri}${this.VACCINATION_STATUS_BASE_URI}`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch error: ${err}`);
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
    ): Promise<RequestResult<Report>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["dateOfVaccine"] = dateOfVaccine;

            return this.http
                .getWithCors<RequestResult<Report>>(
                    `${this.baseUri}${this.VACCINATION_STATUS_BASE_URI}/pdf`,
                    headers
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch report error: ${err}`);
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
                reject();
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
                    this.logger.error(`Fetch vaccine status: ${err}`);
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
        hdid: string,
        proofTemplate: VaccineProofTemplate
    ): Promise<RequestResult<CovidVaccineRecord>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            return this.http
                .getWithCors<RequestResult<CovidVaccineRecord>>(
                    `${this.baseUri}${this.AUTHENTICATED_VACCINATION_STATUS_BASE_URI}/pdf?hdid=${hdid}&proofTemplate=${proofTemplate}`
                )
                .then((requestResult) => {
                    resolve(requestResult);
                })
                .catch((err) => {
                    this.logger.error(`Fetch vaccine record error: ${err}`);
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
