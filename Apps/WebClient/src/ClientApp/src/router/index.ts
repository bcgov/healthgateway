import {
    createRouter,
    createWebHistory,
    RouteLocationNormalized,
} from "vue-router";

import { Path } from "@/constants/path";
import { FeatureToggleConfiguration } from "@/models/configData";
import { afterEachHook } from "@/router/afterEachHook";
import { beforeEachGuard } from "@/router/beforeEachGuard";

const Covid19View = () =>
    import(
        /* webpackChunkName: "covid19" */ "@/components/private/covid19/Covid19View.vue"
    );
const DependentTimelineView = () =>
    import(
        /* webpackChunkName: "dependents" */ "@/components/private/dependent/DependentTimelineView.vue"
    );
const DependentViewSelectorComponent = () =>
    import(
        /* webpackChunkName: "dependents" */ "@/components/private/dependent/DependentViewSelectorComponent.vue"
    );
const LandingView = () =>
    import(
        /* webpackChunkName: "landing" */ "@/components/public/landing/LandingView.vue"
    );
const LoginView = () =>
    import(
        /* webpackChunkName: "login" */ "@/components/authentication/LoginView.vue"
    );
const LoginCallbackView = () =>
    import(
        /* webpackChunkName: "loginCallback" */ "@/components/authentication/LoginCallbackView.vue"
    );
const ProfileView = () =>
    import(
        /* webpackChunkName: profile" */ "@/components/private/profile/ProfileView.vue"
    );
const PublicVaccineCardView = () =>
    import(
        /* webpackChunkName: "vaccinationStatus" */ "@/components/public/vaccine-card/VaccineCardView.vue"
    );
const RegistrationView = () =>
    import(
        /* webpackChunkName: "registration" */ "@/components/private/registration/RegistrationView.vue"
    );
const HomeView = () =>
    import(
        /* webpackChunkName: "home" */ "@/components/private/home/HomeView.vue"
    );
const IdirLoggedInView = () =>
    import(
        /* webpackChunkName: "idirLoggedIn" */ "@/components/error/IdirLoggedInView.vue"
    );
const LogoutCompleteView = () =>
    import(
        /* webpackChunkName: "logoutComplete" */ "@/components/authentication/LogoutCompleteView.vue"
    );
const LogoutView = () =>
    import(
        /* webpackChunkName: "logout" */ "@/components/authentication/LogoutView.vue"
    );
const NotFoundView = () =>
    import(
        /* webpackChunkName: "notFound" */ "@/components/error/NotFoundView.vue"
    );
const UnauthorizedView = () =>
    import(
        /* webpackChunkName: "unauthorized" */ "@/components/error/UnauthorizedView.vue"
    );
const PatientRetrievalErrorView = () =>
    import(
        /* webpackChunkName: "patientRetrievalError" */ "@/components/error/PatientRetrievalErrorView.vue"
    );
const ReportsView = () =>
    import(
        /* webpackChunkName: "reports" */ "@/components/private/reports/ReportsView.vue"
    );
const ServicesView = () =>
    import(
        /* webpackChunkName: "services" */ "@/components/private/services/ServicesView.vue"
    );
const UserTimelineView = () =>
    import(
        /* webpackChunkName: "timeline" */ "@/components/private/timeline/UserTimelineView.vue"
    );
const TermsOfServiceView = () =>
    import(
        /* webpackChunkName: "termsOfService" */ "@/components/public/terms-of-service/TermsOfServiceView.vue"
    );
const AcceptTermsOfServiceView = () =>
    import(
        /* webpackChunkName: "acceptTermsOfService" */ "@/components/private/accept-terms-of-service/AcceptTermsOfServiceView.vue"
    );
const ValidateEmailView = () =>
    import(
        /* webpackChunkName: "validateEmail" */ "@/components/private/validate-email/ValidateEmailView.vue"
    );
const QueueView = () =>
    import(
        /* webpackChunkName: "queue" */ "@/components/public/waitlist/QueueView.vue"
    );
const QueueFullView = () =>
    import(
        /* webpackChunkName: "queueFull" */ "@/components/public/waitlist/QueueFullView.vue"
    );
const PcrTestKitRegistrationView = () =>
    import(
        /* webpackChunkName: "pcrTest" */ "@/components/public/pcr-test-kit-registration/PcrTestKitRegistrationView.vue"
    );
const SharingView = () =>
    import(
        /* webpackChunkName: "sharing" */ "@/components/private/sharing/SharingView.vue"
    );

