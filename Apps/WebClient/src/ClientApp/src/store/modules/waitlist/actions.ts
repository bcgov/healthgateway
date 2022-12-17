import { TicketStatus } from "@/constants/ticketStatus";
import { ResultError } from "@/models/errors";
import { Ticket } from "@/models/ticket";
import container from "@/plugins/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger, ITicketService } from "@/services/interfaces";

import { WaitlistActions } from "./types";

export const actions: WaitlistActions = {
    getTicket: function (context): Promise<Ticket> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const ticketService = container.get<ITicketService>(
            SERVICE_IDENTIFIER.TicketService
        );

        return new Promise((resolve, reject) => {
            logger.debug("Retrieving waitlist ticket");
            context.commit("setTicketRequested");
            ticketService
                .createTicket("healthgateway")
                .then((result) => {
                    if (result) {
                        context.commit("setTicket", result);
                        resolve(result);
                        context.dispatch("handleTicket", { ticket: result });
                    } else {
                        context.commit("setError");
                        reject();
                    }
                })
                .catch((error: ResultError) => {
                    context.commit("setTooBusy");
                    reject(error);
                });
        });
    },
    handleTicket: function (
        context,
        params: { ticket: Ticket }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );
        const ticket = params.ticket;
        const now = new Date().getTime();
        const timeout = ticket.checkInAfter * 1000 - now;
        return new Promise((resolve) => {
            logger.debug(`Handle ticket: ${JSON.stringify(ticket)}`);
            logger.debug(
                `Handle ticket: timeout (milliseconds): ${timeout} - check in after (milliseconds): ${ticket.checkInAfter} - now (milliseconds): ${now}`
            );
            if (
                ticket.status === TicketStatus.Processed &&
                ticket.token !== undefined
            ) {
                httpDelegate.setTicketAuthorizationHeader(ticket.token);
            }
            setTimeout(() => {
                logger.debug(
                    `Set Timeout called - current time (milliseconds): ${new Date().getTime()}`
                );
                context
                    .dispatch("checkIn")
                    .catch((error) =>
                        logger.error(`Error calling checkIn action: ${error}`)
                    );
            }, timeout);

            resolve();
        });
    },
    checkIn: function (context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const ticketService = container.get<ITicketService>(
            SERVICE_IDENTIFIER.TicketService
        );

        return new Promise((resolve, reject) => {
            logger.debug("Checking in with waitlist");

            const ticket = context.state.ticket;
            if (ticket === undefined) {
                reject();
            } else {
                ticketService
                    .updateTicket({
                        id: ticket.id,
                        room: ticket.room,
                        nonce: ticket.nonce,
                    })
                    .then((result) => {
                        context.commit("setTicket", result);
                        resolve();
                        context.dispatch("handleTicket", { ticket: result });
                    })
                    .catch((error: ResultError) => {
                        reject(error);
                    });
            }
        });
    },
    releaseTicket: function (context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const ticketService = container.get<ITicketService>(
            SERVICE_IDENTIFIER.TicketService
        );

        return new Promise((resolve) => {
            logger.debug("Releasing waitlist ticket");

            const ticket = context.state.ticket;
            if (ticket === undefined) {
                context.commit("clearTicket");
                resolve();
            } else {
                ticketService
                    .removeTicket({
                        id: ticket.id,
                        room: ticket.room,
                        nonce: ticket.nonce,
                    })
                    .finally(() => {
                        context.commit("clearTicket");
                        resolve();
                    });
            }
        });
    },
};
