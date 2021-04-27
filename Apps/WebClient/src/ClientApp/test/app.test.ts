import { createLocalVue, shallowMount } from "@vue/test-utils";
import VueRouter from "vue-router";
import Vuex from "vuex";

import AppComponent from "@/app.vue";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { storeOptionsStub } from "./stubs/store/store";

describe("Home view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);

    let store = new Vuex.Store(storeOptionsStub);

    const wrapper = shallowMount(AppComponent, {
        localVue,
        store: store,
    });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
