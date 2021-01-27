import { createLocalVue, shallowMount } from "@vue/test-utils";
import VueRouter from "vue-router";

import AppComponent from "@/app.vue";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

describe("Home view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const localVue = createLocalVue();
    localVue.use(VueRouter);
    const wrapper = shallowMount(AppComponent, { localVue });

    test("is a Vue instance", () => {
        expect(wrapper).toBeTruthy();
    });
});
