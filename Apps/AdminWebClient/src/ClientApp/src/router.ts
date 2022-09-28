import Vue from "vue";
import VueRouter, { Route } from "vue-router";

import { UserRoles } from "@/constants/userRoles";
import store from "@/store/store";
import CovidCardView from "@/views/CovidCard.vue";
import LoginView from "@/views/Login.vue";
import LogoutView from "@/views/Logout.vue";
import SupportView from "@/views/Support.vue";
import UnauthorizedView from "@/views/Unauthorized.vue";

Vue.use(VueRouter);

const SUPPORT_PATH = "/support";
const UNAUTHORIZED_PATH = "/unauthorized";
const VACCINE_CARD_PATH = "/covidcard";

const routes = [
    {
        path: "/Login",
        name: "Login",
        meta: { requiresAuth: false },
    },
    {
        path: "/signin",
        name: "Signin",
        component: LoginView,
        meta: { requiresAuth: false },
    },
    {
        path: "/signoff",
        name: "Logout",
        component: LogoutView,
        meta: { requiresAuth: false },
    },
    {
        path: SUPPORT_PATH,
        name: "Support",
        component: SupportView,
        meta: {
            requiresAuth: true,
            validRoles: [UserRoles.Reviewer, UserRoles.Admin],
        },
        props: (route: Route) => ({
            hdid: route.query.hdid,
        }),
    },
    {
        path: VACCINE_CARD_PATH,
        name: "BC Vaccine Card",
        component: CovidCardView,
        meta: {
            requiresAuth: true,
            validRoles: [UserRoles.SupportUser],
        },
    },
    {
        path: UNAUTHORIZED_PATH,
        name: "Unauthorized",
        component: UnauthorizedView,
        meta: { requiresAuth: false },
    },
    { path: "/", meta: { requiresAuth: true } },
    { path: "*", redirect: UNAUTHORIZED_PATH, meta: { requiresAuth: false } },
];

const router = new VueRouter({
    mode: "history",
    base: process.env.BASE_URL,
    routes,
});

router.beforeEach(async (to, _from, next) => {
    const meta = to.meta;
    if (meta === undefined) {
        next(Error("Route meta property is undefined"));
        return;
    }

    // allow access to route if it doesn't require authentication
    if (!meta.requiresAuth) {
        next();
        return;
    }

    // redirect to sign in page if not authenticated (and accessing route that require authentication)
    const isAuthenticated = store.getters["auth/isAuthenticated"];
    if (!isAuthenticated) {
        next({ path: "/signin", query: { redirect: to.path } });
        return;
    }

    const isAuthorized = store.getters["auth/isAuthorized"];
    const userRoles: string[] = store.getters["auth/roles"];
    const hasValidRole =
        !meta.validRoles ||
        meta.validRoles.some((role: string) => userRoles.includes(role));

    // redirect to unauthorized page if user does not have a role assigned that allows access to the route
    if (!isAuthorized || !hasValidRole) {
        next({ path: UNAUTHORIZED_PATH });
        return;
    }

    // redirect to an appropriate default page for the user based on their assigned roles when accessing the site root
    if (to.path === "/") {
        let defaultPath = UNAUTHORIZED_PATH;
        if (userRoles.includes(UserRoles.SupportUser)) {
            defaultPath = VACCINE_CARD_PATH;
        } else if (
            userRoles.includes(UserRoles.Reviewer) ||
            userRoles.includes(UserRoles.Admin)
        ) {
            defaultPath = SUPPORT_PATH;
        }
        next({ path: defaultPath });
        return;
    }

    // allow access to the route
    next();
});

export default router;
