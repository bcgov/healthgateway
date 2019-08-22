import { shallowMount, createLocalVue, Wrapper, config } from "@vue/test-utils";
import Vuex from "vuex";
import LoginComponent from "@/views/login.vue";
import { auth as authModule } from "@/store/modules/auth/auth";
import BootstrapVue from "bootstrap-vue";
import { IdentityProviderConfiguration } from "@/models/configData";

const pushMethod = jest.fn();

let $router = {};
let $route = {
  path: "",
  query: {
    redirect: ""
  }
};

let authGetters = {
  oidcIsAuthenticated: (): boolean => false,
  userIsRegistered: (): boolean => false
};

let configGetters = {
  identityProviders: (): IdentityProviderConfiguration[] => []
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
      },
      config: {
        namespaced: true,
        getters: configGetters
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

  test("if authenticated but not registered sets router path to registration", () => {
    authGetters = {
      oidcIsAuthenticated: (): boolean => true,
      userIsRegistered: (): boolean => false
    };
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe("/registration");
    expect(pushMethod).toHaveBeenCalledWith({
      path: wrapper.vm.$data.redirectPath
    });
  });

  test("if authenticated and registered sets router path", () => {
    authGetters = {
      oidcIsAuthenticated: (): boolean => true,
      userIsRegistered: (): boolean => true
    };
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
    expect(pushMethod).toHaveBeenCalledWith({
      path: wrapper.vm.$data.redirectPath
    });
  });

  test("if not authenticated does not set router path", () => {
    authGetters = {
      oidcIsAuthenticated: (): boolean => false,
      userIsRegistered: (): boolean => false
    };
    let wrapper = createWrapper();
    expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
    expect(pushMethod).not.toHaveBeenCalledTimes(0);
  });

  test("has correct identity provider", () => {
    const bceidProvider: IdentityProviderConfiguration = {
      name: "BC Services Card",
      id: "bceid",
      icon: "bceidicon"
    };
    const keycloakProvider: IdentityProviderConfiguration = {
      name: "keycloak",
      id: "keyid",
      icon: "fa-user",
      disabled: true
    };

    configGetters = {
      identityProviders: (): IdentityProviderConfiguration[] => [
        bceidProvider,
        keycloakProvider
      ]
    };
    let wrapper = createWrapper();
    expect(
      wrapper
        .find(`#${bceidProvider.id}Btn`)
        .find(`.${bceidProvider.icon}`)
        .isVisible()
    ).toBe(true);
    expect(wrapper.find(`#${bceidProvider.id}Btn`).text()).toBe(
      bceidProvider.name
    );
    expect(wrapper.find(`#${bceidProvider.id}Btn`).attributes("disabled")).toBe(
      bceidProvider.disabled
    );

    expect(
      wrapper
        .find(`#${keycloakProvider.id}Btn`)
        .find(`.${keycloakProvider.icon}`)
        .isVisible()
    ).toBe(true);
    expect(wrapper.find(`#${keycloakProvider.id}Btn`).text()).toBe(
      keycloakProvider.name
    );
    expect(
      wrapper.find(`#${keycloakProvider.id}Btn`).attributes("disabled")
    ).toBe(`${keycloakProvider.disabled}`);

    expect(wrapper.find(`#loginPicker`).findAll("[type=button]").length).toBe(
      2
    );
  });
});
