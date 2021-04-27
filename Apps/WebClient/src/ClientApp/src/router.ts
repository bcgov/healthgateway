import VueRouter, {
    NavigationGuard,
    NavigationGuardNext,
    Route,
} from "vue-router";

import { Dictionary } from "@/models/baseTypes";
import { SnowplowWindow } from "@/plugins/extensions";
import { SERVICE_IDENTIFIER, STORE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IStoreProvider } from "@/services/interfaces";
const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
const storeWrapper: IStoreProvider = container.get(
    STORE_IDENTIFIER.StoreWrapper
);
const store = storeWrapper.getStore();
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
const LogoutCompleteView = () =>
    import(/* webpackChunkName: "logout" */ "@/views/logoutComplete.vue");
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
const DependentsView = () =>
    import(/* webpackChunkName: "dependents" */ "@/views/dependents.vue");
const FAQView = () => import(/* webpackChunkName: "faq" */ "@/views/faq.vue");

enum UserState {
    unauthenticated = "unauthenticated",
    notRegistered = "notRegistered",
    registered = "registered",
    pendingDeletion = "pendingDeletion",
    invalidLogin = "invalidLogin",
    offline = "offline",
}

function calculateUserState() {
    const isOffline = store.getters["config/isOffline"];
    const isAuthenticated: boolean = store.getters["auth/oidcIsAuthenticated"];
    const isValid: boolean = store.getters["auth/isValidIdentityProvider"];
    const isRegistered: boolean = store.getters["user/userIsRegistered"];
    const userIsActive: boolean = store.getters["user/userIsActive"];

    if (isOffline) {
        return UserState.offline;
    } else if (!isAuthenticated) {
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

export enum ClientModule {
    Immunization = "Immunization",
    Medication = "Medication",
    Laboratory = "Laboratory",
    Encounter = "Encounter",
    Comment = "Comment",
    CovidLabResults = "CovidLabResults",
    Dependent = "Dependent",
    Note = "Note",
}

function getAvailableModules() {
    const availableModules: string[] = [];
    const configModules: Dictionary<boolean> =
        store.getters["config/webClient"].modules;

    for (const moduleName in configModules) {
        if (configModules[moduleName]) {
            availableModules.push(moduleName);
        }
    }
    return availableModules;
}

const REGISTRATION_PATH = "/registration";
const REGISTRATION_INFO_PATH = "/registrationInfo";

const routes = [
    {
        path: "/",
        component: LandingView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.offline,
            ],
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
        path: "/dependents",
        component: DependentsView,
        meta: {
            validStates: [UserState.registered],
            requiredModules: [ClientModule.Dependent],
        },
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
        path: "/faq",
        component: FAQView,
        meta: { stateless: true },
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
        path: "/logoutComplete",
        component: LogoutCompleteView,
        meta: { stateless: true },
    },
    {
        path: "/idirLoggedIn",
        component: IdirLoggedInView,
        meta: { validStates: [UserState.invalidLogin] },
    },
    {
        path: "/unauthorized",
        component: UnauthorizedView,
        meta: { stateless: true },
    },
    {
        path: "/*",
        component: NotFoundView,
        meta: {
            stateless: true,
        },
    }, // Not found; Will catch all other paths not covered previously
];

export const beforeEachGuard: NavigationGuard = (
    to: Route,
    from: Route,
    next: NavigationGuardNext<Vue>
) => {
    to.matched[0].components.default.name;

    logger.debug(
        `from.fullPath: ${JSON.stringify(
            from.fullPath
        )}; to.fullPath: ${JSON.stringify(to.fullPath)}`
    );

    if (to.meta.routeIsOidcCallback || to.meta.stateless) {
        next();
    } else {
        // Make sure that the route accepts the current state
        store.dispatch("auth/oidcCheckUser").then((isValid: boolean) => {
            logger.info("User is valid: " + isValid);

            const currentUserState = calculateUserState();
            logger.debug(`current state: ${currentUserState}`);
            const isValidState = to.meta.validStates.includes(currentUserState);
            const availableModules = getAvailableModules();
            const hasRequiredModules =
                to.meta.requiredModules === undefined
                    ? true
                    : to.meta.requiredModules.every((val: string) =>
                          availableModules.includes(val)
                      );
            if (isValidState && hasRequiredModules) {
                next();
            } else {
                // If the route does not accept the state, go to one of the default locations
                if (currentUserState === UserState.offline) {
                    next({ path: "/" });
                } else if (currentUserState === UserState.pendingDeletion) {
                    next({ path: "/profile" });
                } else if (currentUserState === UserState.registered) {
                    if (hasRequiredModules) {
                        next({ path: "/timeline" });
                    } else {
                        next({ path: "/unauthorized" });
                    }
                } else if (currentUserState === UserState.notRegistered) {
                    next({ path: REGISTRATION_PATH });
                } else if (currentUserState === UserState.invalidLogin) {
                    next({ path: "/idirLoggedIn" });
                } else if (currentUserState === UserState.unauthenticated) {
                    next({ path: "/login", query: { redirect: to.path } });
                } else {
                    next({ path: "/unauthorized" });
                }
            }
        });
    }
};

const router = new VueRouter({
    mode: "history",
    routes,
});

router.beforeEach(beforeEachGuard);

router.afterEach(() => {
    window.snowplow("trackPageView");
});

export default router;
