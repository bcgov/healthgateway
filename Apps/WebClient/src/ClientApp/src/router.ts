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

declare let window: SnowplowWindow;
const ProfileView = () =>
    import(/* webpackChunkName: "profile" */ "@/views/profile.vue");
const LandingView = () =>
    import(/* webpackChunkName: "landing" */ "@/views/landing.vue");
const PublicCovidTestView = () =>
    import(/* webpackChunkName: "covidTest" */ "@/views/publicCovidTest.vue");
const PublicVaccineCardView = () =>
    import(
        /* webpackChunkName: "vaccinationStatus" */ "@/views/publicVaccineCard.vue"
    );
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
const HomeView = () =>
    import(/* webpackChunkName: "home" */ "@/views/home.vue");
const TimelineView = () =>
    import(/* webpackChunkName: "timeline" */ "@/views/timeline.vue");
const Covid19View = () =>
    import(/* webpackChunkName: "covid19" */ "@/views/covid19.vue");
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
const PcrTestView = () =>
    import(/* webpackChunkName: "pcrTest" */ "@/views/pcrTest.vue");

export enum UserState {
    unauthenticated = "unauthenticated",
    notRegistered = "notRegistered",
    registered = "registered",
    pendingDeletion = "pendingDeletion",
    invalidLogin = "invalidLogin",
    offline = "offline",
}

function calculateUserState() {
    const storeWrapper: IStoreProvider = container.get(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
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
    VaccinationStatus = "VaccinationStatus",
    VaccinationStatusPdf = "VaccinationStatusPdf",
    VaccinationExportPdf = "VaccinationExportPdf",
    FederalCardButton = "FederalCardButton",
    PublicLaboratoryResult = "PublicLaboratoryResult",
    AllLaboratory = "AllLaboratory",
    PcrTest = "PcrTest",
}

function getAvailableModules() {
    const storeWrapper: IStoreProvider = container.get(
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

const IDIR_LOGGED_IN_PATH = "/idirLoggedIn";
const LOGIN_PATH = "/login";
const PROFILE_PATH = "/profile";
const REGISTRATION_PATH = "/registration";
const REGISTRATION_INFO_PATH = "/registrationInfo";
const ROOT_PATH = "/";
const TIMELINE_PATH = "/timeline";
const UNAUTHORIZED_PATH = "/unauthorized";

const routes = [
    {
        path: ROOT_PATH,
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
        path: PROFILE_PATH,
        component: ProfileView,
        meta: {
            validStates: [UserState.registered, UserState.pendingDeletion],
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
        path: "/covidtest",
        component: PublicCovidTestView,
        meta: {
            validStates: [
                UserState.unauthenticated,
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
        meta: { validStates: [UserState.invalidLogin] },
    },
    {
        path: UNAUTHORIZED_PATH,
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
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    const storeWrapper: IStoreProvider = container.get(
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

    // Make sure that the route accepts the current state
    store.dispatch("auth/oidcCheckUser").then((isValid: boolean) => {
        logger.info("User is valid: " + isValid);

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
        const defaultPath = getDefaultPath(
            currentUserState,
            hasRequiredModules
        );

        if (defaultPath === LOGIN_PATH) {
            next({ path: defaultPath, query: { redirect: to.path } });
            return;
        }

        next({ path: defaultPath });
    });
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
            return hasRequiredModules ? TIMELINE_PATH : UNAUTHORIZED_PATH;
        case UserState.notRegistered:
            return REGISTRATION_PATH;
        case UserState.invalidLogin:
            return IDIR_LOGGED_IN_PATH;
        case UserState.unauthenticated:
            return hasRequiredModules ? LOGIN_PATH : UNAUTHORIZED_PATH;
        default:
            return UNAUTHORIZED_PATH;
    }
}

const router = new VueRouter({
    mode: "history",
    routes,
});

router.beforeEach(beforeEachGuard);

router.afterEach(() => {
    window.snowplow("trackPageView");
});

export default router;
