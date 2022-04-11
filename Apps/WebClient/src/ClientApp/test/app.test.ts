import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount } from "@vue/test-utils";
import VueRouter from "vue-router";
import Vuex from "vuex";

import AppComponent from "@/app.vue";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

describe("Home view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    const router = new VueRouter();
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);

    const store = new Vuex.Store(new StoreOptionsStub());

    const wrapper = shallowMount(AppComponent, {
        localVue,
        router,
        store: store,
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
