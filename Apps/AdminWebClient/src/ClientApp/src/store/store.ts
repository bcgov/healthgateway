import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { RootState } from "@/models/storeState";
import { config } from "./modules/config/config";
import { auth } from "./modules/auth/auth";
import { drawer } from "./modules/drawer/drawer";

Vue.use(Vuex);

const storeOptions: StoreOptions<RootState> = {
  state: {
    version: "1.0.0" // a simple property
  },
  modules: {
    config,
    auth,
    drawer
  }
};

export default new Vuex.Store<RootState>(storeOptions);
