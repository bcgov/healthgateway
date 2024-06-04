import {
    createRouter,
    createWebHistory,
    RouteLocationNormalized,
} from "vue-router";

import { Path } from "@/constants/path";
import { afterEachHook } from "@/router/afterEachHook";

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

    router.afterEach(afterEachHook);
    return router;
}

export default initializeRouter;
