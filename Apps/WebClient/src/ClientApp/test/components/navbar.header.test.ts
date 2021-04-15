import { createLocalVue, shallowMount } from "@vue/test-utils";
import VueRouter from "vue-router";
import Vuex from "vuex";

import HeaderComponent from "@/components/navmenu/navHeader.vue";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

describe("NavBar Header Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    const localVue = createLocalVue();
    localVue.use(VueRouter);
    localVue.use(Vuex);
    const router = new VueRouter();

    const customStore = new Vuex.Store({
        modules: {
            navbar: {
                namespaced: true,
                getters: {
                    isSidebarOpen: () => {
                        return true;
                    },
                    isHeaderShown: () => {
                        return true;
                    },
                },
            },
            auth: {
                namespaced: true,
                getters: {
                    oidcIsAuthenticated: () => {
                        return false;
                    },
                    isValidIdentityProvider: () => {
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
