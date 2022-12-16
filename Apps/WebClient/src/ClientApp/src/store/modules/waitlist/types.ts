import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { LoadStatus } from "@/models/storeOperations";
import { Ticket } from "@/models/ticket";
import { RootState } from "@/store/types";

export interface WaitlistState {
    ticket?: Ticket;
    tooBusy: boolean;
    status: LoadStatus;
}

export interface WaitlistGetters extends GetterTree<WaitlistState, RootState> {
    isLoading(state: WaitlistState): boolean;
    tooBusy(state: WaitlistState): boolean;
    ticket(state: WaitlistState): Ticket | undefined;
    ticketIsProcessed(state: WaitlistState): boolean;
}

type StoreContext = ActionContext<WaitlistState, RootState>;
export interface WaitlistActions extends ActionTree<WaitlistState, RootState> {
    getTicket(context: StoreContext): Promise<Ticket>;
    checkIn(context: StoreContext): Promise<void>;
    releaseTicket(context: StoreContext): Promise<void>;
}

export interface WaitlistMutations extends MutationTree<WaitlistState> {
    setTicketRequested(state: WaitlistState): void;
    setTooBusy(state: WaitlistState): void;
    setError(state: WaitlistState): void;
    setTicket(state: WaitlistState, ticket: Ticket): void;
    clearTicket(state: WaitlistState): void;
}

export interface WaitlistModule extends Module<WaitlistState, RootState> {
    state: WaitlistState;
    getters: WaitlistGetters;
    actions: WaitlistActions;
    mutations: WaitlistMutations;
}
