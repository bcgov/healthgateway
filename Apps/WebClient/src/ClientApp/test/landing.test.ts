import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";

import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import LandingComponent from "@/views/LandingView.vue";

describe("Landing view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(Vuex);

    const options = new StoreOptionsStub();
    options.modules.config.getters.webClient = (): WebClientConfiguration => {
        return {
            logLevel: "",
            timeouts: { idle: 0, logoutRedirect: "", resendSMS: 1 },
            registrationStatus: "open" as RegistrationStatus,
            externalURLs: {},
            modules: {},
            hoursForDeletion: 1,
            minPatientAge: 16,
            maxDependentAge: 12,
        };
    };

    const store = new Vuex.Store(options);

    const wrapper = shallowMount(LandingComponent, {
        localVue,
        store: store,
        stubs: {
            "hg-icon": true,
            "hg-button": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
