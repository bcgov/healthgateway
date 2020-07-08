import { Wrapper, createLocalVue, shallowMount } from "@vue/test-utils";
import Vuex from "vuex";
import BootstrapVue from "bootstrap-vue";
import LoginComponent from "@/views/login.vue";
import { auth as authModule } from "@/store/modules/auth/auth";
import { user as userModule } from "@/store/modules/user/user";
import { IdentityProviderConfiguration } from "@/models/configData";

const pushMethod = jest.fn();

let $router = {};
let $route = {
    path: "",
    query: {
        redirect: "",
    },
};

let authGetters = {
    oidcIsAuthenticated: (): boolean => false,
};

let userGetters = {
    userIsRegistered: (): boolean => false,
};

let configGetters = {
    identityProviders: (): IdentityProviderConfiguration[] => [],
};

function createWrapper(): Wrapper<LoginComponent> {
    const localVue = createLocalVue();
    localVue.use(Vuex);
    localVue.use(BootstrapVue);

    const customStore = new Vuex.Store({
        modules: {
            auth: {
                namespaced: true,
                getters: authGetters,
                actions: userModule.actions,
            },
            user: {
                namespaced: true,
                getters: userGetters,
                actions: authModule.actions,
            },
            config: {
                namespaced: true,
                getters: configGetters,
            },
        },
    });

    return shallowMount(LoginComponent, {
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

describe("Login view", () => {
    beforeEach(() => {
        $router = {
            push: pushMethod,
        };

        $route = {
            path: "/somePath",
            query: {
                redirect: "/anotherPath",
            },
        };
    });

    test("is a Vue instance", () => {
        const wrapper = createWrapper();
        expect(wrapper).toBeTruthy();
    });

    test("gets redirect route", () => {
        const wrapper = createWrapper();
        expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
    });

    test("sets default route if no redirect", () => {
        $route.query.redirect = "";
        const wrapper = createWrapper();
        expect(wrapper.vm.$data.redirectPath).toBe("/timeline");
    });

    test("if authenticated but not registered sets router path to registration", () => {
        authGetters = {
            oidcIsAuthenticated: (): boolean => true,
        };
        userGetters = {
            userIsRegistered: (): boolean => false,
        };
        const wrapper = createWrapper();
        expect(wrapper.vm.$data.redirectPath).toBe("/registrationInfo");
        expect(pushMethod).toHaveBeenCalledWith({
            path: wrapper.vm.$data.redirectPath,
        });
    });

    test("if authenticated and registered sets router path", () => {
        authGetters = {
            oidcIsAuthenticated: (): boolean => true,
        };
        userGetters = {
            userIsRegistered: (): boolean => true,
        };
        const wrapper = createWrapper();
        expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
        expect(pushMethod).toHaveBeenCalledWith({
            path: wrapper.vm.$data.redirectPath,
        });
    });

    test("if not authenticated does not set router path", () => {
        authGetters = {
            oidcIsAuthenticated: (): boolean => false,
        };
        userGetters = {
            userIsRegistered: (): boolean => false,
        };
        const wrapper = createWrapper();
        expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
        expect(pushMethod).not.toHaveBeenCalledTimes(0);
    });

    test("has correct identity provider", () => {
        const bceidProvider: IdentityProviderConfiguration = {
            name: "BC Services Card",
            id: "bceid",
            icon: "bceidicon",
            hint: "bceid",
            disabled: false,
        };
        const keycloakProvider: IdentityProviderConfiguration = {
            name: "keycloak",
            id: "keyid",
            icon: "user",
            hint: "bceid",
            disabled: false,
        };

        configGetters = {
            identityProviders: (): IdentityProviderConfiguration[] => [
                bceidProvider,
                keycloakProvider,
            ],
        };
        const wrapper = createWrapper();
        expect(
            wrapper
                .find(`#${bceidProvider.id}Btn`)
                .find(`[icon="${bceidProvider.icon}" ]`)
                .isVisible()
        ).toBe(true);
        expect(wrapper.find(`#${bceidProvider.id}Btn`).text()).toBe(
            bceidProvider.name
        );
        expect(
            wrapper
                .find(`#${keycloakProvider.id}Btn`)
                .find(`[icon="${keycloakProvider.icon}" ]`)
                .isVisible()
        ).toBe(true);
        expect(wrapper.find(`#${keycloakProvider.id}Btn`).text()).toBe(
            keycloakProvider.name
        );
        expect(
            wrapper.find(`#loginPicker`).findAll("[type=button]").length
        ).toBe(2);
    });
});
