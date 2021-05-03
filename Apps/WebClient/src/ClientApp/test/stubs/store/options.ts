import { voidMethod } from "@test/stubs/util";
import { injectable } from "inversify";

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

@injectable()
export class StoreOptionsStub implements GatewayStoreOptions {
    actions = {
        setIsMobile: voidMethod,
    };
    getters = {
        isMobile: (): boolean => false,
    };
    mutations = {
        setIsMobile: voidMethod,
    };
    modules = {
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
    };
}
