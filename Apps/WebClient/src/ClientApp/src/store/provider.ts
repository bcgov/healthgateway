import { injectable } from "inversify";
import Vue from "vue";
import Vuex, { Store } from "vuex";

import { IStoreProvider } from "@/services/interfaces";

import { storeOptions } from "./options";
import { RootState } from "./types";

Vue.use(Vuex);

@injectable()
export default class StoreProvider implements IStoreProvider {
    private store!: Store<RootState>;

    public getStore(): Store<RootState> {
        console.log(this.store);
        if (this.store === undefined) {
            console.log("Initializing store...");
            this.store = new Vuex.Store<RootState>(storeOptions);
        }

        return this.store;
    }
}
