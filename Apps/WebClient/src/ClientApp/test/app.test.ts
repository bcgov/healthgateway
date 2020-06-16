import { createLocalVue, shallowMount } from "@vue/test-utils";
import AppComponent from "@/app.vue";
import VueRouter from "vue-router";

describe("Home view", () => {
  const localVue = createLocalVue();
  localVue.use(VueRouter);
  const wrapper = shallowMount(AppComponent, { localVue });

  test("is a Vue instance", () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });
});
