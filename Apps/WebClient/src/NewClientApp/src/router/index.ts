import {
    createRouter,
    createWebHistory,
    RouteLocationNormalized,
} from "vue-router";
import { beforeEachGuard } from "@/router/beforeEachGuard";
import { afterEachHook } from "@/router/afterEachHook";

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

export enum RouterPath {
    ACCEPT_TERMS_OF_SERVICE_PATH = "/acceptTermsOfService",
    HOME_PATH = "/home",
    IDIR_LOGGED_IN_PATH = "/idirLoggedIn",
    LOGIN_PATH = "/login",
    LOGIN_CALLBACK_PATH = "/loginCallback",
    LOGOUT_PATH = "/logout",
    LOGOUT_COMPLETE_PATH = "/logoutComplete",
    PATIENT_RETRIEVAL_ERROR_PATH = "/patientRetrievalError",
    PROFILE_PATH = "/profile",
    REGISTRATION_PATH = "/registration",
    ROOT_PATH = "/",
    TIMELINE_PATH = "/timeline",
    UNAUTHORIZED_PATH = "/unauthorized",
    NOT_FOUND_PATH = "/not-found",
    QUEUE_PATH = "/queue",
    QUEUE_FULL_PATH = "/busy",
}

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
        path: RouterPath.ROOT_PATH,
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
        path: RouterPath.HOME_PATH,
        name: "Home",
        component: HomeView,
        meta: {
            validStates: [UserState.registered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: RouterPath.LOGIN_PATH,
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
        path: RouterPath.LOGIN_CALLBACK_PATH,
        name: "LoginCallback",
        component: LoginCallbackView,
        meta: {
            routeIsOidcCallback: true,
            stateless: true,
        },
    },
    {
        path: RouterPath.LOGOUT_PATH,
        name: "Logout",
        component: LogoutView,
        meta: { stateless: true },
    },
    {
        path: RouterPath.LOGOUT_COMPLETE_PATH,
        name: "LogoutComplete",
        component: LogoutCompleteView,
        meta: { stateless: true },
    },
    {
        path: RouterPath.REGISTRATION_PATH,
        name: "Registration",
        component: RegistrationView,
        meta: {
            validStates: [UserState.notRegistered],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: RouterPath.PATIENT_RETRIEVAL_ERROR_PATH,
        component: PatientRetrievalErrorView,
        meta: {
            validStates: [UserState.noPatient],
            requiresProcessedWaitlistTicket: true,
        },
    },
    {
        path: RouterPath.UNAUTHORIZED_PATH,
        component: UnauthorizedView,
        meta: { stateless: true },
    },
    {
        path: RouterPath.NOT_FOUND_PATH,
        component: NotFoundView,
        meta: { stateless: true },
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
