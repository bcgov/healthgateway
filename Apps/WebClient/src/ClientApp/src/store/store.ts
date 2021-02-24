import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";

import { RootState } from "@/models/storeState";

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
        encounter,
        note,
        navbar,
        idle,
        errorBanner,
        timeline,
    },
};

export default new Vuex.Store<RootState>(storeOptions);
