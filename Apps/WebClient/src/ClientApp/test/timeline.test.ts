import { Wrapper, createLocalVue, mount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import TimelineComponent from "@/views/timeline.vue";
import VueRouter from "vue-router";
import Vuex from "vuex";
import { IHttpDelegate, IMedicationService } from "@/services/interfaces";
import {
    ExternalConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { injectable } from "inversify";
import { user as userModule } from "@/store/modules/user/user";
import User from "@/models/user";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import MedicationResult from "@/models/medicationResult";
import Pharmacy from "@/models/pharmacy";
import { LaboratoryOrder } from "@/models/laboratory";
import { LaboratoryState } from "@/models/storeState";
import Router from "vue-router";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const METHOD_NOT_IMPLEMENTED: string = "Method not implemented.";
const today = new Date();
const yesterday = new Date(today);

const userWithResults = new User();
userWithResults.hdid = "hdid_with_results";

yesterday.setDate(today.getDate() - 1);
const medicationStatements: MedicationStatementHistory[] = [
    {
        medicationSumary: {
            din: "1233",
            brandName: "brand_name_A",
            genericName: "generic_name_A",
        },
        prescriptionIdentifier: "abcmed1",
        dispensedDate: today,
        dispensingPharmacy: {},
    },
    {
        medicationSumary: {
            din: "1234",
            brandName: "brand_name_B",
            genericName: "generic_name_B",
        },
        prescriptionIdentifier: "abcmed2",
        dispensedDate: today,
        dispensingPharmacy: {},
    },
    {
        medicationSumary: {
            din: "1235",
            brandName: "brand_name_C",
            genericName: "generic_name_C",
        },
        prescriptionIdentifier: "abcmed3",
        dispensedDate: yesterday,
        dispensingPharmacy: {},
    },
];

@injectable()
class MockMedicationService implements IMedicationService {
    initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
        // No need to implement for the mock
        throw new Error(METHOD_NOT_IMPLEMENTED);
    }
    getPatientMedicationStatementHistory(
        hdid: string
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise<RequestResult<MedicationStatementHistory[]>>(
            (resolve, reject) => {
                if (hdid === "hdid_with_results") {
                    resolve({
                        totalResultCount: medicationStatements.length,
                        pageIndex: 0,
                        pageSize: medicationStatements.length,
                        resultStatus: ResultType.Success,
                        resultMessage: "",
                        resourcePayload: medicationStatements,
                        errorCode: "",
                    });
                } else if (hdid === "hdid_no_results") {
                    resolve();
                } else {
                    reject({
                        error: "User with " + hdid + " not found.",
                    });
                }
            }
        );
    }
    getMedicationInformation(
        drugIdentifier: string
    ): Promise<MedicationResult> {
        throw new Error(METHOD_NOT_IMPLEMENTED);
    }
    getPharmacyInfo(pharmacyId: string): Promise<Pharmacy> {
        // No need to implement for the mock
        throw new Error(METHOD_NOT_IMPLEMENTED);
    }
}

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

let laboratoryActions = {
    getOrders(
        context: any,
        params: { hdid: string }
    ): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise((resolve, reject) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultMessage: "From storage",
                resultStatus: ResultType.Success,
                totalResultCount: 0,
                errorCode: "",
            });
        });
    },
};

let laboratoryGetters = {
    getStoredLaboratoryOrders: (
        state: LaboratoryState
    ) => (): LaboratoryOrder[] => {
        return [];
    },
};

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
    localVue.use(Vuex);

    container
        .rebind<IMedicationService>(SERVICE_IDENTIFIER.MedicationService)
        .to(MockMedicationService)
        .inSingletonScope();

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
