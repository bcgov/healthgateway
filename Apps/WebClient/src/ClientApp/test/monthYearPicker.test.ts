import { shallowMount } from "@vue/test-utils";
import MonthYearPickerComponent from "@/components/monthYearPicker.vue";
import moment from "moment";

describe("MonthYearPickerComponent", () => {
    test("renders component correctly", () => {
        const wrapper = shallowMount(MonthYearPickerComponent, {
            propsData: {
                currentDate: new Date(),
                availableMonths: [new Date()],
            },
        });

        expect(wrapper.find("#currentDate").text()).toBe(
            moment(new Date()).format("MMMM yyyy")
        );

        expect(wrapper.find(".select .item").text()).toBe(
            new Date().getFullYear().toString()
        );
    });
});
