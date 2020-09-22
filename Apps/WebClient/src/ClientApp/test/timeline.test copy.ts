import { Wrapper, createLocalVue, mount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import TimelineComponent from "@/views/timeline.vue";
import VueRouter from "vue-router";
import VueContentPlaceholders from "vue-content-placeholders";
import Vuex, { ActionTree } from "vuex";
import { WebClientConfiguration } from "@/models/configData";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { user as userModule } from "@/store/modules/user/user";
import User from "@/models/user";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import { LaboratoryOrder } from "@/models/laboratory";
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

let laboratoryActions: ActionTree<LaboratoryState, RootState> = {
    getOrders(): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise((resolve) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        });
    },
};

let laboratoryGetters = {
    getStoredLaboratoryOrders: () => (): LaboratoryOrder[] => {
        return [];
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

function createWrapper(): Wrapper<TimelineComponent> {
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

describe("Timeline view", () => {
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
        var unwatch = wrapper.vm.$watch(
            () => {
                return wrapper.vm.$data.isLoading;
            },
            () => {
                expect(wrapper.findAll(".entryCard").length).toEqual(3);
                expect(wrapper.findAll(".entryCard").length).toEqual(3);
                expect(wrapper.findAll(".date").length).toEqual(2);
                unwatch();
            }
        );
    });

    test("sort button toggles", () => {
        userGetters = {
            user: (): User => {
                const user = new User();
                user.hdid = "hdid_with_results";
                return user;
            },
        };

        const wrapper = createWrapper();
        var unwatch = wrapper.vm.$watch(
            () => {
                return wrapper.vm.$data.isLoading;
            },
            () => {
                expect(
                    wrapper
                        .find(".sortContainer button [name='descending']")
                        .isVisible()
                ).toBe(true);
                expect(
                    wrapper
                        .find(".sortContainer button [name='ascending']")
                        .isVisible()
                ).toBe(false);
                let dates = wrapper.findAll(".date");
                let topDate = new Date(dates.at(0).text());
                let bottomDate = new Date(dates.at(1).text());
                expect(topDate > bottomDate).toBe(true);

                wrapper.find(".sortContainer button").trigger("click");
                wrapper.vm.$nextTick(() => {
                    expect(
                        wrapper
                            .find(".sortContainer button [name='descending']")
                            .isVisible()
                    ).toBe(false);
                    expect(
                        wrapper
                            .find(".sortContainer button [name='ascending']")
                            .isVisible()
                    ).toBe(true);
                    dates = wrapper.findAll(".date");
                    topDate = new Date(dates.at(0).text());
                    bottomDate = new Date(dates.at(1).text());
                    expect(topDate > bottomDate).toBe(false);
                });
                unwatch();
            }
        );
    });
});
