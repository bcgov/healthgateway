import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions, RootState } from "@/store/types";
import TimelineView from "@/views/TimelineView.vue";

let store: Store<RootState>;

function createWrapper(options?: GatewayStoreOptions): Wrapper<TimelineView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    store = new Vuex.Store(options);

    return shallowMount(TimelineView, {
        localVue,
        store: store,
        stubs: {
            "hg-icon": true,
            "hg-button": true,
            "page-title": true,
        },
    });
}

describe("Timeline view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const linearTimelineTag = "lineartimeline-stub";

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Loading state", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.medication.modules.statement.getters.isMedicationStatementLoading =
            () => true;
        const wrapper = createWrapper(options);

        // Check values
        expect(wrapper.find("#loading-toast").isVisible()).toBe(true);
    });

    test("Active", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.medication.modules.statement.getters.isMedicationStatementLoading =
            () => false;
        const wrapper = createWrapper(options);
        expect(wrapper.find(linearTimelineTag).isVisible()).toBe(true);
        expect(wrapper.find("calendartimeline-stub").isVisible()).toBe(false);
    });

    test("Shows Calendar", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.timeline.getters.isLinearView = () => false;
        const wrapper = createWrapper(options);

        expect(wrapper.find(linearTimelineTag).isVisible()).toBe(false);
        expect(wrapper.find("calendartimeline-stub").isVisible()).toBe(true);
    });
});
