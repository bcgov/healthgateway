import { ResultError } from "@/models/errors";
import { Ticket } from "@/models/ticket";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, ITicketService } from "@/services/interfaces";

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
