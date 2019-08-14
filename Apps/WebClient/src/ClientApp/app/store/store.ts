import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { auth } from "./modules/auth/auth";
import { imms } from "./modules/imms/imms";
import { RootState } from "@/models/rootState";

Vue.use(Vuex);

const storeOptions: StoreOptions<RootState> = {
  state: {
    version: "1.0.0" // a simple property
  },
  modules: {
    auth,
    imms
  }
};

export default new Vuex.Store<RootState>(storeOptions);
