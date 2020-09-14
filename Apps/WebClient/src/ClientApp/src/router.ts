import Vue from "vue";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

// Routes
import VueRouter, { Route } from "vue-router";
import store from "./store/store";
import { SnowplowWindow } from "@/plugins/extensions";
const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
declare let window: SnowplowWindow;

const ProfileView = () =>
    import(/* webpackChunkName: "profile" */ "@/views/profile.vue");
const LandingView = () =>
    import(/* webpackChunkName: "landing" */ "@/views/landing.vue");
const NotFoundView = () =>
    import(/* webpackChunkName: "notFound" */ "@/views/errors/notFound.vue");
const LoginView = () =>
    import(/* webpackChunkName: "login" */ "@/views/login.vue");
const LogoutView = () =>
    import(/* webpackChunkName: "logout" */ "@/views/logout.vue");
const UnauthorizedView = () =>
    import(
        /* webpackChunkName: "unauthorized" */ "@/views/errors/unauthorized.vue"
    );
const LoginCallbackView = () =>
    import(/* webpackChunkName: "loginCallback" */ "@/views/loginCallback.vue");
const RegistrationView = () =>
    import(/* webpackChunkName: "registration" */ "@/views/registration.vue");
const RegistrationInfoView = () =>
    import(
        /* webpackChunkName: "registrationInfo" */ "@/views/registrationInfo.vue"
    );
const TimelineView = () =>
    import(/* webpackChunkName: "timeline" */ "@/views/timeline.vue");
const ValidateEmailView = () =>
    import(/* webpackChunkName: "validateEmail" */ "@/views/validateEmail.vue");
const TermsOfServiceView = () =>
    import(
        /* webpackChunkName: "termsOfService" */ "@/views/termsOfService.vue"
    );
const HealthInsightsView = () =>
    import(
        /* webpackChunkName: "healthInsights" */ "@/views/healthInsights.vue"
    );
const ReleaseNotesView = () =>
    import(/* webpackChunkName: "releaseNotes" */ "@/views/releaseNotes.vue");

Vue.use(VueRouter);

const REGISTRATION_PATH = "/registration";
const REGISTRATION_INFO_PATH = "/registrationInfo";

const routes = [
    {
        path: "/",
        component: LandingView,
        meta: { requiresAuth: false },
    },
    {
        path: "/release-notes",
        component: ReleaseNotesView,
        meta: { requiresAuth: false },
    },
    {
        path: REGISTRATION_INFO_PATH,
        component: RegistrationInfoView,
        props: (route: Route) => ({
            inviteKey: route.query.inviteKey,
            email: route.query.email,
        }),
        meta: { requiresAuth: false },
    },
    {
        path: REGISTRATION_PATH,
        component: RegistrationView,
        props: (route: Route) => ({
            inviteKey: route.query.inviteKey,
            inviteEmail: route.query.email,
        }),
        meta: { requiresAuth: true },
    },
    {
        path: "/validateEmail/:inviteKey",
        component: ValidateEmailView,
        props: true,
        meta: { requiresAuth: true },
    },
    {
        path: "/profile",
        component: ProfileView,
        meta: { requiresRegistration: true, roles: ["user"] },
    },
    {
        path: "/timeline",
        component: TimelineView,
        meta: { requiresRegistration: true, roles: ["user"] },
    },
    {
        path: "/healthInsights",
        component: HealthInsightsView,
        meta: { requiresRegistration: true, roles: ["user"] },
    },
    {
        path: "/profile/termsOfService",
        component: TermsOfServiceView,
        meta: { requiresAuth: true, roles: ["user"] },
    },
    {
        path: "/login",
        component: LoginView,
        props: (route: Route) => ({
            isRetry: route.query.isRetry,
        }),
        meta: { requiresAuth: false, roles: ["user"] },
    },
    {
        path: "/loginCallback",
        component: LoginCallbackView,
        meta: {
            requiresAuth: false,
            roles: ["user"],
            routeIsOidcCallback: true,
        },
    },
    {
        path: "/logout",
        component: LogoutView,
        meta: { requiresAuth: false },
    },
    {
        path: "/unauthorized",
        component: UnauthorizedView,
        meta: { requiresAuth: false },
    }, // Unauthorized
    { path: "/*", component: NotFoundView }, // Not found; Will catch all other paths not covered previously
];

const router = new VueRouter({
    mode: "history",
    routes,
});

router.beforeEach(async (to, from, next) => {
    logger.debug(
        `from.fullPath: ${JSON.stringify(
            from.fullPath
        )}; to.fullPath: ${JSON.stringify(to.fullPath)}`
    );
    if (to.meta.requiresAuth || to.meta.requiresRegistration) {
        store.dispatch("auth/oidcCheckAccess", to).then((isAuthorized) => {
            if (!isAuthorized) {
                next({ path: "/login", query: { redirect: to.fullPath } });
            } else {
                handleUserIsAuthorized(to, from, next);
            }
        });
    } else {
        const userIsAuthenticated: boolean =
            store.getters["auth/oidcIsAuthenticated"];
        const userIsRegistered: boolean =
            store.getters["user/userIsRegistered"];

        // If the user is authenticated but not registered, the registration must be completed
        const isRegistrationPath =
            to.path.startsWith(REGISTRATION_PATH) ||
            to.path.startsWith(REGISTRATION_INFO_PATH);
        if (
            userIsAuthenticated &&
            !userIsRegistered &&
            !to.meta.routeIsOidcCallback &&
            !to.path.startsWith("/logout") &&
            !isRegistrationPath
        ) {
            next({ path: REGISTRATION_PATH });
        } else {
            next();
        }
    }
});

router.afterEach((to, from) => {
    window.snowplow("trackPageView");
});

function handleUserIsAuthorized(to: Route, from: Route, next: any) {
    const userIsRegistered: boolean = store.getters["user/userIsRegistered"];
    const userIsActive: boolean = store.getters["user/userIsActive"];

    // If the user is registerd and is attempting to go to the registration flow pages, re-route to the timeline.
    if (userIsRegistered && to.path.startsWith(REGISTRATION_PATH)) {
        next({ path: "/timeline", replace: true });
    } else if (
        userIsRegistered &&
        !userIsActive &&
        !to.path.startsWith("/termsOfService") &&
        !to.path.startsWith("/profile")
    ) {
        next({ path: "/profile" });
    } else if (to.meta.requiresRegistration && !userIsRegistered) {
        next({ path: REGISTRATION_PATH });
    } else {
        next();
    }
}

export default router;
