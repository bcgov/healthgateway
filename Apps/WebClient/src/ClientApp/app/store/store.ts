import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { auth } from "./modules/auth/auth";
import { config } from "./modules/config/config";
import { RootState } from "@/models/storeState";

Vue.use(Vuex);

const storeOptions: StoreOptions<RootState> = {
  state: {
    version: "1.0.0" // a simple property
  },
  modules: {
    auth,
    config
  }
};

export default new Vuex.Store<RootState>(storeOptions);