export enum UserState {
    offline = "offline",
    unauthenticated = "unauthenticated",
    invalidIdentityProvider = "invalidIdentityProvider",
    noPatient = "noPatient",
    notRegistered = "notRegistered",
    pendingDeletion = "pendingDeletion",
    acceptTermsOfService = "acceptTermsOfService",
    registered = "registered",
}

const routes = [
    {
        path: Path.Root,
        name: "Landing",
        component: LandingView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatient,
                UserState.registered,
                UserState.offline,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: Path.AcceptTermsOfService,
        component: AcceptTermsOfServiceView,
        meta: {
            validStates: [UserState.acceptTermsOfService],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Busy,
        component: QueueFullView,
        meta: {
            stateless: true,
        },
    },
    {
        path: Path.Covid19,
        component: Covid19View,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Dependents,
        component: DependentViewSelectorComponent,
        meta: {
            validStates: [UserState.registered],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.dependents.enabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Dependents + "/:id",
        redirect: "/dependents/:id/timeline",
    },
    {
        path: Path.Dependents + "/:id/timeline",
        component: DependentTimelineView,
        props: true,
        meta: {
            validStates: [UserState.registered],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.dependents.enabled && config.dependents.timelineEnabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Home,
        name: "Home",
        component: HomeView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.IdirLoggedIn,
        component: IdirLoggedInView,
        meta: {
            validStates: [UserState.invalidIdentityProvider],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Login,
        name: "Login",
        component: LoginView,
        props: (route: { query: { isRetry: string } }) => ({
            isRetry: route.query.isRetry === "true",
        }),
        meta: {
            validStates: [UserState.unauthenticated],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.LoginCallback,
        name: "LoginCallback",
        component: LoginCallbackView,
        meta: {
            routeIsOidcCallback: true,
            stateless: true,
        },
    },
    {
        path: Path.LogoutComplete,
        name: "LogoutComplete",
        component: LogoutCompleteView,
        meta: { stateless: true },
    },
    {
        path: Path.Logout,
        name: "Logout",
        component: LogoutView,
        meta: { stateless: true },
    },
    {
        path: Path.NotFound,
        component: NotFoundView,
        meta: { stateless: true },
    },
    {
        path: Path.PatientRetrievalError,
        component: PatientRetrievalErrorView,
        meta: {
            validStates: [UserState.noPatient],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.PcrTestKitRegistration,
        component: PcrTestKitRegistrationView,
        props: false,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.notRegistered,
                UserState.pendingDeletion,
            ],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.covid19.pcrTestEnabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.PcrTestKitRegistration + "/:serialNumber",
        component: PcrTestKitRegistrationView,
        props: true,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.registered,
                UserState.notRegistered,
                UserState.pendingDeletion,
            ],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.covid19.pcrTestEnabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Profile,
        name: "Profile",
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
        path: Path.Queue,
        component: QueueView,
        meta: {
            stateless: true,
        },
    },
    {
        path: Path.Registration,
        name: "Registration",
        component: RegistrationView,
        meta: {
            validStates: [UserState.notRegistered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Reports,
        component: ReportsView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Sharing,
        name: "Sharing",
        component: SharingView,
        meta: {
            validStates: [UserState.registered],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.sharing.enabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Services,
        name: "Services",
        component: ServicesView,
        meta: {
            validStates: [UserState.registered],
            requiredFeaturesEnabled: (config: FeatureToggleConfiguration) =>
                config.services.enabled,
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.TermsOfService,
        component: TermsOfServiceView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatient,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: false,
        },
    },
    {
        path: Path.Timeline,
        component: UserTimelineView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Unauthorized,
        component: UnauthorizedView,
        meta: { stateless: true },
    },
    {
        path: Path.VaccineCard,
        component: PublicVaccineCardView,
        meta: {
            validStates: [
                UserState.unauthenticated,
                UserState.invalidIdentityProvider,
                UserState.noPatient,
                UserState.registered,
                UserState.pendingDeletion,
            ],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.ValidateEmail + "/:inviteKey",
        component: ValidateEmailView,
        props: true,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/:pathMatch(.*)*", // will catch all other paths not covered previously
        redirect: Path.NotFound,
        meta: {
            stateless: true,
        },
    },
];

function scrollBehaviour(
    _to: RouteLocationNormalized,
    _from: RouteLocationNormalized,
    savedPosition: any
) {
    if (savedPosition) {
        return savedPosition;
    } else {
        return { x: 0, y: 0 };
    }
}

function initializeRouter() {
    const router = createRouter({
        history: createWebHistory(process.env.BASE_URL),
        routes,
        scrollBehavior: scrollBehaviour,
    });

    router.beforeEach(beforeEachGuard);
    router.afterEach(afterEachHook);
    return router;
}

export default initializeRouter;
