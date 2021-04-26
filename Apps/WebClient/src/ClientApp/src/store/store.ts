import Vue from "vue";
import Vuex from "vuex";

import { auth } from "./modules/auth/auth";
import { comment } from "./modules/comment/comment";
import { config } from "./modules/config/config";
import { encounter } from "./modules/encounter/encounter";
import { errorBanner } from "./modules/error/errorBanner";
import { idle } from "./modules/idle/idle";
import { immunization } from "./modules/immunization/immunization";
import { laboratory } from "./modules/laboratory/laboratory";
import { medication } from "./modules/medication/medication";
import { navbar } from "./modules/navbar/navbar";
import { note } from "./modules/note/note";
import { timeline } from "./modules/timeline/timeline";
import { user } from "./modules/user/user";
import { GatewayStoreOptions, RootState } from "./types";

Vue.use(Vuex);

const storeOptions: GatewayStoreOptions = {
    state: {
        version: "1.0.0",
        isMobile: false,
    },
    actions: {
        setIsMobile(context, isMobile: boolean) {
            context.commit("setIsMobile", isMobile);
        },
    },
    getters: {
        isMobile: (state) => {
            return state.isMobile;
        },
    },
    mutations: {
        setIsMobile(state, isMobile: boolean) {
            state.isMobile = isMobile;
        },
    },
    modules: {
        auth,
        config,
        user,
        medication,
        laboratory,
        comment,
        immunization,
        encounter,
        note,
        navbar,
        idle,
        errorBanner,
        timeline,
    },
};

export default new Vuex.Store<RootState>(storeOptions);
