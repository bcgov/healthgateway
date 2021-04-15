import { createLocalVue, mount, Wrapper } from "@vue/test-utils";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuex, { ActionTree } from "vuex";

import { RegistrationStatus } from "@/constants/registrationStatus";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { Dictionary } from "@/models/baseTypes";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LaboratoryOrder } from "@/models/laboratory";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import {
    CommentState,
    ErrorBannerState,
    ImmunizationState,
    LaboratoryState,
    MedicationStatementState,
    NavbarState,
    RootState,
} from "@/models/storeState";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import { user as userModule } from "@/store/modules/user/user";
import TimelineComponent from "@/views/timeline.vue";

const today = new DateWrapper();
const yesterday = today.subtract({ day: 1 });

const userWithResults = new User();
userWithResults.hdid = "hdid_with_results";
const medicationStatements: MedicationStatementHistory[] = [
    {
        medicationSummary: {
            din: "1233",
            brandName: "brand_name_A",
            genericName: "generic_name_A",
            isPin: false,
        },
        prescriptionIdentifier: "abcmed1",
        dispensedDate: today.toISODate(),
        dispensingPharmacy: {},
    },
    {
        medicationSummary: {
            din: "1234",
            brandName: "brand_name_B",
            genericName: "generic_name_B",
            isPin: false,
        },
        prescriptionIdentifier: "abcmed2",
        dispensedDate: today.toISODate(),
        dispensingPharmacy: {},
    },
    {
        medicationSummary: {
            din: "1235",
            brandName: "brand_name_C",
            genericName: "generic_name_C",
            isPin: true,
        },
        prescriptionIdentifier: "abcmed3",
        dispensedDate: yesterday.toISODate(),
        dispensingPharmacy: {},
    },
];

const a: WebClientConfiguration = {
    logLevel: "",
    timeouts: { idle: 0, logoutRedirect: "", resendSMS: 1 },
    registrationStatus: "closed" as RegistrationStatus,
    externalURLs: {},
    modules: { Note: true },
    hoursForDeletion: 1,
    minPatientAge: 16,
    maxDependentAge: 12,
};

function createWrapper(): Wrapper<TimelineComponent> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    const customStore = new Vuex.Store({
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
    });

    return mount(TimelineComponent, {
        localVue,
        store: customStore,
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
        userGetters = {
            user: (): User => {
                return userWithResults;
            },
        };

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
