import { Wrapper, createLocalVue, mount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import ReportsView from "@/views/reports.vue";
import VueRouter from "vue-router";
import VueContentPlaceholders from "vue-content-placeholders";
import Vuex, { ActionTree } from "vuex";
import { WebClientConfiguration } from "@/models/configData";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { user as userModule } from "@/store/modules/user/user";
import User from "@/models/user";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import {
    LaboratoryState,
    MedicationState,
    RootState,
} from "@/models/storeState";
import Router from "vue-router";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const today = new Date();
const yesterday = new Date(today);

const userWithResults = new User();
userWithResults.hdid = "hdid_with_results";

yesterday.setDate(today.getDate() - 1);
const medicationStatements: MedicationStatementHistory[] = [
    {
        medicationSummary: {
            din: "1233",
            brandName: "brand_name_A",
            genericName: "generic_name_A",
            isPin: false,
        },
        prescriptionIdentifier: "abcmed1",
        dispensedDate: today,
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
        dispensedDate: today,
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
        dispensedDate: yesterday,
        dispensingPharmacy: {},
    },
];

const $router = {};
const $route = {
    path: "",
    query: {
        redirect: "",
    },
};

let userGetters = {
    user: (): User => {
        return new User();
    },
};

let medicationActions: ActionTree<MedicationState, RootState> = {
    getMedicationStatements(
        context,
        params: {
            hdid: string;
            protectiveWord?: string;
        }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise((resolve, reject) => {
            if (params.hdid === "hdid_with_results") {
                resolve({
                    totalResultCount: medicationStatements.length,
                    pageIndex: 0,
                    pageSize: medicationStatements.length,
                    resultStatus: ResultType.Success,
                    resourcePayload: medicationStatements,
                });
            } else if (params.hdid === "hdid_no_results") {
                resolve();
            } else {
                reject({
                    error: "User with " + params.hdid + " not found.",
                });
            }
        });
    },
};

let medicationGetters = {};

const configGetters = {
    webClient: (): WebClientConfiguration => {
        return {
            modules: { Note: true },
        };
    },
};

function createWrapper(): Wrapper<ReportsView> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(BootstrapVue);
    localVue.use(Router);
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
            medication: {
                namespaced: true,
                getters: medicationGetters,
                actions: medicationActions,
            },
        },
    });

    return mount(TimelineComponent, {
        localVue,
        store: customStore,
        mocks: {
            $route,
            $router,
        },
        stubs: {
            "font-awesome-icon": true,
        },
    });
}

describe("Reports view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    const localVue = createLocalVue();
    localVue.use(BootstrapVue);
    localVue.use(VueRouter);
    localVue.use(VueContentPlaceholders);
    localVue.use(Vuex);

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("has header element with static text", () => {
        const expectedH1Text = "Health Gateway Drug History Report";
        const wrapper = createWrapper();
        expect(wrapper.find("h1").text()).toBe(expectedH1Text);
    });
});
