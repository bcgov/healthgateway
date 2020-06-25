import { createLocalVue, shallowMount } from "@vue/test-utils";
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
    const router = new VueRouter();

    const wrapper = shallowMount(HeaderComponent, {
        localVue,
        store,
        router,
        stubs: {
            "font-awesome-icon": true,
        },
    });

    test("is a Vue instance", () => {
        expect(wrapper.isVueInstance()).toBeTruthy();
    });
});
