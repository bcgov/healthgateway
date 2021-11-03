import { injectable } from "inversify";
import { ActionContext } from "vuex";

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
import { vaccinationStatus } from "./modules/vaccinationStatus/vaccinationStatus";
import { GatewayStoreOptions, RootState } from "./types";

@injectable()
export class StoreOptions implements GatewayStoreOptions {
    state = {
        version: "1.0.0",
        isMobile: false,
    };
    actions = {
        setIsMobile(
            context: ActionContext<RootState, RootState>,
            isMobile: boolean
        ): void {
            context.commit("setIsMobile", isMobile);
        },
    };
    getters = {
        isMobile: (state: RootState): boolean => {
            return state.isMobile;
        },
    };
    mutations = {
        setIsMobile(state: RootState, isMobile: boolean): void {
            state.isMobile = isMobile;
        },
    };
    modules = {
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
        vaccinationStatus,
    };
}
