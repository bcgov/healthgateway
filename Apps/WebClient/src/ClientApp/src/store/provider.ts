import { inject, injectable } from "inversify";
import Vue from "vue";
import Vuex, { Store } from "vuex";

import { STORE_IDENTIFIER } from "@/plugins/inversify";
import { IStoreProvider } from "@/services/interfaces";

import type { GatewayStoreOptions, RootState } from "./types";

Vue.use(Vuex);

@injectable()
export default class StoreProvider implements IStoreProvider {
    private readonly store: Store<RootState>;

    constructor(
        @inject(STORE_IDENTIFIER.StoreOptions) options: GatewayStoreOptions
    ) {
        this.store = new Vuex.Store<RootState>(options);
    }

    public getStore(): Store<RootState> {
        if (this.store === undefined) {
            throw Error("Store not initialized");
        }

        return this.store;
    }
}
