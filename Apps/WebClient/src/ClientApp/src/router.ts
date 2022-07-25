import VueRouter, {
    NavigationGuard,
    NavigationGuardNext,
    Route,
} from "vue-router";
import { Position, PositionResult } from "vue-router/types/router";

import { ClientModule } from "@/constants/clientModule";
import { Dictionary } from "@/models/baseTypes";
import container from "@/plugins/container";
import { SnowplowWindow } from "@/plugins/extensions";
import { SERVICE_IDENTIFIER, STORE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IStoreProvider } from "@/services/interfaces";

declare let window: SnowplowWindow;
const ProfileView = () =>
    import(/* webpackChunkName: "profile" */ "@/views/ProfileView.vue");
const LandingView = () =>
    import(/* webpackChunkName: "landing" */ "@/views/LandingView.vue");
const PublicCovidTestView = () =>
    import(
        /* webpackChunkName: "covidTest" */ "@/views/PublicCovidTestView.vue"
    );
const PublicVaccineCardView = () =>
    import(
        /* webpackChunkName: "vaccinationStatus" */ "@/views/PublicVaccineCardView.vue"
    );
const NotFoundView = () =>
    import(
        /* webpackChunkName: "notFound" */ "@/views/errors/NotFoundView.vue"
    );
const LoginView = () =>
    import(/* webpackChunkName: "login" */ "@/views/LoginView.vue");
const LogoutView = () =>
    import(/* webpackChunkName: "logout" */ "@/views/LogoutView.vue");
const LogoutCompleteView = () =>
    import(/* webpackChunkName: "logout" */ "@/views/LogoutCompleteView.vue");
const UnauthorizedView = () =>
    import(
        /* webpackChunkName: "unauthorized" */ "@/views/errors/UnauthorizedView.vue"
    );
const IdirLoggedInView = () =>
    import(
        /* webpackChunkName: "idirLoggedIn" */ "@/views/errors/IdirLoggedInView.vue"
    );
const LoginCallbackView = () =>
    import(
        /* webpackChunkName: "loginCallback" */ "@/views/LoginCallbackView.vue"
    );
const RegistrationView = () =>
    import(
        /* webpackChunkName: "registration" */ "@/views/RegistrationView.vue"
    );
const HomeView = () =>
    import(/* webpackChunkName: "home" */ "@/views/HomeView.vue");
const TimelineView = () =>
    import(/* webpackChunkName: "timeline" */ "@/views/TimelineView.vue");
const Covid19View = () =>
    import(/* webpackChunkName: "covid19" */ "@/views/Covid19View.vue");
const ValidateEmailView = () =>
    import(
        /* webpackChunkName: "validateEmail" */ "@/views/ValidateEmailView.vue"
    );
const TermsOfServiceView = () =>
    import(
        /* webpackChunkName: "termsOfService" */ "@/views/TermsOfServiceView.vue"
    );
const AcceptTermsOfServiceView = () =>
    import(
        /* webpackChunkName: "acceptTermsOfService" */ "@/views/AcceptTermsOfServiceView.vue"
    );
const ReportsView = () =>
    import(/* webpackChunkName: "reports" */ "@/views/ReportsView.vue");
const ReleaseNotesView = () =>
    import(
        /* webpackChunkName: "releaseNotes" */ "@/views/ReleaseNotesView.vue"
    );
const ContactUsView = () =>
    import(/* webpackChunkName: "contactUs" */ "@/views/ContactUsView.vue");
const DependentsView = () =>
    import(/* webpackChunkName: "dependents" */ "@/views/DependentsView.vue");
const FAQView = () =>
    import(/* webpackChunkName: "faq" */ "@/views/FaqView.vue");
const PcrTestView = () =>
    import(/* webpackChunkName: "pcrTest" */ "@/views/PcrTestView.vue");

export enum UserState {
    unauthenticated = "unauthenticated",
    notRegistered = "notRegistered",
    registered = "registered",
    pendingDeletion = "pendingDeletion",
    invalidIdentityProvider = "invalidIdentityProvider",
    offline = "offline",
    acceptTermsOfService = "acceptTermsOfService",
}

function calculateUserState(): UserState {
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
    const isOffline = store.getters["config/isOffline"];
    const isAuthenticated: boolean = store.getters["auth/oidcIsAuthenticated"];
    const isValidIdentityProvider: boolean =
        store.getters["auth/isValidIdentityProvider"];
    const isRegistered: boolean = store.getters["user/userIsRegistered"];
    const userIsActive: boolean = store.getters["user/userIsActive"];
    const hasTermsOfServiceUpdated: boolean =
        store.getters["user/hasTermsOfServiceUpdated"];

    if (isOffline) {
        return UserState.offline;
    } else if (!isAuthenticated) {
        return UserState.unauthenticated;
    } else if (!isValidIdentityProvider) {
        return UserState.invalidIdentityProvider;
    } else if (!isRegistered) {
        return UserState.notRegistered;
    } else if (!userIsActive) {
        return UserState.pendingDeletion;
    } else if (hasTermsOfServiceUpdated) {
        return UserState.acceptTermsOfService;
    } else {
        return UserState.registered;
    }
}

