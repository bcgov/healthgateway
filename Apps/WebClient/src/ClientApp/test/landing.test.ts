import { createLocalVue, shallowMount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import LandingComponent from "@/views/landing.vue";

describe("Landing view", () => {
  const localVue = createLocalVue();
  localVue.use(BootstrapVue);
  const wrapper = shallowMount(LandingComponent, {
    localVue,
    stubs: {
      "font-awesome-icon": true,
    },
  });

  test("is a Vue instance", () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });
});
