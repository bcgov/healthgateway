import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions, RootState } from "@/store/types";
import TimelineView from "@/views/timeline.vue";

import { storeStub } from "./stubs/store/store";
import { ImmunizationState } from "@/store/modules/immunization/types";
import { LoadStatus } from "@/models/storeOperations";

var store: Store<RootState>;

function createWrapper(options?: GatewayStoreOptions): Wrapper<TimelineView> {
    var localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = storeStub;
    }

    store = new Vuex.Store(options);

    return shallowMount(TimelineView, {
        localVue,
        store: store,
        stubs: {
            "hg-icon": true,
        },
    });
}

describe("Timeline view", () => {
    var logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    test("is a Vue instance", () => {
        var wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Loading state", () => {
        // Setup vuex store
        var options = storeStub;
        storeStub.modules.medication.modules.statement.getters.isMedicationStatementLoading = () =>
            true;
        var wrapper = createWrapper(options);

        // Check values
        expect(wrapper.find("loadingcomponent-stub").exists()).toBe(true);
        expect(wrapper.find("lineartimeline-stub").isVisible()).toBe(false);
    });

    test("Active", () => {
        // Setup vuex store
        var options = storeStub;
        storeStub.modules.medication.modules.statement.getters.isMedicationStatementLoading = () =>
            false;
        var wrapper = createWrapper(options);

        expect(wrapper.find("loadingcomponent-stub").exists()).toBe(false);
        expect(wrapper.find("lineartimeline-stub").isVisible()).toBe(true);
        expect(wrapper.find("calendartimeline-stub").isVisible()).toBe(false);
    });

    test("Shows Calendar", () => {
        // Setup vuex store
        var options = storeStub;
        options.modules.timeline.getters.isLinearView = () => false;
        var wrapper = createWrapper(options);

        expect(wrapper.find("lineartimeline-stub").isVisible()).toBe(false);
        expect(wrapper.find("calendartimeline-stub").isVisible()).toBe(true);
    });

    test("Shows SMS incomplete profile banner", () => {
        var user = new User();
        user.hasSMS = true;
        user.verifiedSMS = false;

        // Setup vuex store
        var options = storeStub;
        options.modules.user.getters.user = () => user;
        var wrapper = createWrapper(options);

        expect(wrapper.find("#incomplete-profile-banner").exists()).toBe(true);
    });

    test("Shows Email incomplete profile banner", () => {
        var user = new User();
        user.hasEmail = true;
        user.verifiedEmail = false;

        // Setup vuex store
        var options = storeStub;
        options.modules.user.getters.user = () => user;
        var wrapper = createWrapper(options);

        expect(wrapper.find("#incomplete-profile-banner").exists()).toBe(true);
    });

    test("Hides incomplete profile banner when verified", () => {
        var user = new User();
        user.hasEmail = true;
        user.verifiedEmail = true;
        user.hasSMS = true;
        user.verifiedSMS = true;

        // Setup vuex store
        var options = storeStub;
        options.modules.user.getters.user = () => user;
        var wrapper = createWrapper(options);

        expect(wrapper.find("#incomplete-profile-banner").exists()).toBe(false);
    });

    test("Shows Loading immunization", () => {
        // Setup vuex store
        var options = storeStub;
        var module = options.modules.immunization;
        module.mutations.setStatus = (state, loadStatus: LoadStatus) => {
            state.status = loadStatus;
        };

        module.state.status = LoadStatus.NONE;
        module.getters.isDeferredLoad = (state: ImmunizationState) =>
            state.status === LoadStatus.DEFERRED;

        var wrapper = createWrapper(options);
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
