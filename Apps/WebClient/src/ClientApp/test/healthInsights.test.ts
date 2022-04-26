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
import HealthInsightsView from "@/views/HealthInsightsView.vue";

let store: Store<RootState>;

function createWrapper(
    options?: GatewayStoreOptions
): Wrapper<HealthInsightsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    store = new Vuex.Store(options);

    const LoadingComponentStub = {
        name: "LoadingComponent",
        template: "<div v-show='isLoading' id='loadingStub'/>",
        props: ["isLoading"],
    };

    return shallowMount(HealthInsightsView, {
        localVue,
        store: store,
        stubs: {
            LoadingComponent: LoadingComponentStub,
            "hg-icon": true,
            "hg-button": true,
            "page-title": true,
        },
    });
}

describe("HealthInsights view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

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
        expect(wrapper.find("linechart-stub").exists()).toBe(false);
    });

    test("Active", () => {
        // Setup vuex store
        const options = new StoreOptionsStub();
        options.modules.medication.modules.statement.getters.isMedicationStatementLoading =
            () => false;
        options.modules.medication.modules.statement.getters.medicationStatements =
            () => [
                {
                    prescriptionIdentifier: "",
                    dispensedDate: "",
                    dispensingPharmacy: {},
                    medicationSummary: {
                        din: "",
                        brandName: "",
                        genericName: "",
                        isPin: false,
                    },
                },
            ];
        const wrapper = createWrapper(options);

        expect(wrapper.find("#loadingStub").isVisible()).toBe(false);
        expect(wrapper.find("linechart-stub").isVisible()).toBe(true);
    });
});
