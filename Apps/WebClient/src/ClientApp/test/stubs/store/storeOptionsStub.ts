import { voidMethod } from "@test/stubs/util";
import { injectable } from "inversify";

import { GatewayStoreOptions } from "@/store/types";

import authStub from "./authStub";
import commentStub from "./commentStub";
import configStub from "./configStub";
import encounterStub from "./encounterStub";
import errorBannerStub from "./errorBannerStub";
import idleStub from "./idleStub";
import immunizationStub from "./immunizationStub";
import laboratoryStub from "./laboratoryStub";
import medicationStub from "./medicationStub";
import navbarStub from "./navbarStub";
import noteStub from "./noteStub";
import timelineStub from "./timelineStub";
import userStub from "./userStub";
import vaccinationStatusStub from "./vaccinationStatusStub";

@injectable()
export default class StoreOptionsStub implements GatewayStoreOptions {
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
        vaccinationStatus: vaccinationStatusStub,
    };
}
