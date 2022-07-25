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
        path: "/support",
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
        path: "/covidcard",
        name: "BC Vaccine Card",
        component: CovidCardView,
        meta: {
            requiresAuth: true,
            validRoles: [
                UserRoles.SupportUser,
                UserRoles.Reviewer,
                UserRoles.Admin,
            ],
        },
    },
    {
        path: "/unauthorized",
        name: "Unauthorized",
        component: UnauthorizedView,
        meta: { requiresAuth: false },
    },
    { path: "*", redirect: "/covidcard" },
];

const router = new VueRouter({
    mode: "history",
    base: process.env.BASE_URL,
    routes,
});

router.beforeEach(async (to, from, next) => {
    const meta = to.meta;
    if (meta === undefined) {
        next(Error("Route meta property is undefined"));
        return;
    }

    if (meta.requiresAuth) {
        const isAuthenticated = store.getters["auth/isAuthenticated"];
        if (!isAuthenticated) {
            next({ path: "/signin", query: { redirect: to.path } });
        } else {
            const isAuthorized = store.getters["auth/isAuthorized"];
            const userRoles: string[] = store.getters["auth/roles"];
            if (
                !isAuthorized ||
                !userRoles.some(
                    (userRole) =>
                        !meta.validRoles || meta.validRoles.includes(userRole)
                )
            ) {
                next({ path: "/unauthorized" });
            } else {
                next();
            }
        }
    } else {
        next();
    }
});

export default router;
