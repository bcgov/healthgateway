import { createLocalVue, Wrapper, mount } from "@vue/test-utils";
import BootstrapVue from "bootstrap-vue";
import TimelineComponent from "@/views/timeline.vue";
import VueRouter from "vue-router";
import Vuex from "vuex";
import { IMedicationService, IHttpDelegate } from "@/services/interfaces";
import { ExternalConfiguration } from "@/models/configData";
import MedicationStatement from "@/models/medicationStatement";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { injectable } from "inversify";
import container from "@/inversify.config";
import { user as userModule } from "@/store/modules/user/user";
import User from "@/models/user";
import medicationSumary from "@/models/medicationSumary";
import Pharmacy from "@/models/pharmacy";
import RequestResult from "@/models/requestResult";

const today = new Date();
var yesterday = new Date(today);
yesterday.setDate(today.getDate() - 1);
const pharmacy: Pharmacy = {
  pharmacyId: "123",
  addressLine1: "TEST",
  addressLine2: "TEST2"
};
const medicationStatements: MedicationStatement[] = [
  {
    medicationSumary: {
      brandName: "brand_name_A",
      genericName: "generic_name_A"
    },
    dispensedDate: today,
    pharmacyId: "123"
  },
  {
    medicationSumary: {
      brandName: "brand_name_B",
      genericName: "generic_name_B"
    },
    dispensedDate: today,
    pharmacyId: "123"
  },
  {
    medicationSumary: {
      brandName: "brand_name_C",
      genericName: "generic_name_C"
    },
    dispensedDate: yesterday,
    pharmacyId: "123"
  }
];

@injectable()
class MockMedicationService implements IMedicationService {
  initialize(config: ExternalConfiguration, http: IHttpDelegate): void {
    // No need to implement for the mock
    throw new Error("Method not implemented.");
  }
  getPatientMedicationStatements(hdid: string): Promise<RequestResult> {
    return new Promise<RequestResult>((resolve, reject) => {
      if (hdid === "hdid_with_results") {
        resolve({
          totalResultCount: medicationStatements.length,
          pageIndex: 0,
          pageSize: medicationStatements.length,
          resultStatus: 0,
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
    });
  }
  getPharmacyInfo(pharmacyId: string): Promise<RequestResult> {
    return new Promise<RequestResult>((resolve, reject) => {
      if (pharmacyId === "123") {
        resolve({
          totalResultCount: 1,
          pageIndex: 0,
          pageSize: 1,
          resultStatus: 0,
          resultMessage: "",
          resourcePayload: pharmacy
        });
      } else {
        reject({
          error: "Pharmacy with " + pharmacyId + " not found."
        });
      }
    });
  }
}

const pushMethod = jest.fn();

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
      }
    }
  });

  return mount(TimelineComponent, {
    localVue,
    store: customStore,
    mocks: {
      $route,
      $router
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
        let user = new User();

        user.firstName = "First Name";
        user.lastName = "Last Name";
        user.email = "test@test.com";
        user.hdid = "hdid_with_results";
        user.phn = "";
        return user;
      }
    };

    let wrapper = createWrapper();
    // Verify the number of records
    wrapper.vm.$nextTick(() => {
      expect(wrapper.findAll(".entryCard").length).toEqual(3);
      expect(wrapper.findAll(".date").length).toEqual(2);
    });
  });

  test("sort button toggles", () => {
    userGetters = {
      user: (): User => {
        let user = new User();

        user.firstName = "Another Name";
        user.lastName = "Last Name";
        user.email = "test@test.com";
        user.hdid = "hdid_with_results";
        user.phn = "";
        return user;
      }
    };

    let wrapper = createWrapper();
    wrapper.vm.$nextTick(() => {
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

      expect(
        wrapper.find(".sortContainer button [name='descending']").isVisible()
      ).toBe(false);
      expect(
        wrapper.find(".sortContainer button [name='ascending']").isVisible()
      ).toBe(true);

      dates = wrapper.findAll(".date");
      topDate = new Date(dates.at(0).text());
      bottomDate = new Date(dates.at(1).text());
      expect(topDate > bottomDate).toBe(false);
    });
  });
});
