import { LoadStatus } from "@/models/storeOperations";
import { Ticket } from "@/models/ticket";

import { WaitlistMutations, WaitlistState } from "./types";

export const mutations: WaitlistMutations = {
    setTicketRequested(state: WaitlistState) {
        state.ticket = undefined;
        state.tooBusy = false;
        state.status = LoadStatus.REQUESTED;
    },
    setTooBusy: function (state: WaitlistState): void {
        state.tooBusy = true;
        state.status = LoadStatus.LOADED;
    },
    setError: function (state: WaitlistState): void {
        state.status = LoadStatus.ERROR;
    },
    setTicket: function (state: WaitlistState, ticket: Ticket): void {
        state.ticket = ticket;
        state.tooBusy = false;
        state.status = LoadStatus.LOADED;
    },
    clearTicket: function (state: WaitlistState): void {
        state.ticket = undefined;
        state.tooBusy = false;
        state.status = LoadStatus.NONE;
    },
    setCheckInTimeoutId: function (
        state: WaitlistState,
        checkInTimeoutId: number
    ): void {
        state.checkInTimeoutId = checkInTimeoutId;
    },
};
