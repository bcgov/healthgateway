import { SERVICE_IDENTIFIER, STORE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import "@/plugins/inversify.config";
import router, { beforeEachGuard, ClientModule } from "@/router";
import { ILogger, IStoreProvider } from "@/services/interfaces";
import NotFoundComponent from "@/views/errors/notFound.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";
import LandingComponent from "@/views/landing.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import ProfileComponent from "@/views/profile.vue";
import RegistrationComponent from "@/views/registration.vue";
import { Route } from "vue-router";

describe("Router", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");
    test("has landing route", () => {
        const actualComponent = router.getMatchedComponents("/")[0];
        expect(actualComponent.name).toBe(LandingComponent.name);
    });

    test("has profile route", () => {
        const result = router.resolve("/profile");
        expect(result.resolved.matched[0].components.default.name).toBe(
            ProfileComponent.name
        );
    });

    test("has registration route", () => {
        const actualComponent = router.getMatchedComponents("/registration")[0];
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
        /*if (to.meta.routeIsOidcCallback || to.meta.stateless) {
            next();
        }*/

        const from: Route = {
            path: "/",
            hash: "abc1234",
            query: {},
            params: {},
            fullPath: "base",
            matched: [],
        };

        const to: Route = {
            path: "/timeline",
            hash: "abc123",
            query: {},
            params: {},
            fullPath: "base/timeline",
            matched: [],
            meta: { stateless: true },
        };
        const next = jest.fn();

        beforeEachGuard(to, from, next);
        expect(next).toHaveBeenCalledWith();
    });

    it("To routeIsOidcCallback route", () => {
        /*if (to.meta.routeIsOidcCallback || to.meta.stateless) {
            next();
        }*/
        const from: Route = {
            path: "/",
            hash: "abc1234",
            query: {},
            params: {},
            fullPath: "base",
            matched: [],
        };

        const to: Route = {
            path: "/timeline",
            hash: "abc123",
            query: {},
            params: {},
            fullPath: "base/timeline",
            matched: [],
            meta: { routeIsOidcCallback: true },
        };
        const next = jest.fn();

        beforeEachGuard(to, from, next);
        expect(next).toHaveBeenCalledWith();
    });

    it("With required modules", () => {
        /*if (to.meta.routeIsOidcCallback || to.meta.stateless) {
            next();
        }*/
        const next = jest.fn();
        const storeWrapper: IStoreProvider = container.get(
            STORE_IDENTIFIER.StoreWrapper
        );

        //app.vm.$router.push("/timeline");

        //expect(router.beforeEach).toHaveBeenCalledWith();

        const from: Route = {
            path: "/",
            hash: "abc1234",
            query: {},
            params: {},
            fullPath: "base",
            matched: [],
        };

        const to: Route = {
            path: "/timeline",
            hash: "abc123",
            query: {},
            params: {},
            fullPath: "base/timeline",
            matched: [],
            meta: { requiredModules: [ClientModule.Dependent] },
        };

        beforeEachGuard(to, from, next);
        expect(next).toHaveBeenCalledWith();
    });
});
