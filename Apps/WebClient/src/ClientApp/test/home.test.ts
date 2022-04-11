import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions, RootState } from "@/store/types";
import HomeView from "@/views/HomeView.vue";

let store: Store<RootState>;

function createWrapper(options?: GatewayStoreOptions): Wrapper<HomeView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    const ChildComponentStub = {
        name: "LoadingComponent",
        template: "<div v-show='isLoading' id='loadingStub'/>",
        props: ["isLoading"],
    };

    return shallowMount(HomeView, {
        localVue,
        store: store,
        stubs: {
            LoadingComponent: ChildComponentStub,
            "hg-icon": true,
            "hg-button": true,
            "hg-card-button": true,
            "page-title": true,
        },
    });
}

describe("Home view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const bannerId = "#incomplete-profile-banner";

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Shows SMS incomplete profile banner", () => {
        const user = new User();
        user.hasSMS = true;
        user.verifiedSMS = false;

        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.user.getters.user = () => user;
        const wrapper = createWrapper(options);

        expect(wrapper.find(bannerId).exists()).toBe(true);
    });

    test("Shows Email incomplete profile banner", () => {
        const user = new User();
        user.hasEmail = true;
        user.verifiedEmail = false;

        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.user.getters.user = () => user;
        const wrapper = createWrapper(options);

        expect(wrapper.find(bannerId).exists()).toBe(true);
    });

    test("Hides incomplete profile banner when verified", () => {
        const user = new User();
        user.hasEmail = true;
        user.verifiedEmail = true;
        user.hasSMS = true;
        user.verifiedSMS = true;

        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.user.getters.user = () => user;
        const wrapper = createWrapper(options);

        expect(wrapper.find(bannerId).exists()).toBe(false);
    });
});
