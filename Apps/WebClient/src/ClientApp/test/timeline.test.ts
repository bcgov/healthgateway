import { createLocalVue, Wrapper, mount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import TimelineComponent from "@/views/timeline.vue";
import VueRouter from "vue-router";
import Vuex from "vuex";
import { IMedicationService, IHttpDelegate } from "@/services/interfaces";
import {
  ExternalConfiguration,
  WebClientConfiguration
} from "@/models/configData";
import MedicationStatement from "@/models/medicationStatement";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { injectable } from "inversify";
import container from "@/plugins/inversify.config";
import { user as userModule } from "@/store/modules/user/user";
import User from "@/models/user";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";
import MedicationResult from "@/models/medicationResult";
import Pharmacy from "@/models/pharmacy";

const METHOD_NOT_IMPLEMENTED: string = "Method not implemented.";
const today = new Date();
var yesterday = new Date(today);

var userWithResults = new User();
userWithResults.hdid = "hdid_with_results";

yesterday.setDate(today.getDate() - 1);
const medicationStatements: MedicationStatement[] = [
  {
    medicationSumary: {
      brandName: "brand_name_A",
      genericName: "generic_name_A"
    },
    dispensedDate: today,
    pharmacyId: "pharmacyId"
  },
  {
    medicationSumary: {
      brandName: "brand_name_B",
      genericName: "generic_name_B"
    },
    dispensedDate: today,
    pharmacyId: "pharmacyId"
  },
  {
    medicationSumary: {
      brandName: "brand_name_C",
      genericName: "generic_name_C"
    },
    dispensedDate: yesterday,
    pharmacyId: "pharmacyId"
  }
];

@injectable()
class MockMedicationService implements IMedicationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    // No need to implement for the mock
    throw new Error(METHOD_NOT_IMPLEMENTED);
  }
  getPatientMedicationStatements(
    hdid: string
  ): Promise<RequestResult<MedicationStatement[]>> {
    return new Promise<RequestResult<MedicationStatement[]>>(
      (resolve, reject) => {
        if (hdid === "hdid_with_results") {
          resolve({
            totalResultCount: medicationStatements.length,
            pageIndex: 0,
            pageSize: medicationStatements.length,
            resultStatus: ResultType.Success,
            resultMessage: "",
            resourcePayload: medicationStatements
          });
        } else if (hdid === "hdid_no_results") {
          resolve();
        } else {
          reject({
            error: "User with " + hdid + " not found."
          });
        }
      }
    );
  }
  getMedicationInformation(drugIdentifier: string): Promise<MedicationResult> {
    throw new Error(METHOD_NOT_IMPLEMENTED);
  }
  getPharmacyInfo(pharmacyId: string): Promise<Pharmacy> {
    // No need to implement for the mock
    throw new Error(METHOD_NOT_IMPLEMENTED);
  }
}

let $router = {};
let $route = {
  path: "",
  query: {
    redirect: ""
  }
};

let userGetters = {
  user: (): User => {
    return new User();
  }
};

let configGetters = {
  webClient: (): WebClientConfiguration => {
    return {
      modules: { Note: true }
    };
  }
};

function createWrapper(): Wrapper<TimelineComponent> {
  const localVue = createLocalVue();
  localVue.use(Vuex);
  localVue.use(BootstrapVue);

  let customStore = new Vuex.Store({
    modules: {
      user: {
        namespaced: true,
        getters: userGetters,
        actions: userModule.actions
      },
      config: {
        namespaced: true,
        getters: configGetters
      }
    }
  });

  return mount(TimelineComponent, {
    localVue,
    store: customStore,
    mocks: {
      $route,
      $router
    },
    stubs: {
      "font-awesome-icon": true
    }
  });
}

describe("Timeline view", () => {
  const localVue = createLocalVue();
  localVue.use(BootstrapVue);
  localVue.use(VueRouter);
  localVue.use(Vuex);

  container
    .rebind<IMedicationService>(SERVICE_IDENTIFIER.MedicationService)
    .to(MockMedicationService)
    .inSingletonScope();

  test("is a Vue instance", () => {
    let wrapper = createWrapper();
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  test("has header element with static text", () => {
    const expectedH1Text = "Health Care Timeline";
    let wrapper = createWrapper();
    expect(wrapper.find("h1").text()).toBe(expectedH1Text);
  });

  test("Has entries", () => {
    userGetters = {
      user: (): User => {
        return userWithResults;
      }
    };

    let wrapper = createWrapper();
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
        let user = new User();
        user.hdid = "hdid_with_results";
        return user;
      }
    };

    let wrapper = createWrapper();
    var unwatch = wrapper.vm.$watch(
      () => {
        return wrapper.vm.$data.isLoading;
      },
      () => {
        expect(
          wrapper.find(".sortContainer button [name='descending']").isVisible()
        ).toBe(true);
        expect(
          wrapper.find(".sortContainer button [name='ascending']").isVisible()
        ).toBe(false);
        var dates = wrapper.findAll(".date");
        var topDate = new Date(dates.at(0).text());
        var bottomDate = new Date(dates.at(1).text());
        expect(topDate > bottomDate).toBe(true);

        wrapper.find(".sortContainer button").trigger("click");
        wrapper.vm.$nextTick(() => {
          expect(
            wrapper
              .find(".sortContainer button [name='descending']")
              .isVisible()
          ).toBe(false);
          expect(
            wrapper.find(".sortContainer button [name='ascending']").isVisible()
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
