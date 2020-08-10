import { createLocalVue, shallowMount } from "@vue/test-utils";
import AppComponent from "@/app.vue";
import VueRouter from "vue-router";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
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
