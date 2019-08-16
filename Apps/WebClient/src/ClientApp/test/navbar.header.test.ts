import { shallowMount, createLocalVue } from "@vue/test-utils";
import HeaderComponent from "@/components/navmenu/navHeader.vue";
import VueRouter from "vue-router";
import boostrapVue from "bootstrap-vue";
import Vuex from "vuex";
import store from "@/store/store";

describe("NavBar Header Component", () => {
  const localVue = createLocalVue();
  localVue.use(VueRouter);
  localVue.use(boostrapVue);
  localVue.use(Vuex);

  const wrapper = shallowMount(HeaderComponent, { localVue, store });

  test("is a Vue instance", () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  const languageWrapper = wrapper.find("#languageSelector");

  test("has default language set", () => {
    expect(languageWrapper.attributes("text")).toBe("English"); // Checks the dropdown element text
    expect(wrapper.vm.$data.currentLanguage.code).toBe("en"); // Checks scope variable
  });

  test("updates the language localization", () => {
    const frenchLink = languageWrapper.find("#fr");
    frenchLink.trigger("click");

    expect(languageWrapper.attributes("text")).toBe("French"); // Checks the dropdown element text
    expect(wrapper.vm.$data.currentLanguage.code).toBe("fr"); // Checks scope variable
  });
});
