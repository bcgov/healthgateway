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
    import(/* webpackChunkName: "covid19" */ "@/views/Covid19View.vue");
const DependentViewSelectorComponent = () =>
    import(
        /* webpackChunkName: "dependents" */ "@/components/dependent/DependentViewSelectorComponent.vue"
    );
const LandingView = () =>
    import(/* webpackChunkName: "landing" */ "@/views/LandingView.vue");
const LoginView = () =>
    import(/* webpackChunkName: "login" */ "@/views/LoginView.vue");
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
const LogoutCompleteView = () =>
    import(
        /* webpackChunkName: "logoutComplete" */ "@/views/LogoutCompleteView.vue"
    );
const LogoutView = () =>
    import(/* webpackChunkName: "logout" */ "@/views/LogoutView.vue");
const NotFoundView = () =>
    import(
        /* webpackChunkName: "notFound" */ "@/views/errors/NotFoundView.vue"
    );
const UnauthorizedView = () =>
    import(
        /* webpackChunkName: "unauthorized" */ "@/views/errors/UnauthorizedView.vue"
    );
const PatientRetrievalErrorView = () =>
    import(
        /* webpackChunkName: "patientRetrievalError" */ "@/views/errors/PatientRetrievalErrorView.vue"
    );
const ReleaseNotesView = () =>
    import(
        /* webpackChunkName: "releaseNotes" */ "@/views/ReleaseNotesView.vue"
    );
const ReportsView = () =>
    import(/* webpackChunkName: "reports" */ "@/views/ReportsView.vue");
const ServicesView = () =>
    import(/* webpackChunkName: "services" */ "@/views/ServicesView.vue");
const UserTimelineView = () =>
    import(/* webpackChunkName: "timeline" */ "@/views/UserTimelineView.vue");

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
        path: Path.Logout,
        name: "Logout",
        component: LogoutView,
        meta: { stateless: true },
    },
    {
        path: Path.LogoutComplete,
        name: "LogoutComplete",
        component: LogoutCompleteView,
        meta: { stateless: true },
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
        path: Path.PatientRetrievalError,
        component: PatientRetrievalErrorView,
        meta: {
            validStates: [UserState.noPatient],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: Path.Unauthorized,
        component: UnauthorizedView,
        meta: { stateless: true },
    },
    {
        path: Path.NotFound,
        component: NotFoundView,
        meta: { stateless: true },
    },
    {
        path: Path.ReleaseNotes,
        name: "ReleaseNotes",
        component: ReleaseNotesView,
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
        path: Path.Timeline,
        component: UserTimelineView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: "/*",
        redirect: "/not-found",
    }, // Not found; Will catch all other paths not covered previously
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

const router = createRouter({
    history: createWebHistory(process.env.BASE_URL),
    routes,
    scrollBehavior: scrollBehaviour,
});

router.beforeEach(beforeEachGuard);

router.afterEach(afterEachHook);
export default router;
