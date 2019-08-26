import { shallowMount, createLocalVue } from "@vue/test-utils";
import CardInfoComponent from "@/views/cardInfo.vue";

describe("Registration view", () => {
  const localVue = createLocalVue();
  const wrapper = shallowMount(CardInfoComponent, { localVue });

  test("is a Vue instance", () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  test("has header element with static text", () => {
    const expectedH1Text = "Registration with BC Services Card";
    expect(wrapper.find("h1").text()).toBe(expectedH1Text);
  });
});
