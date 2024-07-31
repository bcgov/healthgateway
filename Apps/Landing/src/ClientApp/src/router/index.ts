import {
    createRouter,
    createWebHistory,
    RouteLocationNormalized,
} from "vue-router";

import { Path } from "@/constants/path";
import { afterEachHook } from "@/router/afterEachHook";
import { beforeEachGuard } from "@/router/beforeEachGuard";

const LandingView = () =>
    import(
        /* webpackChunkName: "landing" */ "@/components/public/landing/LandingView.vue"
    );
const NotFoundView = () =>
    import(
        /* webpackChunkName: "notFound" */ "@/components/error/NotFoundView.vue"
    );
const TermsOfServiceView = () =>
    import(
        /* webpackChunkName: "termsOfService" */ "@/components/public/terms-of-service/TermsOfServiceView.vue"
    );

const routes = [
    {
        path: Path.Root,
        name: "Landing",
        component: LandingView,
    },
    {
        path: Path.NotFound,
        component: NotFoundView,
    },
    {
        path: Path.TermsOfService,
        component: TermsOfServiceView,
    },
    {
        path: Path.Covid19,
        component: NotFoundView,
        meta: {
            redirectPath: "s/",
        },
    },
    {
        path: Path.Dependents,
        component: NotFoundView,
        meta: {
            redirectPath: "s/dependents",
        },
    },
    {
        path: Path.Home,
        component: NotFoundView,
        meta: {
            redirectPath: "s/",
        },
    },
    {
        path: Path.Login,
        component: NotFoundView,
        meta: {
            redirectPath: "s/",
        },
    },
    {
        path: Path.Profile,
        component: NotFoundView,
        meta: {
            redirectPath: "s/profile",
        },
    },
    {
        path: Path.Registration,
        component: NotFoundView,
        meta: {
            redirectPath: "s/",
        },
    },
    {
        path: Path.Reports,
        component: NotFoundView,
        meta: {
            redirectPath: "s/export-records",
        },
    },
    {
        path: Path.Services,
        component: NotFoundView,
        meta: {
            redirectPath: "s/service",
        },
    },
    {
        path: Path.Timeline,
        component: NotFoundView,
        meta: {
            redirectPath: "s/timeline",
        },
    },
    {
        path: Path.ValidateEmail + "/:inviteKey",
        component: NotFoundView,
        meta: {
            redirectPath: "s/ValidateEmail",
        },
    },
    {
        path: Path.VaccineCard,
        component: NotFoundView,
        meta: {
            classicRedirectPath: "vaccinecard",
        },
    },
    {
        path: "/:pathMatch(.*)*", // will catch all other paths not covered previously
        redirect: Path.NotFound,
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
        return { top: 0 };
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
