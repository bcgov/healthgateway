import { createLocalVue, shallowMount } from "@vue/test-utils";
import HeaderComponent from "@/components/navmenu/navHeader.vue";
import VueRouter from "vue-router";
import boostrapVue from "bootstrap-vue";
import Vuex from "vuex";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

describe("NavBar Header Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    const localVue = createLocalVue();
    localVue.use(VueRouter);
    localVue.use(boostrapVue);
    localVue.use(Vuex);
    const router = new VueRouter();

    const customStore = new Vuex.Store({
        modules: {
            sidebar: {
                namespaced: true,
                getters: {
                    isOpen: () => {
                        return true;
                    },
                },
            },
            auth: {
                namespaced: true,
                getters: {
                    oidcIsAuthenticated: () => {
                        return true;
                    },
                },
            },
            user: {
                namespaced: true,
                getters: {
                    userIsRegistered: () => {
                        return true;
                    },
                    userIsActive: () => {
                        return true;
                    },
                },
            },
        },
    });

    const wrapper = shallowMount(HeaderComponent, {
        localVue,
        store: customStore,
        router,
        stubs: {
            "font-awesome-icon": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
