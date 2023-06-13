import { ServiceCode } from "@/constants/serviceCodes";
import { CheckInRequest } from "@/models/checkInRequest";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import { Ticket } from "@/models/ticket";
import { IHttpDelegate, ILogger, ITicketService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

export class RestTicketService implements ITicketService {
    private readonly TICKET_BASE_URI: string = "Ticket";
    private logger;
    private http;
    private baseUri;
    private isEnabled;

    constructor(
        logger: ILogger,
        http: IHttpDelegate,
        config: ExternalConfiguration
    ) {
        this.logger = logger;
        this.http = http;
        this.baseUri = config.serviceEndpoints["Ticket"];
        this.isEnabled =
            config.webClient.featureToggleConfiguration.waitingQueue.enabled;
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
