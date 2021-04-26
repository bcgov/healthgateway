import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { Store } from "vuex";

import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions, RootState } from "@/store/types";
import HealthInsightsView from "@/views/healthInsights.vue";

import { storeStub } from "./stubs/store/store";
import { DateTime } from "luxon";

let store: Store<RootState>;

function createWrapper(
    options?: GatewayStoreOptions
): Wrapper<HealthInsightsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = storeStub;
    }

    store = new Vuex.Store(options);

    return shallowMount(HealthInsightsView, {
        localVue,
        store: store,
        stubs: {
            "font-awesome-icon": true,
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
        const options = storeStub;
        storeStub.modules.medication.modules.statement.getters.isMedicationStatementLoading = () =>
            true;
        const wrapper = createWrapper(options);
        // Check values
        expect(wrapper.find("loadingcomponent-stub").exists()).toBe(true);
        expect(wrapper.find("linechart-stub").exists()).toBe(false);
    });

    test("Active", () => {
        // Setup vuex store
        const options = storeStub;
        storeStub.modules.medication.modules.statement.getters.isMedicationStatementLoading = () =>
            false;
        storeStub.modules.medication.modules.statement.getters.medicationStatements = () => [
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

        expect(wrapper.find("loadingcomponent-stub").exists()).toBe(false);
        expect(wrapper.find("linechart-stub").isVisible()).toBe(true);
    });
});