function getAvailableModules(): string[] {
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
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

const HOME_PATH = "/home";
const IDIR_LOGGED_IN_PATH = "/idirLoggedIn";
const LOGIN_PATH = "/login";
const PROFILE_PATH = "/profile";
const REGISTRATION_PATH = "/registration";
const ROOT_PATH = "/";
const TIMELINE_PATH = "/timeline";
const UNAUTHORIZED_PATH = "/unauthorized";
const ACCEPT_TERMS_OF_SERVICE_PATH = "/acceptTermsOfService";

const routes = [
    {
        path: ROOT_PATH,
        component: LandingView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.offline,
            ],
        },
    },
    {
        path: REGISTRATION_PATH,
        component: RegistrationView,
        props: (route: Route) => ({
            inviteKey: route.query.inviteKey,
            inviteEmail: route.query.email,
        }),
        meta: {
            validStates: [UserState.notRegistered],
        },
    },
    {
        path: ACCEPT_TERMS_OF_SERVICE_PATH,
        component: AcceptTermsOfServiceView,
        meta: {
            validStates: [UserState.acceptTermsOfService],
        },
    },
    {
        path: "/validateEmail/:inviteKey",
        component: ValidateEmailView,
        props: true,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: PROFILE_PATH,
        component: ProfileView,
        meta: {
            validStates: [
                UserState.registered,
                UserState.pendingDeletion,
                UserState.acceptTermsOfService,
            ],
        },
    },
    {
        path: "/home",
        component: HomeView,
        meta: {
            validStates: [UserState.registered],
        },
    },
    {
        path: TIMELINE_PATH,
        component: TimelineView,
        meta: { validStates: [UserState.registered] },
    },
    {
        path: "/covid19",
        component: Covid19View,
        meta: {
            validStates: [UserState.registered],
            requiredModules: [ClientModule.VaccinationStatus],
        },
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
        path: "/covidtest",
        component: PublicCovidTestView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.PublicLaboratoryResult],
        },
    },
    {
        path: "/pcrtest",
        component: PcrTestView,
        props: false,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.notRegistered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.PcrTest],
        },
    },
    {
        path: "/pcrtest/:serialNumber",
        component: PcrTestView,
        props: true,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.notRegistered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.PcrTest],
        },
    },
    {
        path: "/vaccinecard",
        component: PublicVaccineCardView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.VaccinationStatus],
        },
    },
    {
        path: "/termsOfService",
        component: TermsOfServiceView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
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
                UserState.invalidIdentityProvider,
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
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.pendingDeletion,
            ],
        },
    },
    {
        path: "/faq",
        component: FAQView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.registered,
                UserState.pendingDeletion,
            ],
        },
    },
    {
        path: LOGIN_PATH,
        component: LoginView,
        props: (route: Route) => ({
            isRetry: route.query.isRetry === "true",
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
        path: IDIR_LOGGED_IN_PATH,
        component: IdirLoggedInView,
        meta: { validStates: [UserState.invalidIdentityProvider] },
    },
    {
        path: UNAUTHORIZED_PATH,
        component: UnauthorizedView,
        meta: { stateless: true },
    },
    {
        path: "/not-found",
        component: NotFoundView,
        meta: { stateless: true },
    },
    {
        path: "/*",
        redirect: "/not-found",
    }, // Not found; Will catch all other paths not covered previously
];

export const beforeEachGuard: NavigationGuard = async (
    to: Route,
    from: Route,
    next: NavigationGuardNext<Vue>
) => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
    logger.debug(
        `from.fullPath: ${JSON.stringify(
            from.fullPath
        )}; to.fullPath: ${JSON.stringify(to.fullPath)}`
    );

    const meta = to.meta;
    if (meta === undefined) {
        next(Error("Route meta property is undefined"));
        return;
    }

    if (meta.routeIsOidcCallback || meta.stateless) {
        next();
        return;
    }

    await store.dispatch("auth/checkStatus");

    // Make sure that the route accepts the current state
    const currentUserState = calculateUserState();
    logger.debug(`current state: ${currentUserState}`);

    const isValidState = meta.validStates.includes(currentUserState);
    const availableModules = getAvailableModules();
    const hasRequiredModules =
        meta.requiredModules === undefined ||
        meta.requiredModules.every((val: string) =>
            availableModules.includes(val)
        );

    if (isValidState && hasRequiredModules) {
        next();
        return;
    }

    // If the route does not accept the state, go to one of the default locations
    const defaultPath = getDefaultPath(currentUserState, hasRequiredModules);

    if (defaultPath === LOGIN_PATH) {
        next({ path: defaultPath, query: { redirect: to.path } });
        return;
    }

    next({ path: defaultPath });
};

function getDefaultPath(
    currentUserState: UserState,
    hasRequiredModules: boolean
): string {
    switch (currentUserState) {
        case UserState.offline:
            return ROOT_PATH;
        case UserState.pendingDeletion:
            return PROFILE_PATH;
        case UserState.registered:
            return hasRequiredModules ? HOME_PATH : UNAUTHORIZED_PATH;
        case UserState.notRegistered:
            return REGISTRATION_PATH;
        case UserState.invalidIdentityProvider:
            return IDIR_LOGGED_IN_PATH;
        case UserState.unauthenticated:
            return hasRequiredModules ? LOGIN_PATH : UNAUTHORIZED_PATH;
        case UserState.acceptTermsOfService:
            return ACCEPT_TERMS_OF_SERVICE_PATH;
        default:
            return UNAUTHORIZED_PATH;
    }
}

function scrollBehaviour(
    _to: Route,
    _from: Route,
    savedPosition: void | Position
): PositionResult {
    if (savedPosition) {
        return savedPosition;
    } else {
        return { x: 0, y: 0 };
    }
}

const router = new VueRouter({
    mode: "history",
    routes,
    scrollBehavior: scrollBehaviour,
});

router.beforeEach(beforeEachGuard);

router.afterEach(() => window.snowplow("trackPageView"));

export default router;
