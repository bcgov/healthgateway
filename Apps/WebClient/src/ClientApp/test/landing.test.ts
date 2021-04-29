import { createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";

import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import LandingComponent from "@/views/landing.vue";

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

    const configGetters = {
        webClient: (): WebClientConfiguration => {
            return a;
        },
    };

    const customStore = new Vuex.Store({
        modules: {
            config: {
                namespaced: true,
                getters: configGetters,
            },
        },
    });

    const wrapper = shallowMount(LandingComponent, {
        localVue,
        store: customStore,
        stubs: {
            "hg-icon": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
