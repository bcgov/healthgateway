import { defineStore } from "pinia";
import { LoadStatus } from "@/models/storeOperations";
import { computed, ref } from "vue";
import { Ticket } from "@/models/ticket";
import { TicketStatus } from "@/constants/ticketStatus";
import { container } from "@/ioc/container";
import { IHttpDelegate, ILogger, ITicketService } from "@/services/interfaces";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";

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
        const tooBusyField = ref(false);
        const ticketField = ref<Ticket>();
        const checkInTimeoutId = ref<number>();

        const isLoading = computed(() => status.value === LoadStatus.REQUESTED);

        const tooBusy = computed(() => tooBusyField.value);

        const ticket = computed(() => ticketField.value);

        const ticketIsProcessed = computed(
            () => ticketField.value?.status === TicketStatus.Processed
        );

        function setTicketRequested() {
            ticketField.value = undefined;
            tooBusyField.value = false;
            status.value = LoadStatus.REQUESTED;
        }

        function setTooBusy() {
            tooBusyField.value = true;
            status.value = LoadStatus.LOADED;
        }

        function setError() {
            status.value = LoadStatus.ERROR;
        }

        function setTicket(ticket: Ticket) {
            ticketField.value = ticket;
            tooBusyField.value = false;
            status.value = LoadStatus.LOADED;
        }

        function clearTicket() {
            clearTimeout(checkInTimeoutId.value);
            ticketField.value = undefined;
            tooBusyField.value = false;
            status.value = LoadStatus.NONE;
        }

        function setCheckInTimeoutId(incomingCheckInTimeoutId: number) {
            checkInTimeoutId.value = incomingCheckInTimeoutId;
        }

        function scheduleCheckIn(
            ticket?: Ticket,
            getTicketOnFail: boolean = false
        ): void {
            clearTimeout(checkInTimeoutId.value);
            const localTicket = ticket ?? ticketField.value;
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
            if (ticketField.value === undefined) {
                logger.debug(`Waitlist - Ticket undefined`);
                throw new Error("Ticket undefined");
            }
            logger.debug(
                `Waitlist - checkIn - Ticket id: ${ticketField.value.id}`
            );
            return ticketService
                .checkIn({
                    id: ticketField.value.id,
                    room: ticketField.value.room,
                    nonce: ticketField.value.nonce,
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

            if (ticketField.value === undefined) {
                clearTicket();
            } else {
                ticketService
                    .removeTicket({
                        id: ticketField.value.id,
                        room: ticketField.value.room,
                        nonce: ticketField.value.nonce,
                    })
                    .finally(() => {
                        clearTicket();
                    });
            }
        }

        // must expose state variable for persistence
        return {
            ticketField,
            checkInTimeoutId,
            isLoading,
            tooBusy,
            ticket,
            ticketIsProcessed,
            getTicket,
            handleTicket,
            releaseTicket,
        };
    },
    {
        persist: {
            storage: localStorage,
        },
    }
);
