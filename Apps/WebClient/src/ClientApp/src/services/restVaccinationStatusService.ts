import { injectable } from "inversify";
import { load } from "recaptcha-v3";

import { Dictionary } from "@/models/baseTypes";
import { ExternalConfiguration } from "@/models/configData";
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
    private baseUri = "";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private captchaSiteKey = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Immunization"];
        this.http = http;
        this.isEnabled = config.webClient.modules["Immunization"];
        this.captchaSiteKey = config.webClient.captchaSiteKey || "";
    }

    public getVaccinationStatus(
        phn: string,
        dateOfBirth: StringISODate,
        token: string
    ): Promise<RequestResult<VaccinationStatus>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["token"] = token;

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

    public getReport(
        phn: string,
        dateOfBirth: StringISODate,
        token: string
    ): Promise<RequestResult<Report>> {
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                reject();
                return;
            }

            const headers: Dictionary<string> = {};
            headers["phn"] = phn;
            headers["dateOfBirth"] = dateOfBirth;
            headers["token"] = token;

            return this.http
                .post<RequestResult<Report>>(
                    `${this.baseUri}${this.VACCINATION_STATUS_BASE_URI}`,
                    null,
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

    getCaptchaToken(): Promise<string> {
        return new Promise((resolve, reject) => {
            load(this.captchaSiteKey || "")
                .then((recaptcha) => {
                    recaptcha.showBadge();
                    recaptcha
                        .execute("vaccinationStatus")
                        .then((token) => {
                            resolve(token);
                        })
                        .catch((err) => {
                            this.logger.error(
                                `Error executing captcha action: ${err}`
                            );
                            reject(err);
                        });
                })
                .catch((err) => {
                    this.logger.error(`Error loading captcha: ${err}`);
                    reject(err);
                });
        });
    }
}
