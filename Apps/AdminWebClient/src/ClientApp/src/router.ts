import Vue from "vue";
import VueRouter, { Route } from "vue-router";

import store from "@/store/store";
import CommunicationView from "@/views/Communication.vue";
import DashboardView from "@/views/Dashboard.vue";
import FeedbackView from "@/views/Feedback.vue";
import LoginView from "@/views/Login.vue";
import LogoutView from "@/views/Logout.vue";
import ResendEmailView from "@/views/ResendEmail.vue";
import StatsView from "@/views/Stats.vue";
import SupportView from "@/views/Support.vue";
import UnauthorizedView from "@/views/Unauthorized.vue";

import { UserRoles } from "./constants/userRoles";

Vue.use(VueRouter);

const routes = [
    {
        path: "/",
        name: "Dashboard",
        component: DashboardView,
        meta: {
            requiresAuth: true,
            validRoles: [UserRoles.Admin, UserRoles.Reviewer],
        },
    },
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
        path: "/job-scheduler",
        name: "JobScheduler",
        meta: { requiresAuth: true, validRoles: [UserRoles.Admin] },
        beforeEnter() {
            location.href =
                store.getters["config/serviceEndpoints"]["JobScheduler"];
        },
    },
    {
        path: "/admin-email",
        name: "Resend Emails",
        component: ResendEmailView,
        meta: { requiresAuth: true, validRoles: [UserRoles.Admin] },
    },
    {
        path: "/user-feedback",
        name: "User Feedback list",
        component: FeedbackView,
        meta: {
            requiresAuth: true,
            validRoles: [UserRoles.Admin, UserRoles.Reviewer],
        },
    },
    {
        path: "/communication",
        name: "System Communications",
        component: CommunicationView,
        meta: { requiresAuth: true, validRoles: [UserRoles.Admin] },
    },
    {
        path: "/stats",
        name: "System Analytics",
        component: StatsView,
        meta: { requiresAuth: true, validRoles: [UserRoles.Admin] },
    },
    {
        path: "/support",
        name: "Support",
        component: SupportView,
        meta: { requiresAuth: true, validRoles: [UserRoles.Admin] },
        props: (route: Route) => ({
            hdid: route.query.hdid,
        }),
    },
    {
        path: "/unauthorized",
        name: "Unauthorized",
        component: UnauthorizedView,
        meta: { requiresAuth: false },
    },
    { path: "*", redirect: "/" },
];

const router = new VueRouter({
    mode: "history",
    base: process.env.BASE_URL,
    routes,
});

router.beforeEach(async (to, from, next) => {
    if (to.meta.requiresAuth) {
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
                        !to.meta.validRoles ||
                        to.meta.validRoles.includes(userRole)
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
