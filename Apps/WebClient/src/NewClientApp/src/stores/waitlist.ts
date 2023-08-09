import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { TicketStatus } from "@/constants/ticketStatus";
import { container } from "@/ioc/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import { Ticket } from "@/models/ticket";
import { IHttpDelegate, ILogger, ITicketService } from "@/services/interfaces";

export const useWaitlistStore = defineStore(
    "waitlist",
    () => {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );
        const ticketService = container.get<ITicketService>(
            SERVICE_IDENTIFIER.TicketService
        );

        const status = ref(LoadStatus.NONE);
        const tooBusy = ref(false);
        const ticket = ref<Ticket>();
        const checkInTimeoutId = ref<number>();

        const isLoading = computed(() => status.value === LoadStatus.REQUESTED);

        const ticketIsProcessed = computed(
            () => ticket.value?.status === TicketStatus.Processed
        );

        function setTicketRequested() {
            ticket.value = undefined;
            tooBusy.value = false;
            status.value = LoadStatus.REQUESTED;
        }

        function setTooBusy() {
            tooBusy.value = true;
            status.value = LoadStatus.LOADED;
        }

        function setError() {
            status.value = LoadStatus.ERROR;
        }

        function setTicket(incomingTicket: Ticket) {
            ticket.value = incomingTicket;
            tooBusy.value = false;
            status.value = LoadStatus.LOADED;
        }

        function clearTicket() {
            clearTimeout(checkInTimeoutId.value);
            ticket.value = undefined;
            tooBusy.value = false;
            status.value = LoadStatus.NONE;
        }

        function setCheckInTimeoutId(incomingCheckInTimeoutId: number) {
            checkInTimeoutId.value = incomingCheckInTimeoutId;
        }

        function scheduleCheckIn(
            incomingTicket?: Ticket,
            getTicketOnFail: boolean = false
        ): void {
            clearTimeout(checkInTimeoutId.value);
            const localTicket = incomingTicket ?? ticket.value;
            if (localTicket === undefined) {
                logger.debug(`Waitlist - Ticket undefined`);
                return;
            }
            const now = new Date().getTime();
            const checkInAfter = localTicket.checkInAfter * 1000;
            const timeout = Math.max(0, checkInAfter - now);
            logger.debug(`Waitlist - Scheduling checkIn in ${timeout}ms`);
            const newTimeoutId = window.setTimeout(() => {
                checkIn().catch(() => {
                    logger.error(`Waitlist - Scheduled checkIn failed`);
                    if (getTicketOnFail) {
                        getTicket().catch(() =>
                            logger.error(
                                `Waitlist - Scheduled checkIn failed to get ticket`
                            )
                        );
                    }
                });
            }, timeout);
            if (newTimeoutId !== undefined) {
                setCheckInTimeoutId(newTimeoutId);
            } else {
                logger.error(
                    `Waitlist - Scheduled checkIn failed to set timeout`
                );
            }
        }

        function handleTicket(incomingTicket: Ticket): void {
            if (
                incomingTicket.status === TicketStatus.Processed &&
                incomingTicket.token !== undefined
            ) {
                httpDelegate.setTicketAuthorizationHeader(incomingTicket.token);
            }
            scheduleCheckIn(incomingTicket);
        }

        function getTicket(): Promise<Ticket> {
            const localTraceId = "error-ticket-undefined";
            logger.debug("Waitlist - Retrieving ticket");
            setTicketRequested();
            return ticketService
                .createTicket("healthgateway")
                .then((result) => {
                    if (result) {
                        setTicket(result);
                        handleTicket(result);
                        return result;
                    } else {
                        throw {
                            resultMessage: "Ticket is undefined",
                            traceId: localTraceId,
                        } as ResultError;
                    }
                })
                .catch((error: ResultError) => {
                    if (error.traceId === localTraceId) {
                        setError();
                    } else {
                        setTooBusy();
                    }
                    throw error;
                });
        }

        function checkIn(): Promise<void> {
            if (ticket.value === undefined) {
                logger.debug(`Waitlist - Ticket undefined`);
                throw new Error("Ticket undefined");
            }
            logger.debug(`Waitlist - checkIn - Ticket id: ${ticket.value.id}`);
            return ticketService
                .checkIn({
                    id: ticket.value.id,
                    room: ticket.value.room,
                    nonce: ticket.value.nonce,
                })
                .then((result) => {
                    setTicket(result);
                    handleTicket(result);
                })
                .catch((error: ResultError) => {
                    throw error;
                });
        }

        function releaseTicket(): void {
            logger.debug("Waitlist - Releasing ticket");

            if (ticket.value === undefined) {
                clearTicket();
            } else {
                ticketService
                    .removeTicket({
                        id: ticket.value.id,
                        room: ticket.value.room,
                        nonce: ticket.value.nonce,
                    })
                    .finally(() => {
                        clearTicket();
                    });
            }
        }

        // must expose state variable for persistence
        return {
            ticket,
            checkInTimeoutId,
            isLoading,
            tooBusy,
            ticketIsProcessed,
            getTicket,
            handleTicket,
            scheduleCheckIn,
            releaseTicket,
        };
    },
    {
        persist: {
            storage: localStorage,
        },
    }
);
