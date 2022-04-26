import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount } from "@vue/test-utils";
import VueRouter from "vue-router";
import Vuex from "vuex";

import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

describe("NavBar Header Component", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    const localVue = createLocalVue();
    localVue.use(VueRouter);
    localVue.use(Vuex);
    const router = new VueRouter();

    const options = new StoreOptionsStub();
    options.modules.navbar.getters.isSidebarOpen = () => true;
    options.modules.navbar.getters.isHeaderShown = () => true;

    options.modules.auth.getters.oidcIsAuthenticated = () => false;
    options.modules.auth.getters.isValidIdentityProvider = () => true;

    options.modules.user.getters.userIsRegistered = () => true;
    options.modules.user.getters.userIsActive = () => true;

    const customStore = new Vuex.Store(options);

    const wrapper = shallowMount(HeaderComponent, {
        localVue,
        store: customStore,
        router,
        stubs: {
            "hg-icon": true,
            "hg-button": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
