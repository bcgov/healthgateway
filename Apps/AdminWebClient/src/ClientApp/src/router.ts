import Vue from "vue";
import store from "@/store/store";
import LoginView from "@/views/Login.vue";
import LogoutView from "@/views/Logout.vue";
import DashboardView from "@/views/Dashboard.vue";
import BetaQueueView from "@/views/BetaQueue.vue";
import ResendEmailView from "@/views/ResendEmail.vue";
import VueRouter from "vue-router";
import FeedbackView from "@/views/Feedback.vue";
import UnauthorizedView from "@/views/Unauthorized.vue";
import CommunicationView from "./views/Communication.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Dashboard",
    component: DashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: "/signin",
    name: "Login",
    component: LoginView,
    meta: { requiresAuth: false }
  },
  {
    path: "/signoff",
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
  {
    path: "/admin-email",
    name: "Resend Emails",
    component: ResendEmailView,
    meta: { requiresAuth: true }
  },
  {
    path: "/user-feedback",
    name: "User Feedback list",
    component: FeedbackView,
    meta: { requiresAuth: true }
  },
  {
    path: "/communication",
    name: "System Communications",
    component: CommunicationView,
    meta: { requiresAuth: true }
  },
  {
    path: "/unauthorized",
    name: "Unauthorized",
    component: UnauthorizedView,
    meta: { requiresAuth: false }
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
      next({ path: "/signin", query: { redirect: to.path } });
    } else {
      let isAuthorized = store.getters["auth/isAuthorized"];
      if (!isAuthorized) {
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
