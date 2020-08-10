import { createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";
import BootstrapVue from "bootstrap-vue";
import LandingComponent from "@/views/landing.vue";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { WebClientConfiguration } from "@/models/configData";

describe("Landing view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(BootstrapVue);

    const configGetters = {
        webClient: (): WebClientConfiguration => {
            return {
                modules: {},
                registrationStatus: undefined,
            };
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
            "font-awesome-icon": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
