import { createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";

import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import LandingComponent from "@/views/landing.vue";
import { storeOptionsStub } from "./stubs/store/store";

describe("Landing view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(Vuex);

    const a: WebClientConfiguration = {
        logLevel: "",
        timeouts: { idle: 0, logoutRedirect: "", resendSMS: 1 },
        registrationStatus: "open" as RegistrationStatus,
        externalURLs: {},
        modules: {},
        hoursForDeletion: 1,
        minPatientAge: 16,
        maxDependentAge: 12,
    };

    storeOptionsStub.modules.config.getters.webClient = (): WebClientConfiguration => {
        return a;
    };

    let store = new Vuex.Store(storeOptionsStub);

    const wrapper = shallowMount(LandingComponent, {
        localVue,
        store: store,
        stubs: {
            "font-awesome-icon": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
