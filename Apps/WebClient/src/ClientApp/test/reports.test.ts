import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";

import { createLocalVue, shallowMount, Stubs, Wrapper } from "@vue/test-utils";
import {
    BButton,
    BForm,
    BFormInput,
    BFormSelect,
    BFormTag,
} from "bootstrap-vue";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex from "vuex";

import { RegistrationStatus } from "@/constants/registrationStatus";
import { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ClientModule } from "@/router";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions } from "@/store/types";
import ReportsView from "@/views/reports.vue";

interface ReportComponent extends Vue {
    generatePdf(): Promise<void>;
}

const webclientConfig: WebClientConfiguration = {
    logLevel: "error",
    timeouts: { idle: 50, logoutRedirect: "", resendSMS: 3 },
    registrationStatus: RegistrationStatus.Open,
    externalURLs: {},
    modules: {},
    hoursForDeletion: 1,
    minPatientAge: 21,
    maxDependentAge: 21,
};

const baseMedication: MedicationStatementHistory = {
    prescriptionIdentifier: "",
    dispensedDate: "",
    medicationSummary: {
        din: "",
        brandName: "The Brand Name",
        genericName: "Generic Name",
        isPin: false,
    },
    dispensingPharmacy: {},
};

function createWrapper(
    options?: GatewayStoreOptions,
    customStubs?: Stubs
): Wrapper<ReportsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    options = options as GatewayStoreOptions;

    const store = new Vuex.Store(options);

    const defaultStubs = {
        "hg-icon": true,
        "hg-button": true,
        "page-title": true,
    };

    return shallowMount(ReportsView, {
        localVue,
        store: store,
        stubs: { ...defaultStubs, ...customStubs },
    });
}

