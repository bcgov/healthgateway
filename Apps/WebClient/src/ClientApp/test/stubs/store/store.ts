import { GatewayStoreOptions } from "@/store/types";

import authStub from "./auth";
import commentStub from "./comment";
import configStub from "./config";
import encounterStub from "./encounter";
import errorBannerStub from "./error";
import idleStub from "./idle";
import immunizationStub from "./immunization";
import laboratoryStub from "./laboratory";
import medicationStub from "./medication";
import navbarStub from "./navbar";
import noteStub from "./note";
import timelineStub from "./timeline";
import userStub from "./user";

export var storeStub: GatewayStoreOptions = {
    actions: {
        setIsMobile(): void {},
    },
    getters: {
        isMobile: (): void => {},
    },
    mutations: {
        setIsMobile(): void {},
    },
    modules: {
        auth: authStub,
        config: configStub,
        user: userStub,
        medication: medicationStub,
        laboratory: laboratoryStub,
        comment: commentStub,
        immunization: immunizationStub,
        encounter: encounterStub,
        note: noteStub,
        navbar: navbarStub,
        idle: idleStub,
        errorBanner: errorBannerStub,
        timeline: timelineStub,
    },
};
