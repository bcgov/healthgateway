import { TicketStatus } from "@/constants/ticketStatus";
import { LoadStatus } from "@/models/storeOperations";
import { Ticket } from "@/models/ticket";

import { WaitlistGetters, WaitlistState } from "./types";

export const getters: WaitlistGetters = {
    isLoading: function (state: WaitlistState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
    tooBusy: function (state: WaitlistState): boolean {
        return state.tooBusy;
    },
    ticket: function (state: WaitlistState): Ticket | undefined {
        return state.ticket;
    },
    ticketIsProcessed: function (state: WaitlistState): boolean {
        return state.ticket?.status === TicketStatus.Processed;
    },
    ticketIsCreated: function (state: WaitlistState): boolean {
        return state.ticket !== undefined;
    },
};
