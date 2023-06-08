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
            logger.debug("Waitlist - Retrieving ticket");
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
    scheduleCheckIn(
        context,
        params: {
            getTicketOnFail: boolean;
            ticket?: Ticket;
        } = { getTicketOnFail: false }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        clearTimeout(context.state.checkInTimeoutId);
        const ticket = params.ticket ?? context.state.ticket;
        return new Promise((resolve) => {
            if (ticket === undefined) {
                logger.debug(`Waitlist - Ticket undefined`);
                resolve();
            } else {
                const now = new Date().getTime();
                const checkInAfter = ticket.checkInAfter * 1000;
                const timeout = Math.max(0, checkInAfter - now);
                logger.debug(`Waitlist - scheduling checkIn in ${timeout}ms.`);
                const newCheckInTimeoutId = setTimeout(() => {
                    context.dispatch("checkIn").catch(() => {
                        logger.error(
                            `Waitlist - Error calling checkIn action.`
                        );
                        if (params.getTicketOnFail) {
                            context.dispatch("getTicket").then(() => {
                                logger.debug(
                                    `Waitlist - Error calling getTicket action.`
                                );
                            });
                        }
                    });
                }, timeout);
                // Store new timeout id in state
                context.commit("setCheckInTimeoutId", newCheckInTimeoutId);
                resolve();
            }
        });
    },
    handleTicket: function (
        context,
        params: { ticket: Ticket }
    ): Promise<void> {
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );
        const ticket = params.ticket;
        return new Promise((resolve) => {
            if (
                ticket.status === TicketStatus.Processed &&
                ticket.token !== undefined
            ) {
                httpDelegate.setTicketAuthorizationHeader(ticket.token);
            }
            context.dispatch("scheduleCheckIn", { ticket: ticket });

            resolve();
        });
    },
    checkIn: function (context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const ticketService = container.get<ITicketService>(
            SERVICE_IDENTIFIER.TicketService
        );

        return new Promise((resolve, reject) => {
            logger.debug("Waitlist - Checking in started");

            const ticket = context.state.ticket;
            if (ticket === undefined) {
                reject();
            } else {
                logger.debug(`Waitlist - checkIn - Ticket id: ${ticket.id}`);
                ticketService
                    .checkIn({
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
            logger.debug("Waitlist - Releasing ticket");

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
