import VueRouter, {
    NavigationGuard,
    NavigationGuardNext,
    Route,
} from "vue-router";
import { Position, PositionResult } from "vue-router/types/router";

import { ClientModule } from "@/constants/clientModule";
import { TicketStatus } from "@/constants/ticketStatus";
import { Dictionary } from "@/models/baseTypes";
import { WebClientConfiguration } from "@/models/configData";
import { Ticket } from "@/models/ticket";
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
const PatientRetrievalErrorView = () =>
    import(
        /* webpackChunkName: "patientRetrievalError" */ "@/views/errors/PatientRetrievalErrorView.vue"
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
const QueueView = () =>
    import(/* webpackChunkName: "queue" */ "@/views/waitlist/QueueView.vue");
const QueueFullView = () =>
    import(
        /* webpackChunkName: "queueFull" */ "@/views/waitlist/QueueFullView.vue"
    );

export enum UserState {
    offline = "offline",
    unauthenticated = "unauthenticated",
    invalidIdentityProvider = "invalidIdentityProvider",
    noPatientData = "noPatientData",
    notRegistered = "notRegistered",
    pendingDeletion = "pendingDeletion",
    acceptTermsOfService = "acceptTermsOfService",
    registered = "registered",
}

function calculateUserState(): UserState {
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
    const isOffline = store.getters["config/isOffline"];
    const isAuthenticated: boolean = store.getters["auth/oidcIsAuthenticated"];
    const isValidIdentityProvider: boolean =
        store.getters["user/isValidIdentityProvider"];
    const patientRetrievalFailed: boolean =
        store.getters["user/patientRetrievalFailed"];
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
    } else if (patientRetrievalFailed) {
        return UserState.noPatientData;
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

function getEnabledModules(): string[] {
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
    const webClientConfig: WebClientConfiguration =
        store.getters["config/webClient"];
    const configModules: Dictionary<boolean> = webClientConfig?.modules ?? [];
    const availableModules: string[] = [];

    for (const moduleName in configModules) {
        if (configModules[moduleName]) {
            availableModules.push(moduleName);
        }
    }
    return availableModules;
}

const ACCEPT_TERMS_OF_SERVICE_PATH = "/acceptTermsOfService";
const HOME_PATH = "/home";
const IDIR_LOGGED_IN_PATH = "/idirLoggedIn";
const LOGIN_PATH = "/login";
const PATIENT_RETRIEVAL_ERROR_PATH = "/patientRetrievalError";
const PROFILE_PATH = "/profile";
const REGISTRATION_PATH = "/registration";
const ROOT_PATH = "/";
const TIMELINE_PATH = "/timeline";
const UNAUTHORIZED_PATH = "/unauthorized";
const QUEUE_PATH = "/queue";
const QUEUE_FULL_PATH = "/busy";

const routes = [
    {
        path: ROOT_PATH,
        component: LandingView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.offline,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: QUEUE_PATH,
        component: QueueView,
        meta: {
            stateless: true,
        },
    },
    {
        path: QUEUE_FULL_PATH,
        component: QueueFullView,
        meta: {
            stateless: true,
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
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: ACCEPT_TERMS_OF_SERVICE_PATH,
        component: AcceptTermsOfServiceView,
        meta: {
            validStates: [UserState.acceptTermsOfService],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/validateEmail/:inviteKey",
        component: ValidateEmailView,
        props: true,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
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
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/home",
        component: HomeView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: TIMELINE_PATH,
        component: TimelineView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/covid19",
        component: Covid19View,
        meta: {
            validStates: [UserState.registered],
            requiredModules: [ClientModule.VaccinationStatus],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/reports",
        component: ReportsView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/dependents",
        component: DependentsView,
        meta: {
            validStates: [UserState.registered],
            requiredModules: [ClientModule.Dependent],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/covidtest",
        component: PublicCovidTestView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.PublicLaboratoryResult],
            requiresProcessedWaitlistTicket: true,
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
            requiresProcessedWaitlistTicket: true,
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
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/vaccinecard",
        component: PublicVaccineCardView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiredModules: [ClientModule.VaccinationStatus],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/termsOfService",
        component: TermsOfServiceView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: "/release-notes",
        component: ReleaseNotesView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: "/contact-us",
        component: ContactUsView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: "/faq",
        component: FAQView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatientData,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: LOGIN_PATH,
        component: LoginView,
        props: (route: Route) => ({
            isRetry: route.query.isRetry === "true",
        }),
        meta: {
            validStates: [UserState.unauthenticated],
            requiresProcessedWaitlistTicket: true,
        },
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
        meta: {
            validStates: [UserState.invalidIdentityProvider],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: PATIENT_RETRIEVAL_ERROR_PATH,
        component: PatientRetrievalErrorView,
        meta: {
            validStates: [UserState.noPatientData],
            requiresProcessedWaitlistTicket: true,
        },
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

    const enabledModules = getEnabledModules();

    const waitlistIsEnabled = enabledModules.includes(ClientModule.Ticket);
    const isAuthenticated: boolean = store.getters["auth/oidcIsAuthenticated"];
    const isFromBusy = from.fullPath.includes(QUEUE_FULL_PATH);
    let metaRequiresProcessedWaitlistTicket =
        meta.requiresProcessedWaitlistTicket;

    if (from.fullPath === QUEUE_FULL_PATH || from.fullPath === QUEUE_PATH) {
        metaRequiresProcessedWaitlistTicket = true;
    }

    logger.debug(
        `Before guard - user is authenticated: ${isAuthenticated}, waitlist enabled: ${waitlistIsEnabled}, meta requires processed waitlist ticket: ${metaRequiresProcessedWaitlistTicket} and is from busy: ${isFromBusy}`
    );

    if (waitlistIsEnabled && metaRequiresProcessedWaitlistTicket) {
        redirectWhenTicketIsInvalid(to, next);
    }

    await store.dispatch("auth/checkStatus");

    // Make sure that the route accepts the current state
    const currentUserState = calculateUserState();
    logger.debug(`current state: ${currentUserState}`);

    const isValidState = meta.validStates.includes(currentUserState);
    const hasRequiredModules =
        meta.requiredModules === undefined ||
        meta.requiredModules.every((val: string) =>
            enabledModules.includes(val)
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

async function redirectWhenTicketIsInvalid(
    to: Route,
    next: NavigationGuardNext<Vue>
) {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();
    let ticket = store.getters["waitlist/ticket"];

    try {
        if (ticketIsValid(ticket)) {
            logger.debug(`Router -check existing ticket`);
            const timeoutId = store.getters["waitlist/checkInTimeoutId"];
            clearTimeout(timeoutId);

            const now = new Date().getTime();
            const checkInAfter = ticket.checkInAfter * 1000;
            const timeout = Math.max(0, checkInAfter - now);
            // TODO: determine how to deal with already-queued timeouts
            setTimeout(() => {
                store
                    .dispatch("waitlist/checkIn")
                    .catch(() => logger.error(`Error calling checkIn action.`));
            }, timeout);
        } else {
            logger.debug(`Router - get new ticket`);
            ticket = await store.dispatch("waitlist/getTicket");

            if (!ticketIsValid(ticket)) {
                // redirect to queued page if new ticket is not valid
                next({ path: QUEUE_PATH, query: { redirect: to.path } });
                return;
            }
        }
    } catch {
        // redirect to busy page if new ticket could not be retrieved
        next({ path: QUEUE_FULL_PATH, query: { redirect: to.path } });
        return;
    }
}

function ticketIsValid(ticket: Ticket | undefined): boolean {
    if (ticket?.status !== TicketStatus.Processed) {
        return false;
    }

    const now = new Date().getTime();
    return now < ticket.tokenExpires * 1000;
}

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
        case UserState.noPatientData:
            return PATIENT_RETRIEVAL_ERROR_PATH;
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

router.afterEach(() => {
    const storeWrapper = container.get<IStoreProvider>(
        STORE_IDENTIFIER.StoreProvider
    );
    const store = storeWrapper.getStore();

    store.dispatch("errorBanner/clearErrors");
    store.dispatch("errorBanner/clearTooManyRequestsWarning");
    store.dispatch("errorBanner/clearTooManyRequestsError");

    window.snowplow("trackPageView");
});

export default router;
