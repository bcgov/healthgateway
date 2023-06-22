// Composables
import { createRouter, createWebHistory } from "vue-router";

const App = import("@/App.vue");
const LandingView = () =>
    import(/* webpackChunkName: "landing" */ "@/views/LandingView.vue");

const routes = [
    {
        path: "/",
        name: "Landing",
        component: LandingView,
    },
];

const router = createRouter({
    history: createWebHistory(process.env.BASE_URL),
    routes,
});

export default router;
