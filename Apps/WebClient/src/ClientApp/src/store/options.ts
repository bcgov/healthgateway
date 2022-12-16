import { injectable } from "inversify";
import { ActionContext } from "vuex";

import { AppErrorType } from "@/constants/errorType";

import { auth } from "./modules/auth/auth";
import { clinicalDocument } from "./modules/clinicalDocument/clinicalDocument";
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
import { notification } from "./modules/notification/notification";
import { timeline } from "./modules/timeline/timeline";
import { user } from "./modules/user/user";
import { vaccinationStatus } from "./modules/vaccinationStatus/vaccinationStatus";
import { waitlist } from "./modules/waitlist/waitlist";
import { GatewayStoreOptions, RootState } from "./types";

@injectable()
export class StoreOptions implements GatewayStoreOptions {
    state = {
        appError: undefined,
        isMobile: false,
        version: "1.0.0",
    };
    actions = {
        setAppError(
            context: ActionContext<RootState, RootState>,
            appError: AppErrorType
        ): void {
            context.commit("setAppError", appError);
        },
        setIsMobile(
            context: ActionContext<RootState, RootState>,
            isMobile: boolean
        ): void {
            context.commit("setIsMobile", isMobile);
        },
    };
    getters = {
        appError: (state: RootState): AppErrorType | undefined =>
            state.appError,
        isMobile: (state: RootState): boolean => state.isMobile,
    };
    mutations = {
        setAppError(state: RootState, appError: AppErrorType): void {
            state.appError = appError;
        },
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
        clinicalDocument,
        note,
        notification,
        navbar,
        idle,
        errorBanner,
        timeline,
        vaccinationStatus,
        waitlist,
    };
}
