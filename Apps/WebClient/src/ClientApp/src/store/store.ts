import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { RootState } from "@/models/storeState";
import { auth } from "./modules/auth/auth";
import { config } from "./modules/config/config";
import { user } from "./modules/user/user";
import { medication } from "./modules/medication/medication";
import { laboratory } from "./modules/laboratory/laboratory";
import { immunization } from "./modules/immunization/immunization";
import { sidebar } from "./modules/sidebar/sidebar";
import { errorBanner } from "./modules/error/errorBanner";
import { comment } from "./modules/comment/comment";

Vue.use(Vuex);

const storeOptions: StoreOptions<RootState> = {
    state: {
        version: "1.0.0",
    },
    modules: {
        auth,
        config,
        user,
        medication,
        laboratory,
        comment,
        immunization,
        sidebar,
        errorBanner,
    },
};

export default new Vuex.Store<RootState>(storeOptions);
