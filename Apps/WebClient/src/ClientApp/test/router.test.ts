// eslint-disable-next-line
import container from "@/plugins/inversify.container";

import StoreOptionsStub from "@test/stubs/store/storeOptionsStub";
import "setimmediate";
import { Route } from "vue-router";

import { ClientModule } from "@/constants/clientModule";
import { RegistrationStatus } from "@/constants/registrationStatus";
import { WebClientConfiguration } from "@/models/configData";
import { SERVICE_IDENTIFIER, STORE_IDENTIFIER } from "@/plugins/inversify";
import router, { beforeEachGuard, UserState } from "@/router";
import { ILogger, IStoreProvider } from "@/services/interfaces";
import { WinstonLogger } from "@/services/winstonLogger";
import StoreProvider from "@/store/provider";
import { GatewayStoreOptions } from "@/store/types";
import NotFoundComponent from "@/views/errors/notFound.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";
import LandingComponent from "@/views/landing.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import ProfileComponent from "@/views/profile.vue";
import PublicCovidTestComponent from "@/views/publicCovidTest.vue";
import RegistrationComponent from "@/views/registration.vue";

function flushPromises() {
    return new Promise((resolve) => setImmediate(resolve));
}

function setOidcCheckUser(options: GatewayStoreOptions, value: boolean): void {
    options.modules.auth.actions.oidcCheckUser = (): Promise<boolean> => {
        return new Promise<boolean>((resolve) => {
            resolve(value);
        });
    };
}

interface RouteMeta {
    stateless?: boolean;
    routeIsOidcCallback?: boolean;
    requiredModules?: ClientModule[];
    validStates?: UserState[];
}

function createToRoute(meta: RouteMeta): Route {
    return {
        path: "/stateles",
        hash: "abc123",
        query: {},
        params: {},
        fullPath: "base/stateles",
        matched: [],
        meta: meta,
    };
}

const webclientConfig: WebClientConfiguration = {
    logLevel: "debug",
    timeouts: { idle: 50, logoutRedirect: "", resendSMS: 3 },
    registrationStatus: RegistrationStatus.Open,
    externalURLs: {},
    modules: {},
    hoursForDeletion: 1,
    minPatientAge: 21,
    maxDependentAge: 21,
};

describe("Router", () => {
    container
        .bind<ILogger>(SERVICE_IDENTIFIER.Logger)
        .to(WinstonLogger)
        .inSingletonScope();
    container
        .bind<IStoreProvider>(STORE_IDENTIFIER.StoreProvider)
        .to(StoreProvider)
        .inRequestScope();
    container
        .bind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
        .to(StoreOptionsStub)
        .inRequestScope();

    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    const registrationPath = "/registration";

    test("has landing route", () => {
        const actualComponent = router.getMatchedComponents("/")[0];
        expect(actualComponent.name).toBe(LandingComponent.name);
    });

    test("has covidtest route", () => {
        const actualComponent = router.getMatchedComponents("/covidtest")[0];
        expect(actualComponent.name).toBe(PublicCovidTestComponent.name);
    });

    test("has profile route", () => {
        const result = router.resolve("/profile");
        expect(result.resolved.matched[0].components.default.name).toBe(
            ProfileComponent.name
        );
    });

    test("has registration route", () => {
        const actualComponent =
            router.getMatchedComponents(registrationPath)[0];
        expect(actualComponent.name).toBe(RegistrationComponent.name);
    });

    test("has logout route", () => {
        const actualComponent = router.getMatchedComponents("/logout")[0];
        expect(actualComponent.name).toBe(LogoutComponent.name);
    });

    test("has login route", () => {
        const actualComponent = router.getMatchedComponents("/login")[0];
        expect(actualComponent.name).toBe(LoginComponent.name);
    });

    test("has unauthorized route", () => {
        const actualComponent = router.getMatchedComponents("/unauthorized")[0];
        expect(actualComponent.name).toBe(UnauthorizedComponent.name);
    });

    test("handles unexisting route", () => {
        const actualComponent = router.getMatchedComponents(
            "/paththatdoesnotexist"
        )[0];
        expect(actualComponent.name).toBe(NotFoundComponent.name);
    });

    it("To Stateless route", () => {
        const from = router.resolve("/").route;
        const to = createToRoute({ stateless: true });

        const next = jest.fn();

        beforeEachGuard(to, from, next);
        expect(next).toHaveBeenCalledWith();
    });

    it("To routeIsOidcCallback route", () => {
        const from = router.resolve("/").route;
        const to = createToRoute({
            routeIsOidcCallback: true,
        });
        const next = jest.fn();

        beforeEachGuard(to, from, next);
        expect(next).toHaveBeenCalledWith();
    });

    it("With required modules", async () => {
        const from = router.resolve("/").route;
        const to = createToRoute({
            requiredModules: [ClientModule.Dependent],
            validStates: [UserState.registered],
        });

        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        webclientConfig.modules = { [ClientModule.Dependent]: true };
        options.modules.config.getters.webClient = (): WebClientConfiguration =>
            webclientConfig;

        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        const next = jest.fn();
        beforeEachGuard(to, from, next);
        await flushPromises();
        expect(next).toHaveBeenCalledWith();
    });

    it("Offline Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => {
            return true;
        };
        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve("/timeline").route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({ path: "/" });
    });

    it("PendingDeletion Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => false;
        options.modules.user.getters.userIsActive = (): boolean => false;

        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve("/timeline").route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({ path: "/profile" });
    });

    it("Registered Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => false;
        options.modules.user.getters.userIsActive = (): boolean => true;
        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve(registrationPath).route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({ path: "/home" });
    });

    it("Unregistered Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => false;
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => true;
        options.modules.auth.getters.isValidIdentityProvider = (): boolean =>
            true;
        options.modules.user.getters.userIsRegistered = (): boolean => false;
        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve("/timeline").route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({ path: registrationPath });
    });

    it("Bad idp Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => false;
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => true;
        options.modules.auth.getters.isValidIdentityProvider = (): boolean =>
            false;
        options.modules.user.getters.userIsRegistered = (): boolean => false;
        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve("/timeline").route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({ path: "/idirLoggedIn" });
    });

    it("Not authenticated Default Path", async () => {
        // Config store
        const options = new StoreOptionsStub();
        setOidcCheckUser(options, true);
        options.modules.config.getters.isOffline = (): boolean => false;
        options.modules.auth.getters.oidcIsAuthenticated = (): boolean => false;
        container
            .rebind<GatewayStoreOptions>(STORE_IDENTIFIER.StoreOptions)
            .toConstantValue(options);

        // Setup params
        const from = router.resolve("/").route;
        const to = router.resolve("/timeline").route;
        const next = jest.fn();

        // Execute test
        beforeEachGuard(to, from, next);
        await flushPromises();

        // Assert
        expect(next).toHaveBeenCalledWith({
            path: "/login",
            query: { redirect: to.path },
        });
    });
});
