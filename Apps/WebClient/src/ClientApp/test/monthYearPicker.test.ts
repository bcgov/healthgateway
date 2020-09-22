import { shallowMount } from "@vue/test-utils";
import MonthYearPickerComponent from "@/components/monthYearPicker.vue";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { DateWrapper } from "@/models/dateWrapper";

describe("MonthYearPickerComponent", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    test("renders component correctly", () => {
        const wrapper = shallowMount(MonthYearPickerComponent, {
            propsData: {
                currentMonth: new DateWrapper(),
                availableMonths: [new DateWrapper()],
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
