import "@/plugins/inversify.config";
import { createLocalVue, shallowMount } from "@vue/test-utils";

import Vuex from "vuex";

import MonthYearPickerComponent from "@/components/monthYearPicker.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { StoreOptionsStub } from "@test/stubs/store/store";

describe("MonthYearPickerComponent", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    const localVue = createLocalVue();
    localVue.use(Vuex);

    const options = new StoreOptionsStub();
    let store = new Vuex.Store(options);

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
                "font-awesome-icon": true,
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
