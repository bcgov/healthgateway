import { shallowMount } from "@vue/test-utils";
import MonthYearPickerComponent from "@/components/monthYearPicker.vue";
import moment from "moment";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

describe("MonthYearPickerComponent", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    test("renders component correctly", () => {
        const wrapper = shallowMount(MonthYearPickerComponent, {
            propsData: {
                currentMonth: new Date(),
                availableMonths: [new Date()],
            },
        });

        expect(wrapper.find("#currentDate").text()).toBe(
            moment(new Date()).format("MMMM yyyy")
        );

        expect(wrapper.find("#selectedYearBtn").text()).toBe(
            new Date().getFullYear().toString()
        );
    });
});