describe("Report view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const reportIdTag = "#reportType";
    const testIdExclusion = "[data-testid=medicationExclusionFilter]";
    const testIdSelectedDates = "[data-testid=selectedDatesFilter]";
    const testIdClearFilter = "[data-testid=clearFilter]";

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("Available report types - single", () => {
        // Setup vuex store
        webclientConfig.modules = {
            [ClientModule.Medication]: true,
        };
        const options = new StoreOptionsStub();
        options.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const wrapperSingleReport = createWrapper(options);

        // Check values
        expect(
            wrapperSingleReport.find(reportIdTag).props()["options"].length
        ).toBe(2);
        expect(
            wrapperSingleReport.find(reportIdTag).props()["options"][1].text
        ).toBe("Medications");
    });

    test("Available report types - multiple", () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
            ["Encounter"]: true,
            ["Laboratory"]: true,
            ["Immunization"]: true,
            ["MedicationRequest"]: true,
        };
        const options = new StoreOptionsStub();
        options.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const wrapperMultipleReport = createWrapper(options);

        // Check values
        expect(
            wrapperMultipleReport.find(reportIdTag).props()["options"].length
        ).toBe(6);
    });

    test("Select med report", async () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
            ["Encounter"]: true,
            ["Laboratory"]: true,
            ["Immunization"]: true,
            ["MedicationRequest"]: true,
        };
        const storeOptions = new StoreOptionsStub();
        storeOptions.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const wrapper = createWrapper(storeOptions, {
            "b-form-select": BFormSelect,
        });

        // Check values
        expect(wrapper.find(testIdExclusion).isVisible()).toBe(false);
        const comboOptions = wrapper.find(reportIdTag).findAll("option");
        await comboOptions.at(1).setSelected();

        expect(wrapper.find(testIdExclusion).isVisible()).toBe(true);
    });

    test("Advanced cancel", async () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
            ["Encounter"]: true,
            ["Laboratory"]: true,
            ["Immunization"]: true,
            ["MedicationRequest"]: true,
        };
        const storeOptions = new StoreOptionsStub();
        storeOptions.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const wrapper = createWrapper(storeOptions, {
            "b-form-select": BFormSelect,
            "hg-date-picker": BFormInput,
            "hg-button": BButton,
        });

        expect(wrapper.find(testIdSelectedDates).exists()).toBe(false);
        expect(wrapper.find(testIdClearFilter).exists()).toBe(false);

        // Select a start date
        const startInput = wrapper.find("[data-testid=startDateInput]");
        const today = new DateWrapper();
        startInput.setValue(today.toISODate());

        // Clear the filter
        await wrapper.find("[data-testid=clearBtn]").trigger("click");
        expect(wrapper.find(testIdSelectedDates).exists()).toBe(false);
        expect(startInput.props().value).toBe(null);
    });

    test("Date Filter clear", async () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
            ["Encounter"]: true,
            ["Laboratory"]: true,
            ["Immunization"]: true,
            ["MedicationRequest"]: true,
        };
        const storeOptions = new StoreOptionsStub();
        storeOptions.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const wrapper = createWrapper(storeOptions, {
            "b-form-select": BFormSelect,
            "hg-date-picker": BFormInput,
            "b-form-tag": BFormTag,
            "hg-button": BButton,
        });

        expect(wrapper.find(testIdSelectedDates).exists()).toBe(false);
        expect(wrapper.find(testIdClearFilter).exists()).toBe(false);

        // Select a start date
        const startInput = wrapper.find("[data-testid=startDateInput]");
        const today = new DateWrapper();
        startInput.setValue(today.toISODate());

        // Apply the filter
        await wrapper.find("[data-testid=applyFilterBtn]").trigger("click");
        expect(wrapper.find(testIdSelectedDates).exists()).toBe(true);
        expect(wrapper.find(testIdClearFilter).exists()).toBe(true);
        expect(startInput.props().value).toBe(today.toISODate());

        // Clear the filter
        await wrapper.find(testIdClearFilter).find("button").trigger("click");
        expect(wrapper.find(testIdSelectedDates).exists()).toBe(false);
        expect(wrapper.find(testIdClearFilter).exists()).toBe(false);
    });

    test("Export button", async () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
            ["Encounter"]: true,
            ["Laboratory"]: true,
            ["Immunization"]: true,
            ["MedicationRequest"]: true,
        };
        const storeOptions = new StoreOptionsStub();
        storeOptions.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;

        const wrapper = createWrapper(storeOptions, {
            "b-form-select": BFormSelect,
            "message-modal": BForm,
        });
        const comboOptions = wrapper.find(reportIdTag).findAll("option");

        const testIdModal = "[data-testid=messageModal]";

        // Execute Medication report
        await comboOptions.at(1).setSelected();
        const mockedMedMethod = jest.fn().mockResolvedValue(undefined);
        (wrapper.vm.$refs
            .medicationHistoryReport as ReportComponent).generatePdf = mockedMedMethod;

        await wrapper.find(testIdModal).trigger("submit");

        // Execute Encounter report
        await comboOptions.at(2).setSelected();
        const mockedEncMethod = jest.fn().mockResolvedValue(undefined);
        (wrapper.vm.$refs
            .mspVisitsReport as ReportComponent).generatePdf = mockedEncMethod;

        await wrapper.find(testIdModal).trigger("submit");

        // Execute Lab report
        await comboOptions.at(3).setSelected();
        const mockedLabMethod = jest.fn().mockResolvedValue(undefined);
        (wrapper.vm.$refs
            .covid19Report as ReportComponent).generatePdf = mockedLabMethod;

        await wrapper.find(testIdModal).trigger("submit");

        // Execute Immz report
        await comboOptions.at(4).setSelected();
        const mockedImmzMethod = jest.fn().mockResolvedValue(undefined);
        (wrapper.vm.$refs
            .immunizationHistoryReport as ReportComponent).generatePdf = mockedImmzMethod;

        await wrapper.find(testIdModal).trigger("submit");

        // Execute Med Request report
        await comboOptions.at(5).setSelected();
        const mockedMedRequestMethod = jest.fn().mockResolvedValue(undefined);
        (wrapper.vm.$refs
            .medicationRequestReport as ReportComponent).generatePdf = mockedMedRequestMethod;

        await wrapper.find(testIdModal).trigger("submit");

        // Execute No recors does nothing
        await comboOptions.at(0).setSelected();
        await wrapper.find(testIdModal).trigger("submit");

        expect(mockedMedMethod).toHaveBeenCalledTimes(1);
        expect(mockedEncMethod).toHaveBeenCalledTimes(1);
        expect(mockedLabMethod).toHaveBeenCalledTimes(1);
        expect(mockedImmzMethod).toHaveBeenCalledTimes(1);
        expect(mockedMedRequestMethod).toHaveBeenCalledTimes(1);
    });

    test("Medication filter", async () => {
        // Setup vuex store
        webclientConfig.modules = {
            ["Medication"]: true,
        };
        const storeOptions = new StoreOptionsStub();
        storeOptions.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;
        const firstMed = JSON.parse(JSON.stringify(baseMedication));
        firstMed.medicationSummary.brandName = "Brand Name A";
        const seconMed = JSON.parse(JSON.stringify(baseMedication));
        seconMed.medicationSummary.brandName = "Brand Name B";
        const thirdMed = JSON.parse(JSON.stringify(baseMedication));
        thirdMed.medicationSummary.brandName = "Brand Name B";
        const fourthMed = JSON.parse(JSON.stringify(baseMedication));
        fourthMed.medicationSummary.brandName = "Brand Name C";
        storeOptions.modules.medication.modules.statement.getters.medicationStatements = (): MedicationStatementHistory[] => {
            return [firstMed, seconMed, thirdMed, fourthMed];
        };
        const wrapper = createWrapper(storeOptions, {
            "b-form-select": BFormSelect,
        });

        // Check values
        expect(wrapper.find(testIdExclusion).isVisible()).toBe(false);
        const comboOptions = wrapper.find(reportIdTag).findAll("option");
        await comboOptions.at(1).setSelected();

        expect(wrapper.find(testIdExclusion).isVisible()).toBe(true);

        expect(wrapper.find(testIdExclusion).props()["options"].length).toBe(3);
    });
});
