import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import { LoadStatus } from "@/models/storeOperations";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { ImmunizationState } from "@/store/modules/immunization/types";
import { GatewayStoreOptions, RootState } from "@/store/types";
import TimelineView from "@/views/timeline.vue";

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

    const ChildComponentStub = {
        name: "LoadingComponent",
        template: "<div v-show='isLoading' id='loadingStub'/>",
        props: ["isLoading"],
    };

    return shallowMount(TimelineView, {
        localVue,
        store: store,
        stubs: {
            LoadingComponent: ChildComponentStub,
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
        expect(wrapper.find("#loadingStub").isVisible()).toBe(true);
        expect(wrapper.find(linearTimelineTag).isVisible()).toBe(false);
    });

    test("Active", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.medication.modules.statement.getters.isMedicationStatementLoading =
            () => false;
        const wrapper = createWrapper(options);
        expect(wrapper.find("#loadingStub").isVisible()).toBe(false);
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

    test("Shows Loading immunization", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        const module = options.modules.immunization;
        module.mutations.setStatus = (
            state: ImmunizationState,
            loadStatus: LoadStatus
        ) => {
            state.status = loadStatus;
        };

        module.state.status = LoadStatus.NONE;
        module.getters.isDeferredLoad = (state: ImmunizationState) =>
            state.status === LoadStatus.DEFERRED;

        const wrapper = createWrapper(options);
        expect(wrapper.find("[data-testid=immunizationLoading]").exists()).toBe(
            false
        );

        store.commit("immunization/setStatus", LoadStatus.DEFERRED);

        wrapper.vm.$nextTick().then(() => {
            expect(
                wrapper.find("[data-testid=immunizationLoading]").exists()
            ).toBe(true);
        });
    });
});
