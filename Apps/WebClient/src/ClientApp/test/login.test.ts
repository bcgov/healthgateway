import { shallowMount, createLocalVue, Wrapper } from "@vue/test-utils";
import Vuex from "vuex";
import LoginComponent from "@/views/login.vue";
import { auth as authModule } from "@/store/modules/auth/auth";
import BootstrapVue from "bootstrap-vue";

const pushMethod = jest.fn();

let $router = {};
let $route = {
  path: "",
  query: {
    redirect: ""
  }
};

let authGetters = {
  isAuthenticated: (): boolean => false
};

function createWrapper(): Wrapper<LoginComponent> {
  const localVue = createLocalVue();
  localVue.use(Vuex);
  localVue.use(BootstrapVue);

  let customStore = new Vuex.Store({
    modules: {
      auth: {
        namespaced: true,
        getters: authGetters,
        actions: authModule.actions
      }
    }
  });

  return shallowMount(LoginComponent, {
    localVue,
    store: customStore,
    mocks: {
      $route,
      $router
    }
  });
}

describe("Login view", () => {
  beforeEach(() => {
    $router = {
      push: pushMethod
    };

    $route = {
      path: "/somePath",
      query: {
        redirect: "/anotherPath"
      }
    };
  });

  test("is a Vue instance", () => {
    let wrapper = createWrapper();
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  test("gets redirect route", () => {
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
  });

  test("sets default route if no redirect", () => {
    $route.query.redirect = "";
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe("/home");
  });

  test("if authenticated sets router path", () => {
    authGetters = {
      isAuthenticated: (): boolean => true
    };
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
    expect(pushMethod).toHaveBeenCalledWith({
      path: wrapper.vm.$data.redirectPath
    });
  });

  test("if not authenticated does not set router path", () => {
    authGetters = {
      isAuthenticated: (): boolean => false
    };
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
    expect(pushMethod).not.toHaveBeenCalledTimes(0);
  });
});
