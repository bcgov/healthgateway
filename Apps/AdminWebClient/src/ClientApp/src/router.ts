import Vue from "vue";
import store from "@/store/store";
import LoginView from "@/views/Login.vue";
import LogoutView from "@/views/Logout.vue";
import DashboardView from "@/views/Dashboard.vue";
import BetaQueueView from "@/views/BetaQueue.vue";
import VueRouter from "vue-router";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Dashboard",
    component: DashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: "/login",
    name: "Login",
    component: LoginView,
    meta: { requiresAuth: false }
  },
  {
    path: "/logout",
    name: "Logout",
    component: LogoutView,
    meta: { requiresAuth: false }
  },
  {
    path: "/job-scheduler",
    name: "JobScheduler",
    meta: { requiresAuth: true },
    beforeEnter() {
      location.href = store.getters["config/serviceEndpoints"]["JobScheduler"];
    }
  },
  {
    path: "/beta-invites",
    name: "Beta user list",
    component: BetaQueueView,
    meta: { requiresAuth: true }
  },
  { path: "*", redirect: "/" }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

router.beforeEach(async (to, from, next) => {
  if (to.meta.requiresAuth) {
    let isAuthenticated = store.getters["auth/isAuthenticated"];
    if (!isAuthenticated) {
      next({ path: "/login", query: { redirect: to.path } });
    } else {
      next();
    }
  } else {
    next();
  }
});

export default router;
