import "@/plugins/inversify.config";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import { createLocalVue, shallowMount, Wrapper } from "@vue/test-utils";
import Vuex from "vuex";

import { IdentityProviderConfiguration } from "@/models/configData";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { GatewayStoreOptions } from "@/store/types";
import LoginComponent from "@/views/LoginView.vue";

const pushMethod = jest.fn();

let $router = {};
let $route = {
    path: "",
    query: {
        redirect: "",
    },
};

function createWrapper(options?: GatewayStoreOptions): Wrapper<LoginComponent> {
    const localVue = createLocalVue();
    localVue.use(Vuex);

    if (options === undefined) {
        options = new StoreOptionsStub();
    }

    const store = new Vuex.Store(options);

    return shallowMount(LoginComponent, {
        localVue,
        store: store,
        mocks: {
            $route,
            $router,
        },
        stubs: {
            "hg-icon": true,
            "hg-button": true,
        },
    });
}

describe("Login view", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
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
        expect(wrapper.vm.$data.redirectPath).toBe("/home");
    });

    test("if authenticated but not registered sets router path to registration", () => {
        const options = new StoreOptionsStub();
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => true;
        options.modules.user.getters.userIsRegistered = (): boolean => false;
        const wrapper = createWrapper(options);
        expect(wrapper.vm.$data.redirectPath).toBe("/registrationInfo");
        expect(pushMethod).toHaveBeenCalledWith({
            path: wrapper.vm.$data.redirectPath,
        });
    });

    test("if authenticated and registered sets router path", () => {
        const options = new StoreOptionsStub();
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => true;
        options.modules.user.getters.userIsRegistered = (): boolean => true;
        const wrapper = createWrapper(options);
        expect(wrapper.vm.$data.redirectPath).toBe($route.query.redirect);
        expect(pushMethod).toHaveBeenCalledWith({
            path: wrapper.vm.$data.redirectPath,
        });
    });

    test("if not authenticated does not set router path", () => {
        const options = new StoreOptionsStub();
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => false;
        options.modules.user.getters.userIsRegistered = (): boolean => false;
        const wrapper = createWrapper(options);
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
        const options = new StoreOptionsStub();
        options.modules.config.getters.identityProviders =
            (): IdentityProviderConfiguration[] => [
                bceidProvider,
                keycloakProvider,
            ];
        const wrapper = createWrapper(options);
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
            wrapper.find("#loginPicker").findAll("hg-button-stub").length
        ).toBe(3);
    });
});
