import { shallowMount, createLocalVue } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import ProfileComponent from "@/views/profile.vue";
import VueRouter from "vue-router";
import Vuex from "vuex";
import store from "@/store/store";

describe("Home view", () => {
  const localVue = createLocalVue();
  localVue.use(BootstrapVue);
  localVue.use(VueRouter);
  localVue.use(Vuex);
  const wrapper = shallowMount(ProfileComponent, { localVue, store });

  test("is a Vue instance", () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  test("has header element with static text", () => {
    const expectedH1Text = "Welcome to Health Gateway!";
    expect(wrapper.find("h1").text()).toBe(expectedH1Text);
  });
});
