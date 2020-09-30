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
const IdirLoggedInView = () =>
    import(
        /* webpackChunkName: "idirLoggedIn" */ "@/views/errors/idirLoggedIn.vue"
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
const ReportsView = () =>
    import(/* webpackChunkName: "reports" */ "@/views/reports.vue");
const ReleaseNotesView = () =>
    import(/* webpackChunkName: "releaseNotes" */ "@/views/releaseNotes.vue");
const ContactUsView = () =>
    import(/* webpackChunkName: "contactUs" */ "@/views/contactUs.vue");

Vue.use(VueRouter);

enum UserState {
    unauthenticated = "unauthenticated",
    notRegistered = "notRegistered",
    registered = "registered",
    pendingDeletion = "pendingDeletion",
    invalidLogin = "invalidLogin",
}

function calculateUserState() {
    let isAuthenticated: boolean = store.getters["auth/oidcIsAuthenticated"];
    let isValid: boolean = store.getters["auth/isValidIdentityProvider"];
    let isRegistered: boolean = store.getters["user/userIsRegistered"];
    let userIsActive: boolean = store.getters["user/userIsActive"];

    if (!isAuthenticated) {
        return UserState.unauthenticated;
    } else if (!isValid) {
        return UserState.invalidLogin;
    } else if (!isRegistered) {
        return UserState.notRegistered;
    } else if (!userIsActive) {
        return UserState.pendingDeletion;
    } else {
        return UserState.registered;
    }
}

const REGISTRATION_PATH = "/registration";
const REGISTRATION_INFO_PATH = "/registrationInfo";

const routes = [
    {
        path: "/",
        component: LandingView,
        meta: {
            validStates: [UserState.unauthenticated, UserState.registered],
        },
    },
    {
        path: REGISTRATION_INFO_PATH,
        component: RegistrationInfoView,
        props: (route: Route) => ({
            inviteKey: route.query.inviteKey,
            email: route.query.email,
        }),
        meta: {
            validStates: [UserState.unauthenticated, UserState.notRegistered],
        },
    },
    {
        path: REGISTRATION_PATH,
        component: RegistrationView,
        props: (route: Route) => ({
            inviteKey: route.query.inviteKey,
            inviteEmail: route.query.email,
        }),
        meta: { validStates: [UserState.notRegistered] },
    },
    {
        path: "/validateEmail/:inviteKey",
        component: ValidateEmailView,
        props: true,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: "/profile",
        component: ProfileView,
        meta: {
            validStates: [UserState.registered, UserState.pendingDeletion],
        },
    },
    {
        path: "/timeline",
        component: TimelineView,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: "/healthInsights",
        component: HealthInsightsView,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: "/reports",
        component: ReportsView,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: "/termsOfService",
        component: TermsOfServiceView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.pendingDeletion,
            ],
        },
    },
    {
        path: "/release-notes",
        component: ReleaseNotesView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.pendingDeletion,
            ],
        },
    },
    {
        path: "/contact-us",
        component: ContactUsView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.pendingDeletion,
            ],
        },
    },
    {
        path: "/login",
        component: LoginView,
        props: (route: Route) => ({
            isRetry: route.query.isRetry,
        }),
        meta: { validStates: [UserState.unauthenticated] },
    },
    {
        path: "/loginCallback",
        component: LoginCallbackView,
        meta: {
            routeIsOidcCallback: true,
            stateless: true,
        },
    },
    {
        path: "/logout",
        component: LogoutView,
        meta: { stateless: true },
    },
    {
        path: "/idirLoggedIn",
        component: IdirLoggedInView,
        meta: { validStates: [UserState.invalidLogin] },
    }, // IDIR Logged In warning
    {
        path: "/unauthorized",
        component: UnauthorizedView,
        meta: { stateless: true },
    }, // Unauthorized

    {
        path: "/*",
        component: NotFoundView,
        meta: {
            stateless: true,
        },
    }, // Not found; Will catch all other paths not covered previously
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
    if (to.meta.routeIsOidcCallback || to.meta.stateless) {
        next();
    } else {
        // Make sure that the route accepts the current state
        let currentUserState = calculateUserState();
        logger.debug(`current state: ${currentUserState}`);
        if (to.meta.validStates.includes(currentUserState)) {
            next();
        } else {
            // If the route does not accept the state, go to one of the default locations
            if (currentUserState === UserState.pendingDeletion) {
                next({ path: "/profile" });
            } else if (currentUserState === UserState.registered) {
                next({ path: "/timeline" });
            } else if (currentUserState === UserState.notRegistered) {
                next({ path: REGISTRATION_PATH });
            } else if (currentUserState === UserState.invalidLogin) {
                next({ path: "/idirLoggedIn" });
            } else if (currentUserState === UserState.unauthenticated) {
                next({ path: "/login" });
            } else {
                next({ path: "/unauthorized" });
            }
        }
    }
});

router.afterEach((to, from) => {
    window.snowplow("trackPageView");
});

export default router;
