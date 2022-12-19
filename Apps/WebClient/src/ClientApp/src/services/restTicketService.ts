import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { CheckInRequest } from "@/models/checkInRequest";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { Ticket } from "@/models/ticket";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger, ITicketService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestTicketService implements ITicketService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly TICKET_BASE_URI: string = "Ticket";
    private http!: IHttpDelegate;
    private isEnabled = false;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.isEnabled = config.webClient.modules["Ticket"];
        this.baseUri = config.serviceEndpoints["Ticket"];
    }

    public createTicket(room: string): Promise<Ticket | undefined> {
        this.logger.debug(`createTicket: ${room}`);
        return new Promise((resolve, reject) => {
            if (!this.isEnabled) {
                resolve(undefined);
                return;
            }

            this.http
                .post<Ticket>(
                    `${this.baseUri}${this.TICKET_BASE_URI}?room=${room}`,
                    null
                )
                .then((ticket) => {
                    this.logger.debug(
                        `Ticket created: ${JSON.stringify(ticket)}`
                    );
                    resolve(ticket);
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestTicketService.createTicket() - ${err.statusCode}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Ticket
                        )
                    );
                });
        });
    }
    public checkIn(checkInRequest: CheckInRequest): Promise<Ticket> {
        this.logger.debug(`checkIn: ${JSON.stringify(checkInRequest)}`);
        return new Promise((resolve, reject) => {
            this.http
                .put<Ticket>(
                    `${this.baseUri}${this.TICKET_BASE_URI}/check-in`,
                    checkInRequest
                )
                .then((ticket) => {
                    this.logger.debug(
                        `Ticket updated: ${JSON.stringify(ticket)}`
                    );
                    resolve(ticket);
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestTicketService.checkInRequest() - ${err.statusCode}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Ticket
                        )
                    );
                });
        });
    }
    public removeTicket(checkInRequest: CheckInRequest): Promise<void> {
        this.logger.debug(`removeTicket: ${JSON.stringify(checkInRequest)}`);
        return new Promise((resolve, reject) => {
            this.http
                .delete<void>(
                    `${this.baseUri}${this.TICKET_BASE_URI}`,
                    checkInRequest
                )
                .then(() => {
                    this.logger.debug(`Ticket removed.`);
                    resolve();
                })
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestTicketService.removeTicket() - ${err.statusCode}`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Ticket
                        )
                    );
                });
        });
    }
}
