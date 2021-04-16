import { createLocalVue, mount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex from "vuex";

import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import TimelineComponent from "@/views/timeline.vue";

const userWithResults = new User();
userWithResults.hdid = "hdid_with_results";

function createWrapper(): Wrapper<TimelineComponent> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    /*const customStore = new Vuex.Store({
        modules: {
            user: {
                namespaced: true,
                getters: userGetters,
                actions: userModule.actions,
            },
            config: {
                namespaced: true,
                getters: configGetters,
            },
            laboratory: {
                namespaced: true,
                getters: laboratoryGetters,
                actions: laboratoryActions,
            },
            medication: {
                namespaced: true,
                getters: medicationGetters,
                actions: medicationActions,
            },
            immunization: {
                namespaced: true,
                getters: immunizationGetters,
                actions: immunizationActions,
            },
            comment: {
                namespaced: true,
                getters: commentGetters,
                actions: commentActions,
            },
            sidebar: {
                namespaced: true,
                getters: sidebarGetters,
                actions: sidebarActions,
            },
            errorBanner: {
                namespaced: true,
                getters: errorBannerGetters,
                actions: errorBannerActions,
            },
        },
    });*/

    return mount(TimelineComponent, {
        localVue,
        store: new Vuex.Store({}),
        stubs: {
            "font-awesome-icon": true,
        },
    });
}

describe("Timeline view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("has header element with static text", () => {
        const expectedH1Text = "Health Care Timeline";
        const wrapper = createWrapper();
        expect(wrapper.find("h1").text()).toBe(expectedH1Text);
    });

    test("Has entries", () => {
        /*userGetters = {
            user: (): User => {
                return userWithResults;
            },
        };*/

        const wrapper = createWrapper();
        // Verify the number of records
        const unwatch = wrapper.vm.$watch(
            () => {
                return wrapper.vm.$data.isLoading;
            },
            () => {
                expect(wrapper.findAll(".cardWrapper").length).toEqual(3);
                expect(wrapper.findAll(".cardWrapper").length).toEqual(3);
                expect(wrapper.findAll(".dateHeading").length).toEqual(2);
                unwatch();
            }
        );
    });
});
