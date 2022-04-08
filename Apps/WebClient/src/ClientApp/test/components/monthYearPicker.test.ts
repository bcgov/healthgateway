import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";

import MonthYearPickerComponent from "@/components/MonthYearPickerComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

describe("MonthYearPickerComponent", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    const localVue = createLocalVue();
    localVue.use(Vuex);

    const options = new StoreOptionsStub();
    const store = new Vuex.Store(options);

    logger.initialize("info");
    test("renders component correctly", () => {
        const wrapper = shallowMount(MonthYearPickerComponent, {
            localVue,
            propsData: {
                currentMonth: new DateWrapper(),
                availableMonths: [new DateWrapper()],
            },
            store: store,
            stubs: {
                "hg-icon": true,
                "hg-button": true,
            },
        });

        expect(wrapper.find("#currentDate").text()).toBe(
            new DateWrapper().format("MMMM yyyy")
        );

        expect(wrapper.find("#selectedYearBtn").text()).toBe(
            new DateWrapper().format("yyyy")
        );
    });
});
